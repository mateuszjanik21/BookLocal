import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/services/auth_service.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> with SingleTickerProviderStateMixin {
  final _formKey = GlobalKey<FormState>();
  
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  
  bool _isLoading = false;
  
  late AnimationController _animController;
  late Animation<double> _fadeAnimation;
  late Animation<Offset> _slideAnimation;

  final Color colorPrimary = const Color(0xFF16a34a);
  final Color colorDark = const Color(0xFF15803d);
  final Color colorAccent = const Color(0xFF4ade80);
  final Color colorInputBg = const Color(0xFFF8FAFC);

  @override
  void initState() {
    super.initState();
    
    _animController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 700),
    );

    _fadeAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(parent: _animController, curve: Curves.easeOut),
    );

    _slideAnimation = Tween<Offset>(begin: const Offset(0, 0.1), end: Offset.zero).animate(
      CurvedAnimation(parent: _animController, curve: Curves.easeOut),
    );

    _animController.forward();
  }

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    _animController.dispose();
    super.dispose();
  }

  Future<void> _onSubmit() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() => _isLoading = true);

    final authService = Provider.of<AuthService>(context, listen: false);
    
    final success = await authService.login(
      _emailController.text.trim(),
      _passwordController.text,
    );

    setState(() => _isLoading = false);

    if (success) {
      if (!mounted) return;

      final user = authService.currentUser;
      
      if (user != null && user.isOwner) {
        
      } else {
        
      }
    } else {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: const Text('Nieprawidłowy email lub hasło.'),
          backgroundColor: Colors.red.shade400,
          behavior: SnackBarBehavior.floating,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Container(
        width: double.infinity,
        height: double.infinity,
        decoration: BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
            colors: [
              colorDark, 
              colorPrimary, 
              colorAccent, 
              colorDark, 
            ],
          ),
        ),
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(20),
            child: FadeTransition(
              opacity: _fadeAnimation,
              child: SlideTransition(
                position: _slideAnimation,
                child: Card(
                  elevation: 10, 
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(16), 
                  ),
                  clipBehavior: Clip.antiAlias,
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Container(
                        width: double.infinity,
                        padding: const EdgeInsets.all(32),
                        color: colorPrimary, 
                        child: Column(
                          children: [
                            const Icon(
                              Icons.verified_user_outlined,
                              size: 64,
                              color: Colors.white,
                            ),
                            const SizedBox(height: 16),
                            const Text(
                              "Cieszymy się, że jesteś!",
                              style: TextStyle(
                                fontSize: 24,
                                fontWeight: FontWeight.bold,
                                color: Colors.white,
                              ),
                              textAlign: TextAlign.center,
                            ),
                            const SizedBox(height: 8),
                            const Text(
                              "Zaloguj się, by zarządzać swoimi rezerwacjami i odkrywać nowe usługi.",
                              style: TextStyle(
                                fontSize: 14,
                                color: Colors.white70, 
                              ),
                              textAlign: TextAlign.center,
                            ),
                          ],
                        ),
                      ),

                      Padding(
                        padding: const EdgeInsets.all(32),
                        child: Form(
                          key: _formKey,
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              const Text(
                                "Zaloguj się",
                                style: TextStyle(
                                  fontSize: 28,
                                  fontWeight: FontWeight.bold,
                                  color: Colors.black87,
                                ),
                              ),
                              const SizedBox(height: 24),

                              const Text("Adres Email", style: TextStyle(fontWeight: FontWeight.w500)),
                              const SizedBox(height: 8),
                              TextFormField(
                                controller: _emailController,
                                keyboardType: TextInputType.emailAddress,
                                decoration: _inputDecoration("twoj.email@przyklad.com"),
                                validator: (value) {
                                  if (value == null || value.isEmpty || !value.contains('@')) {
                                    return 'Wprowadź poprawny adres email.';
                                  }
                                  return null;
                                },
                              ),

                              const SizedBox(height: 16),

                              const Text("Hasło", style: TextStyle(fontWeight: FontWeight.w500)),
                              const SizedBox(height: 8),
                              TextFormField(
                                controller: _passwordController,
                                obscureText: true,
                                decoration: _inputDecoration("••••••••"),
                                validator: (value) {
                                  if (value == null || value.isEmpty) {
                                    return 'Hasło jest wymagane.';
                                  }
                                  return null;
                                },
                              ),

                              const SizedBox(height: 32),

                              SizedBox(
                                width: double.infinity,
                                height: 50,
                                child: ElevatedButton(
                                  onPressed: _isLoading ? null : _onSubmit,
                                  style: ElevatedButton.styleFrom(
                                    backgroundColor: colorPrimary,
                                    foregroundColor: Colors.white,
                                    elevation: 2,
                                    shape: RoundedRectangleBorder(
                                      borderRadius: BorderRadius.circular(8),
                                    ),
                                  ),
                                  child: _isLoading
                                      ? const SizedBox(
                                          width: 24, 
                                          height: 24, 
                                          child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2)
                                        )
                                      : const Text("Zaloguj", style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                                ),
                              ),

                              const SizedBox(height: 24),

                              Center(
                                child: GestureDetector(
                                  onTap: () {
                                    // TODO: Nawigacja do rejestracji
                                  },
                                  child: RichText(
                                    text: TextSpan(
                                      text: "Nie masz jeszcze konta? ",
                                      style: TextStyle(color: Colors.grey[600], fontSize: 14),
                                      children: [
                                        TextSpan(
                                          text: "Zarejestruj się",
                                          style: TextStyle(
                                            color: colorPrimary,
                                            fontWeight: FontWeight.bold,
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  InputDecoration _inputDecoration(String hint) {
    return InputDecoration(
      hintText: hint,
      filled: true,
      fillColor: colorInputBg,
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(8),
        borderSide: BorderSide(color: Colors.grey.shade300),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(8),
        borderSide: BorderSide(color: Colors.grey.shade300), 
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(8),
        borderSide: BorderSide(color: colorPrimary, width: 2),
      ),
      errorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(8),
        borderSide: const BorderSide(color: Colors.red),
      ),
    );
  }
}