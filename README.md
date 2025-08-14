# 📖 BookLocal

BookLocal to w pełni funkcjonalna aplikacja webowa typu SPA (Single Page Application) z dedykowanym backendem, stworzona do rezerwacji wizyt w lokalnych firmach usługowych. Umożliwia klientom wyszukiwanie i rezerwowanie usług, a właścicielom firm kompleksowe zarządzanie swoim biznesem, pracownikami, grafikami i rezerwacjami.

---

## 🚀 Podgląd na żywo

- **Aplikacja Frontendowa:** <https://wonderful-pebble-00b01fe03.2.azurestaticapps.net>
- **Dokumentacja API (Swagger):** <https://booklocal-api-gja0begeg4gfbfcj.polandcentral-01.azurewebsites.net/swagger>

---

## ✨ Główne Funkcje

- **Wyszukiwanie i filtrowanie:** Zaawansowane wyszukiwanie firm usługowych z opcjami filtrowania i sortowania.
- **System Rezerwacji:** Intuicyjny kalendarz do rezerwacji i zarządzania wizytami.
- **Panel Zarządzania:** Dedykowany panel dla właścicieli firm do zarządzania usługami, pracownikami i grafikami.
- **Uwierzytelnianie i Autoryzacja:** Bezpieczny system logowania oparty na rolach (klient, właściciel) przy użyciu tokenów JWT.
- **System Ocen i Opinii:** Możliwość dodawania ocen i komentarzy do zrealizowanych usług.
- **Zarządzanie Zdjęciami:** Przesyłanie i zarządzanie zdjęciami dla profili firmowych, zintegrowane z usługą Cloudinary.
- **Komunikacja w czasie rzeczywistym:** Czat między użytkownikami oraz system powiadomień, zaimplementowane przy użyciu SignalR.

---

## 🛠️ Architektura i Stos Technologiczny

Aplikacja została zbudowana w architekturze rozproszonej, oddzielającej warstwę prezentacji (Frontend) od logiki biznesowej (Backend).

### **Frontend**
Zbudowany jako Single Page Application (SPA), co zapewnia płynne i szybkie działanie bez przeładowywania strony.
- **Framework:** **Angular** – dojrzały i wydajny framework do budowania dynamicznych interfejsów użytkownika.
- **Język:** **TypeScript** – dla bezpieczeństwa typów i lepszej skalowalności kodu.
- **Wdrożenie:** **Azure Static Web Apps** – usługa idealnie dopasowana do hostowania nowoczesnych aplikacji frontendowych, zintegrowana z globalną siecią CDN.
- **CI/CD:** **GitHub Actions** – proces budowania i wdrażania jest w pełni zautomatyzowany po każdym `push` do głównej gałęzi repozytorium.

### **Backend**
Wydajne i skalowalne API RESTowe, które dostarcza dane i obsługuje całą logikę biznesową aplikacji.
- **Framework:** **.NET (ASP.NET Core)** – do budowy szybkiego i bezpiecznego Web API.
- **Dostęp do Danych:** **Entity Framework Core** – nowoczesny ORM (Object-Relational Mapper) do komunikacji z bazą danych.
- **Uwierzytelnianie:** **ASP.NET Core Identity z JWT** – standard branżowy do zabezpieczania endpointów API.
- **Komunikacja Real-time:** **SignalR** – do implementacji dwukierunkowej komunikacji (np. w czacie).
- **Wdrożenie:** **Azure App Service** – niezawodna platforma do hostowania aplikacji webowych .NET.

### **Baza Danych**
- **Silnik:** **Azure SQL Database** – w pełni zarządzana, relacyjna baza danych w chmurze Microsoft.
- **Zasilanie Danych:** **Bogus** – biblioteka używana do generowania realistycznych, przykładowych danych na potrzeby deweloperskie.

### **Usługi Zewnętrzne**
- **Przechowywanie Zdjęć:** **Cloudinary** – platforma do zarządzania mediami, używana do hostowania i serwowania zdjęć.

---

## ⚙️ Uruchomienie Lokalne

### Wymagania wstępne

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/)
- [Angular CLI](https://angular.io/cli) (`npm install -g @angular/cli`)
- Lokalna instancja SQL Server (np. SQL Server Express)

### Backend

1. Otwórz `BookLocal.sln` w Visual Studio.
2. W pliku `BookLocal.API/appsettings.Development.json` uzupełnij sekcje `ConnectionStrings`, `Jwt` i `CloudinarySettings`.
3. Uruchom projekt `BookLocal.API` (klawisz F5). Spowoduje to również wypełnienie bazy danych przykładowymi danymi.

### Frontend

1. Przejdź do folderu `booklocal-frontend`:
   ```bash
   cd booklocal-frontend
   ```
2. Zainstaluj zależności:
   ```bash
   npm install
   ```
3. W pliku `src/environments/environment.ts` upewnij się, że `apiUrl` wskazuje na adres Twojego lokalnego API (np. `https://localhost:5001/api`).
4. Uruchom serwer deweloperski:
   ```bash
   ng serve
   ```
5. Otwórz w przeglądarce `http://localhost:4200`.

---

## 📄 License

Ten projekt jest licencjonowany na warunkach licencji MIT - zobacz plik [LICENSE](LICENSE) po szczegóły.

