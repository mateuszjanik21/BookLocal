import 'package:flutter/material.dart';
import '../main_screen.dart'; // Upewnij się, że ścieżka do MainScreen jest poprawna

class BookingSuccessScreen extends StatelessWidget {
  const BookingSuccessScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      body: Center(
        child: Padding(
          padding: const EdgeInsets.all(30.0),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              // 1. Animacja lub Ikona Sukcesu
              Container(
                padding: const EdgeInsets.all(20),
                decoration: BoxDecoration(
                  color: const Color(0xFF16a34a).withOpacity(0.1),
                  shape: BoxShape.circle,
                ),
                child: const Icon(Icons.check_circle, size: 80, color: Color(0xFF16a34a)),
              ),
              const SizedBox(height: 30),
              
              // 2. Teksty
              const Text(
                "Rezerwacja udana!",
                style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 10),
              const Text(
                "Twoja wizyta została zaplanowana.\nMożesz sprawdzić jej status w zakładce 'Moje Wizyty'.",
                textAlign: TextAlign.center,
                style: TextStyle(color: Colors.grey, fontSize: 16),
              ),
              
              const SizedBox(height: 50),

              // 3. Przyciski
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  onPressed: () {
                    // POPRAWKA: Nawigujemy do MainScreen z indeksem 2 (Wizyty)
                    // Dzięki temu załaduje się pasek menu i otworzy odpowiednia zakładka
                    Navigator.of(context).pushAndRemoveUntil(
                      MaterialPageRoute(
                        builder: (context) => const MainScreen(initialIndex: 2), 
                      ),
                      (route) => false,
                    );
                  },
                  // ... styl przycisku bez zmian ...
                  child: const Text("Zobacz moje wizyty", style: TextStyle(fontSize: 16)),
                ),
              ),
              const SizedBox(height: 15),
              TextButton(
                onPressed: () {
                   // Wróć do ekranu głównego (zakładka Home to index 0)
                   Navigator.of(context).pushAndRemoveUntil(
                      MaterialPageRoute(builder: (context) => const MainScreen(initialIndex: 0)), 
                      (route) => false,
                    );
                },
                child: const Text("Wróć do strony głównej", style: TextStyle(color: Colors.grey)),
              ),
            ],
          ),
        ),
      ),
    );
  }
}