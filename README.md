# BookLocal - Platforma Rezerwacji Usług Lokalnych

> Aplikacja webowa typu marketplace umożliwiająca klientom wyszukiwanie i rezerwowanie usług u lokalnych specjalistów, a przedsiębiorcom zarządzanie swoją ofertą, pracownikami i rezerwacjami. Projekt realizowany w ramach nauki/studiów.

## Status Projektu

**Projekt w trakcie rozwoju.** Głównym celem była nauka i implementacja kluczowych technologii webowych w ekosystemie .NET. Wiele podstawowych mechanizmów CRUD oraz interfejsu użytkownika zostało zaimplementowanych, jednak brakuje jeszcze pełnej logiki biznesowej, systemu uwierzytelniania i niektórych zaawansowanych funkcji.

## Technologie

* **Backend:** C#, ASP.NET Core MVC (.NET 9)
* **Dostęp do danych:** Entity Framework Core 9
* **Baza Danych:** SQL Server LocalDB
* **Frontend:** HTML5, CSS3, JavaScript, Bootstrap 5, jQuery
* **Biblioteki JS:** FullCalendar.io (dla kalendarza rezerwacji), Font Awesome (dla ikon)
* **Architektura:** Podejście MVC, podział na projekty (PortalWWW, Intranet, Data)

## Funkcjonalności

### Zaimplementowane (w różnym stopniu ukończenia):

* **Projekt Intranet:**
    * Layout panelu administracyjnego (Sidebar + Główna treść).
    * Podstawowe operacje CRUD (Create, Read, Update, Delete) dla:
        * Pracowników/Specjalistów (`Pracownik`)
        * Usług Głównych (`Usluga`)
        * Wariantów Usług (`SzczegolyUslugi` - w tym cena i czas trwania)
        * Kategorii Usług (`KategoriaUslug` - jeśli istnieje ten model)
        * Treści CMS (np. `OdnosnikCms`, `SekcjaCms`, `ZawartoscCms`, `LogoCms`)
    * Strona główna panelu (Dashboard) z dynamicznymi statystykami (np. liczba rezerwacji).
    * Widok listy rezerwacji (`Rezerwacja`) z podstawowym kalendarzem (FullCalendar) i filtrowaniem po pracowniku (AJAX).
    * Formularz dodawania rezerwacji z obsługą klientów niezarejestrowanych.
    * Automatyczne ustawianie daty modyfikacji dla encji CMS.
    * Dynamiczne/kaskadowe listy rozwijane w formularzach (np. Pracownik -> Warianty Usługi, Usługa -> Pracownicy).
* **Projekt PortalWWW:**
    * Layout strony publicznej.
    * Dynamiczne menu nawigacyjne oparte na danych z CMS (`OdnosnikCms`).
    * Strona główna wyświetlająca dynamiczną treść z CMS (nagłówki, sekcje "Jak działa", "Dlaczego my", karuzela logo).
    * Widok listy usług (`Usluga`) w formie kart poziomych, wyświetlający podstawowe informacje i listę wariantów (`SzczegolyUslugi`) z cenami.
    * Widok szczegółów usługi (`Usluga`) grupujący warianty (`SzczegolyUslugi`) według pracowników (`Pracownik`) i zawierający przyciski "Umów się".
    * Widok listy specjalistów (`Pracownik`) w formie kart z wyszukiwarką (po nazwisku, po usłudze).
* **Projekt Data:**
    * Zdefiniowane modele encji dla bazy danych.
    * Konfiguracja `DbContext` z `DbSet`ami.
    * Migracje Entity Framework Core do tworzenia schematu bazy.

### Planowane / Niezakończone / Do Poprawy:

* **Pełny system logowania i rejestracji:** Zarówno dla Klientów (`Uzytkownik`) w Portalu, jak i dla Przedsiębiorców w Intranecie (obecnie brakuje implementacji weryfikacji hasła, zarządzania sesją/tokenami).
* **Autoryzacja:** Implementacja ról i uprawnień (np. Przedsiębiorca widzi tylko swoje dane, Admin widzi wszystko).
* **Panel Klienta w Portalu:** Strony "Mój Profil".
* **System Opinii:** Pełna implementacja dodawania i wyświetlania opinii.
* **Rozliczenia / Pensje:** Rozwinięcie logiki biznesowej i interfejsu.
* **Obsługa obrazów:** Zastąpienie pola `ZdjecieUrl` (URL) mechanizmem uploadu plików na serwer.
* **Walidacja:** Rozbudowa walidacji (po stronie serwera i klienta).
* **Testy:** Brak testów jednostkowych i integracyjnych.
* **Optymalizacja:** Poprawa wydajności zapytań, paginacja list.
* **Styling:** Dokończenie i ujednolicenie stylów CSS.
* **Wdrożenie (Deployment).**

## Struktura Projektu

Projekt składa się z kilku części:

* **BookLocal.Data:** Biblioteka klas zawierająca modele danych, definicję `DbContext` oraz Migracje EF Core.
* **BookLocal.Intranet:** Aplikacja webowa ASP.NET Core MVC - panel administracyjny dla przedsiębiorców.
* **BookLocal.PortalWWW:** Aplikacja webowa ASP.NET Core MVC - strona publiczna dla klientów.

## Uruchomienie Lokalne

### Wymagania Wstępne

* Zainstalowany .NET SDK (wersja 9.0).
* Visual Studio 2022 (lub nowszy) z zainstalowanym workloadem "ASP.NET and web development".
* SQL Server LocalDB.

### Kroki Instalacji

1.  **Sklonuj Repozytorium:**
    ```bash
    git clone [https://sjp.pl/repozytorium](https://sjp.pl/repozytorium)
    cd [Folder Projektu]
    ```
2.  **Otwórz Rozwiązanie:** Otwórz plik `.sln` w Visual Studio.
3.  **Skonfiguruj Bazę Danych:**
    * Otwórz `Package Manager Console` (Tools -> NuGet Package Manager -> Package Manager Console).
    * Upewnij się, że jako "Default project" wybrany jest projekt zawierający Twój `DbContext` (np. `BookLocal.Data`).
    * Wykonaj komendę, aby utworzyć bazę danych na podstawie migracji:
      ```powershell
      Update-Database
      ```
      Spowoduje to utworzenie bazy danych SQL Server LocalDB (nazwa zdefiniowana w `appsettings.Development.json` w projektach Web).
4.  **Uruchom Aplikacje:**
    * Możesz ustawić wiele projektów startowych (kliknij prawym na Solution -> Set Startup Projects...) i wybrać `BookLocal.Intranet` oraz `BookLocal.PortalWWW`.
    * Naciśnij F5 lub przycisk "Start" w Visual Studio. Aplikacje powinny uruchomić się na różnych portach lokalnych.

---
