import 'package:booklocal_mobile/core/services/chat_services.dart';
import 'package:booklocal_mobile/core/services/reservation_service.dart';
import 'package:booklocal_mobile/core/services/review_service.dart';
import 'package:booklocal_mobile/core/services/service_bundle_service.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'core/services/auth_service.dart';
import 'core/services/client_service.dart';
import 'features/auth/login_screen.dart';
import 'features/client/main_screen.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'core/services/favorites_service.dart';
import 'features/client/favorites/providers/favorites_provider.dart';
import 'features/client/chat/providers/chat_provider.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await initializeDateFormatting('pl_PL', null);
  runApp(
    MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => AuthService()),
        Provider(create: (_) => ClientService()),
        Provider(create: (_) => ReviewService()),
        Provider(create: (_) => ServiceBundleService()),
        ProxyProvider<AuthService, ChatService>(
          update: (_, auth, _) => ChatService(auth),
        ),
        ChangeNotifierProxyProvider<ChatService, ChatProvider>(
          create: (context) => ChatProvider(Provider.of<ChatService>(context, listen: false)),
          update: (context, service, previous) => previous!,
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
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF16a34a)),
        scaffoldBackgroundColor: Colors.white,
      ),
      home: Consumer<AuthService>(
        builder: (context, auth, _) {
          if (auth.isAuthenticated) {
            return const MainScreen();
          }
          return const LoginScreen();
        },
      ),
    );
  }
}