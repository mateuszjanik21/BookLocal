import 'package:booklocal_mobile/core/services/chat_services.dart';
import 'package:booklocal_mobile/core/services/reservation_service.dart';
import 'package:booklocal_mobile/core/services/review_service.dart';
import 'package:booklocal_mobile/core/services/service_bundle_service.dart';
import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:provider/provider.dart';
import 'core/services/auth_service.dart';
import 'core/services/client_service.dart';
import 'features/auth/login_screen.dart';
import 'features/client/main_screen.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'core/services/favorites_service.dart';
import 'features/client/favorites/providers/favorites_provider.dart';
import 'features/client/chat/providers/chat_provider.dart';
import 'core/services/presence_service.dart';
import 'features/client/profile/providers/profile_provider.dart';

final GlobalKey<ScaffoldMessengerState> globalMessengerKey = GlobalKey<ScaffoldMessengerState>();
final GlobalKey<NavigatorState> globalNavigatorKey = GlobalKey<NavigatorState>();

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await initializeDateFormatting('pl_PL', null);
  runApp(
    MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => AuthService()),
        Provider(create: (_) => ClientService()),
        // ChatService — SINGLETON, tworzony RAZ
        Provider<ChatService>(
          create: (context) => ChatService(
            Provider.of<AuthService>(context, listen: false),
          ),
        ),
        // PresenceService — SINGLETON ChangeNotifier, tworzony RAZ
        ChangeNotifierProvider<PresenceService>(
          create: (context) => PresenceService(
            Provider.of<AuthService>(context, listen: false),
          ),
        ),
        // ChatProvider — SINGLETON ChangeNotifier, tworzony RAZ
        ChangeNotifierProvider<ChatProvider>(
          create: (context) => ChatProvider(
            Provider.of<ChatService>(context, listen: false),
          ),
        ),
        ChangeNotifierProxyProvider<AuthService, ProfileProvider>(
          create: (context) => ProfileProvider(Provider.of<AuthService>(context, listen: false)),
          update: (context, auth, previous) => previous ?? ProfileProvider(auth),
        ),
        ProxyProvider<AuthService, ServiceBundleService>(
          update: (_, auth, prev) => ServiceBundleService(auth),
        ),
        ProxyProvider<AuthService, ReservationService>(
          update: (_, auth, _) => ReservationService(auth),
        ),
        ProxyProvider<AuthService, ReviewService>(
          update: (_, auth, _) => ReviewService(auth),
        ),
        ProxyProvider<AuthService, FavoritesService>(
          update: (_, auth, _) => FavoritesService(auth),
        ),
        ChangeNotifierProxyProvider<FavoritesService, FavoritesProvider>(
          create: (context) => FavoritesProvider(Provider.of<FavoritesService>(context, listen: false)),
          update: (context, service, previous) => previous!..updateService(service),
        ),
      ],
      child: const BookLocalApp(),
    ),
  );
}

class BookLocalApp extends StatelessWidget {
  const BookLocalApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'BookLocal',
      debugShowCheckedModeBanner: false,
      navigatorKey: globalNavigatorKey,
      scaffoldMessengerKey: globalMessengerKey,
      localizationsDelegates: const [
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      supportedLocales: const [
        Locale('pl'),
        Locale('en'),
      ],
      locale: const Locale('pl'),
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF16a34a)),
        scaffoldBackgroundColor: Colors.white,
      ),
      home: Consumer<AuthService>(
        builder: (context, auth, _) {
          if (auth.isAuthenticated) {
            return const _AuthenticatedGate();
          }
          return const LoginScreen();
        },
      ),
    );
  }
}

/// Brama po zalogowaniu — uruchamia PresenceHub RAZ.
class _AuthenticatedGate extends StatefulWidget {
  const _AuthenticatedGate();

  @override
  State<_AuthenticatedGate> createState() => _AuthenticatedGateState();
}

class _AuthenticatedGateState extends State<_AuthenticatedGate> {
  bool _initialized = false;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) async {
      if (!_initialized) {
        _initialized = true;

        // Celowe opóźnienie wywołania Hub'a SignalR oraz wiadomości,
        // Zapobiega to "wąskiemu gardłu" sieci podczas logowania,
        // dając najwyższy priorytet na wyrenderowanie i pobranie ekranu Home.
        await Future.delayed(const Duration(milliseconds: 800));

        if (!mounted) return;

        // Uruchom PresenceHub — dokładnie jak Angular createHubConnection() przy starcie
        final presence = Provider.of<PresenceService>(context, listen: false);
        presence.createHubConnection();

        // Załaduj listę konwersacji dla badge'a
        final chatProvider = Provider.of<ChatProvider>(context, listen: false);
        chatProvider.loadMyConversations();

        print('[Main] ✅ PresenceHub uruchomiony globalnie po opóźnieniu optymalizacyjnym');
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return const MainScreen();
  }
}