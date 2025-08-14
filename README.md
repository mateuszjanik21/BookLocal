BookLocal

BookLocal to w pełni funkcjonalna aplikacja webowa do rezerwacji wizyt w lokalnych firmach usługowych. Umożliwia klientom wyszukiwanie i rezerwowanie usług, a właścicielom firm zarządzanie swoim biznesem, pracownikami, grafikami i rezerwacjami.

Podgląd na żywo

    Aplikacja Frontendowa: https://wonderful-pebble-00b01fe03.2.azurestaticapps.net

    Dokumentacja API (Swagger): https://booklocal-api-gja0begeg4gfbfcj.polandcentral-01.azurewebsites.net/swagger

Główne Funkcje

    Wyszukiwanie i filtrowanie firm usługowych.

    System rezerwacji wizyt online.

    Panel zarządzania dla właścicieli firm.

    Uwierzytelnianie użytkowników oparte na rolach (klient, właściciel) przy użyciu tokenów JWT.

    System ocen i opinii dla firm.

    Przesyłanie i zarządzanie zdjęciami (usługa Cloudinary).

    Komunikacja w czasie rzeczywistym (czat, powiadomienia) za pomocą SignalR.

Stos Technologiczny

Frontend

    Framework: Angular

    Wdrożenie: Azure Static Web Apps

    CI/CD: GitHub Actions

Backend

    Framework: .NET (ASP.NET Core Web API)

    Baza Danych: Entity Framework Core

    Uwierzytelnianie: ASP.NET Core Identity z JWT

    Komunikacja Real-time: SignalR

    Wdrożenie: Azure App Service

Baza Danych

    Silnik: Azure SQL Database

    Zasilanie danymi: Aplikacja zawiera mechanizm do automatycznego wypełniania bazy danych realistycznymi, wygenerowanymi danymi przy użyciu biblioteki Bogus.

Usługi Chmurowe

    Hosting: Microsoft Azure

    Przechowywanie zdjęć: Cloudinary

Struktura Projektu

/
├── BookLocal.API/         # Główny projekt backendowy (ASP.NET Core API)
├── BookLocal.Data/        # Projekt z kontekstem bazy danych i modelami (EF Core)
├── booklocal-frontend/    # Projekt frontendowy (Angular)
└── BookLocal.sln          # Plik solucji Visual Studio

Konfiguracja i Uruchomienie Lokalne

Wymagania Wstępne

    .NET SDK (wersja zgodna z projektem)

    Node.js i npm

    Angular CLI (npm install -g @angular/cli)

    Lokalna instancja SQL Server (np. SQL Server Express) lub dostęp do bazy danych w chmurze

Uruchomienie Backendu

    Otwórz plik BookLocal.sln w Visual Studio.

    W pliku appsettings.Development.json w projekcie BookLocal.API skonfiguruj następujące sekcje:

        ConnectionStrings:DefaultConnection: Wpisz swój connection string do lokalnej bazy danych.

        Jwt: Ustaw swój klucz, wystawcę (issuer) i odbiorcę (audience).

        CloudinarySettings: Wpisz swoje klucze do usługi Cloudinary.

    Aby wypełnić bazę danych przykładowymi danymi, upewnij się, że kod inicjalizujący w Program.cs jest aktywny.

    Uruchom projekt BookLocal.API z Visual Studio (klawisz F5).

Uruchomienie Frontendu

    Otwórz terminal w folderze booklocal-frontend.

    Zainstaluj zależności: npm install.

    W pliku src/environments/environment.ts upewnij się, że apiUrl wskazuje na adres Twojego lokalnie uruchomionego API (domyślnie https://localhost:5001/api lub podobny).

    Uruchom serwer deweloperski Angulara: ng serve.

    Otwórz przeglądarkę pod adresem http://localhost:4200.
