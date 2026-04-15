#  BookLocal

**BookLocal** to rozbudowana, wielomodułowa aplikacja webowa do rezerwacji wizyt w lokalnych firmach usługowych. Projekt obejmuje pełen ekosystem: od aplikacji klienckiej (**Angular SPA**), przez serwer API (**ASP.NET Core**), aż po aplikację mobilną (**Flutter**). System obsługuje trzy role użytkowników – **Klient**, **Właściciel firmy** i **Super Administrator** – każda z dedykowanym zestawem funkcjonalności.

---

##  Podgląd na żywo

| Zasób | Link |
|---|---|
| **Aplikacja Web** | [wonderful-pebble-00b01fe03.2.azurestaticapps.net](https://wonderful-pebble-00b01fe03.2.azurestaticapps.net) |
| **Dokumentacja API (Swagger)** | [booklocal-api-…/swagger](https://booklocal-api-gja0begeg4gfbfcj.polandcentral-01.azurewebsites.net/swagger) |

> [!NOTE]
> Aplikacja hostowana jest na darmowym planie Azure – pierwsze ładowanie może potrwać kilka sekund.

---

##  Konta testowe

| Rola | Login | Hasło |
|---|---|---|
| **Super Administrator** | `admin@test.com` | `Admin123!` |
| **Właściciel firmy** | `owner@test.com` | `P@ssword1` |
| **Klient** | `customer@test.com` | `P@ssword1` |

Możesz również samodzielnie przejść przez proces rejestracji jako klient lub właściciel.

> [!WARNING]
> To publiczna aplikacja demonstracyjna – **nie rejestruj się prawdziwymi danymi osobowymi**. Używaj fikcyjnych informacji do testów.

---

##  Funkcjonalności

###  Panel Klienta

| Funkcjonalność | Opis |
|---|---|
| **Wyszukiwanie firm** | Zaawansowane wyszukiwanie z filtrami (kategoria, miasto, ocena) i sortowaniem |
| **Przeglądanie profili firm** | Lista usług z cenami, warianty usług, opinie klientów |
| **System rezerwacji** | Wybór pracownika → usługi → daty → godziny → forma płatności, z widokiem dostępnych slotów podzielonych na pory dnia |
| **Rezerwacja pakietów usług** | Możliwość zarezerwowania pakietu wielu usług jednocześnie |
| **Kody rabatowe** | Weryfikacja i stosowanie kodów rabatowych (procentowych i kwotowych) ze sprawdzaniem ważności |
| **Punkty lojalnościowe** | Zbieranie punktów za zakończone wizyty i wykorzystywanie ich jako zniżki przy kolejnych rezerwacjach (1 pkt = 1 PLN, min. 1 PLN do zapłaty) |
| **Płatności** | Obsługa płatności gotówką, kartą i online (Blik, Przelewy24, Apple Pay) |
| **Moje rezerwacje** | Lista rezerwacji z podziałem na nadchodzące/przeszłe, możliwość anulowania |
| **Ulubione firmy** | Dodawanie firm do ulubionych |
| **Opinie i oceny** | Dodawanie recenzji z oceną gwiazdkową po zakończonej wizycie |
| **Czat** | Komunikacja w czasie rzeczywistym z firmami (SignalR) |
| **Obecność online** | Status online/offline użytkowników z wskaźnikiem aktywności |
| **Profil użytkownika** | Edycja danych osobowych i zdjęcia profilowego |

###  Panel Właściciela Firmy (Dashboard)

| Moduł | Opis |
|---|---|
| **Dashboard Home** | Podsumowanie kluczowych wskaźników, nadchodzące rezerwacje |
| **Zarządzanie usługami** | CRUD usług z wariantami (Standard/Premium), cenami, czasem trwania i czasem sprzątania; archiwizacja usług |
| **Kategorie usług** | Tworzenie własnych podkategorii usług z przypisaniem do kategorii głównych |
| **Pakiety usług** | Tworzenie pakietów z wielu usług z ceną pakietową niższą niż suma składowych |
| **Zarządzanie pracownikami** | Dodawanie pracowników z: bio, specjalizacją, zdjęciem, certyfikatami; przypisywanie usług do pracowników |
| **Grafiki pracy** | Definiowanie grafików (7 dni/tydzień) z godzinami pracy, wyjątkami (urlopy, L4), automatyczne obliczanie dostępności |
| **Zarządzanie rezerwacjami** | Kalendarz rezerwacji, widok listy, zmiana statusów (potwierdzona/anulowana/zakończona/no-show), tworzenie rezerwacji w imieniu klienta (guest mode) |
| **Moduł HR** | Umowy o pracę (UoP, zlecenie, B2B), dane finansowe pracowników (stawka godzinowa, typ dojazdu, PIT-2), listy płac z automatycznym obliczaniem nagrodzenia |
| **Finanse** | Raporty dzienne z przychodami, prowizjami, strukturą płatności; wykresy i statystyki (nowi/powracający klienci, occupancy rate, top selling service) |
| **Faktury** | Generowanie faktur VAT z pozycjami, wartością netto/brutto/VAT, możliwość pobrania |
| **CRM – Zarządzanie klientami** | Profile klientów z historią wizyt, łączną kwotą wydatków, liczbą no-show, datą ostatniej wizyty |
| **Kody rabatowe** | Tworzenie i zarządzanie rabatami (procentowe/kwotowe), limity użyć, daty ważności, przypisanie do usług |
| **Program lojalnościowy** | Konfiguracja programu: kwota za 1 punkt, historia transakcji (naliczenia/wykorzystania), zarządzanie saldami klientów |
| **Zdjęcia** | Galeria firmowa – upload/usuwanie zdjęć (Cloudinary), zarządzanie okładką |
| **Subskrypcje** | Wybór planu (Free/Silver/Gold) z różnym limitem pracowników/usług, auto-renewal |
| **Powiadomienia** | Real-time powiadomienia o nowych rezerwacjach (SignalR) |
| **Szablony** | Zarządzanie szablonami (dodatkowy moduł konfiguracyjny) |

###  Panel Super Administratora

| Funkcjonalność | Opis |
|---|---|
| **Statystyki platformy** | Łączna liczba firm, nowe firmy w miesiącu, aktywne subskrypcje, przychód z planów, oczekujące weryfikacje |
| **Weryfikacja firm** | Przeglądanie zgłoszeń nowych firm, zatwierdzanie/odrzucanie z podaniem powodu |
| **Zarządzanie planami subskrypcji** | CRUD planów (nazwa, cena miesięczna/roczna, limity pracowników/usług, prowizja, zaawansowane raporty, narzędzia marketingowe) |

---

### Frontend (Web)
- **Framework:** Angular (standalone components, lazy loading)
- **Język:** TypeScript
- **Stylizacja:** Tailwind CSS + DaisyUI (UI component library)
- **Zarządzanie stanem:** RxJS, Reactive Forms
- **Komunikacja real-time:** SignalR Client
- **Hosting:** Azure Static Web Apps
- **CI/CD:** GitHub Actions (automatyczny deploy przy push do `master`)
- **Serwisy:** 33 dedykowane serwisy Angular (auth, business, reservation, chat, finance, HR, loyalty, invoice, itp.)

### Frontend (Mobile)
- **Framework:** Flutter
- **Język:** Dart
- **Platformy:** Android, iOS, Web, Windows, Linux, macOS

### Backend
- **Framework:** ASP.NET Core (.NET 8) Web API
- **ORM:** Entity Framework Core (Code-First)
- **Architektura:** Wzorzec warstwowy (Controllers + Services). Kontrolery obsługują żądania HTTP, a Serwisy realizują całą logikę biznesową.
- **Uwierzytelnianie:** ASP.NET Core Identity + JWT (Bearer tokens) z 3 rolami (`customer`, `owner`, `superadmin`)
- **Real-time:** SignalR – 3 huby:
  - `ChatHub` – czat między klientem a firmą
  - `NotificationHub` – powiadomienia o nowych rezerwacjach
  - `PresenceHub` – status obecności online/offline
- **Hosting:** Azure App Service
- **CI/CD:** GitHub Actions (automatyczny deploy na Azure)

### Baza danych
- **Silnik:** Azure SQL Database (relacyjna, w pełni zarządzana), SQL Server do developmentu
- **Modele:** 37 modeli domenowych (Business, Employee, Reservation, Service, Payment, Invoice, Loyalty, Review, Discount, Schedule, itp.)
- **Seeding:** Bogus (faker dla .NET) – generowanie realistycznych danych testowych (30 firm, 770+ klientów, tysiące rezerwacji)
- **Widoki SQL:** Procedury i widoki do raportów finansowych (`DailyEmployeePerformance`)

### Usługi zewnętrzne
- **Cloudinary** – hosting i zarządzanie zdjęciami (upload, transformacje, usuwanie)

---

##  Modele domenowe

Kluczowe encje w systemie:

| Grupa | Encje |
|---|---|
| **Użytkownicy** | `User`, `CustomerBusinessProfile` |
| **Firmy** | `Business`, `BusinessSubscription`, `BusinessVerification`, `MainCategory`, `ServiceCategory` |
| **Usługi** | `Service`, `ServiceVariant`, `ServiceBundle`, `ServiceBundleItem`, `EmployeeService` |
| **Pracownicy** | `Employee`, `EmployeeDetails`, `EmployeeCertificate`, `EmployeeFinanceSettings`, `EmployeePayroll`, `EmploymentContract` |
| **Grafiki** | `WorkSchedule`, `ScheduleException` |
| **Rezerwacje** | `Reservation` (statusy: Confirmed, Completed, Cancelled, NoShow) |
| **Finanse** | `Payment`, `Invoice`, `InvoiceItem`, `DailyFinancialReport`, `DailyEmployeePerformance`, `CommissionRate` |
| **Marketing** | `Discount`, `LoyaltyPoint`, `LoyaltyProgramConfig`, `LoyaltyTransaction` |
| **Komunikacja** | `Conversation`, `Message` |
| **Oceny** | `Review` |
| **Ulubione** | `UserFavoriteService` |
| **Subskrypcje** | `SubscriptionPlan` (Free, Silver, Gold) |

---

##  Endpointy API (przegląd)

| Kontroler | Odpowiedzialność |
|---|---|
| `AuthController` | Rejestracja, logowanie, odświeżanie tokenów JWT |
| `BusinessesController` | CRUD firm, profil publiczny, zdjęcia, kategorie |
| `ServicesController` | CRUD usług i wariantów |
| `ServiceBundlesController` | Pakiety usług |
| `ServiceCategoriesController` | Podkategorie usług |
| `EmployeesController` | CRUD pracowników, certyfikaty, przypisania usług |
| `SchedulesController` | Grafiki pracy, wyjątki |
| `AvailabilityController` | Obliczanie dostępnych slotów czasowych |
| `ReservationsController` | Tworzenie rezerwacji (klient + owner), statusy, kalendarz, rabaty, punkty lojalnościowe |
| `ReviewsController` | Opinie i oceny |
| `DiscountsController` | Kody rabatowe – tworzenie, weryfikacja, stosowanie |
| `LoyaltyController` | Program lojalnościowy – konfiguracja, saldo, transakcje |
| `PaymentsController` | Rejestracja płatności, prowizje |
| `FinanceController` | Raporty dzienne, statystyki finansowe |
| `EmployeeFinanceController` | HR – umowy, listy płac, ustawienia finansowe |
| `InvoicesController` | Generowanie i zarządzanie fakturami |
| `CustomersController` | CRM – profile klientów biznesowych |
| `FavoritesController` | Ulubione firmy |
| `MessagesController` | Historia wiadomości |
| `PhotosController` | Upload/zarządzanie zdjęciami (Cloudinary) |
| `SearchController` | Wyszukiwanie z filtrami i sortowaniem |
| `SubscriptionController` | Zarządzanie subskrypcjami |
| `AdminController` | Panel admina – statystyki, weryfikacja firm, plany |
| `DocumentController` | Dokumenty firmowe |

Pełna dokumentacja endpointów dostępna na [Swagger UI](https://booklocal-api-gja0begeg4gfbfcj.polandcentral-01.azurewebsites.net/swagger).

---

##  Uruchomienie lokalne

### Wymagania

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Angular CLI](https://angular.io/cli) (`npm install -g @angular/cli`)
- SQL Server (lokalny lub Azure SQL)

### Backend

1. Otwórz `BookLocal.sln` w Visual Studio lub Rider.
2. Uzupełnij plik `BookLocal.API/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=BookLocal;Trusted_Connection=True;TrustServerCertificate=True"
     },
     "Jwt": {
       "Key": "your-secret-key-min-32-characters",
       "Issuer": "BookLocal",
       "Audience": "BookLocalApp"
     },
     "CloudinarySettings": {
       "CloudName": "your-cloud",
       "ApiKey": "your-key",
       "ApiSecret": "your-secret"
     }
   }
   ```
3. Uruchom projekt `BookLocal.API`. Baza danych zostanie utworzona i wypełniona danymi testowymi automatycznie (~800 użytkowników, 30 firm, tysiące rezerwacji).

### Frontend (Web)

```bash
cd booklocal-frontend
npm install
```

Upewnij się, że `src/environments/environment.ts` wskazuje na lokalne API:
```typescript
export const environment = {
  apiUrl: 'https://localhost:7036/api'
};
```

```bash
ng serve
```

Aplikacja dostępna pod `http://localhost:4200`.

### Mobile (opcjonalnie)

```bash
cd booklocal_mobile
flutter pub get
flutter run
```

---

##  CI/CD

Projekt zawiera **3 workflow GitHub Actions**:

| Workflow | Trigger | Cel |
|---|---|---|
| `azure-static-web-apps-*.yml` | Push do `master` | Deploy frontendu Angular na Azure Static Web Apps |
| `master_booklocal-api.yml` | Push do `master` | Deploy backendu .NET na Azure App Service |
| `deploy.yml` | Push do `master` | Dodatkowy pipeline wdrożeniowy |

---

##  Struktura projektu

```
BookLocal/
├── BookLocal.API/          # Backend – ASP.NET Core Web API
│   ├── Controllers/        # 28 kontrolerów REST API (endpointy HTTP)
│   ├── Services/           # 33 serwisy z logiką biznesową
│   ├── Interfaces/         # Interfejsy dla serwisów (Dependency Injection)
│   ├── DTOs/               # Data Transfer Objects
│   ├── Data/               # DbContext, DbInitializer, Migracje
│   ├── Hubs/               # SignalR (ChatHub, NotificationHub, PresenceHub)
│   └── Program.cs          # Konfiguracja aplikacji
│
├── BookLocal.Data/         # Warstwa danych
│   └── Models/             # 37 modeli domenowych (EF Core entities)
│
├── booklocal-frontend/     # Frontend – Angular SPA
│   └── src/
│       ├── core/services/  # 33 serwisy (auth, business, chat, finance...)
│       ├── features/       # Moduły funkcjonalne (home, dashboard, auth...)
│       ├── shared/         # Współdzielone komponenty (modale rezerwacji)
│       └── types/          # Interfejsy i modele TypeScript
│
├── booklocal_mobile/       # Aplikacja mobilna – Flutter/Dart
│
├── .github/workflows/      # CI/CD pipelines (GitHub Actions)
├── *.sql                   # Procedury SQL (raporty finansowe)
└── README.md
```

---

##  Licencja

Ten projekt jest licencjonowany na warunkach licencji MIT – szczegóły w pliku [LICENSE](LICENSE).
