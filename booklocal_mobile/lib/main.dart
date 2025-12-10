import 'package:booklocal_mobile/features/auth/login_screen.dart';
import 'core/services/auth_service.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

void main() {
  runApp(
    ChangeNotifierProvider(
      create: (_) => AuthService(),
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
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF15803d)), // Twój zielony
      ),
      // Consumer nasłuchuje zmian w AuthService
      home: Consumer<AuthService>(
        builder: (context, auth, _) {
          // Logika Routingowa:
          // Jeśli jest zalogowany -> Pokaż Pulpit (placeholder na razie)
          // Jeśli nie -> Pokaż Logowanie
          return auth.isAuthenticated 
              ? const Scaffold(body: Center(child: Text("ZALOGOWANO! Tutaj będzie Dashboard"))) 
              : const LoginScreen();
        },
      ),
    );
  }
}
