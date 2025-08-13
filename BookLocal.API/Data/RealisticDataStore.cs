using Bogus;

namespace BookLocal.API.Data;


/// <param name="Name">Nazwa usługi.</param>
/// <param name="MinPrice">Minimalna cena.</param>
/// <param name="MaxPrice">Maksymalna cena.</param>
/// <param name="DurationMinutes">Czas trwania w minutach.</param>
public record ServiceTemplate(string Name, decimal MinPrice, decimal MaxPrice, int DurationMinutes);


/// <param name="BusinessNameTemplates">Szablony nazw dla firm z tej branży. "{Name}" zostanie zastąpione losowym nazwiskiem.</param>
/// <param name="SubCategoryNames">Przykładowe nazwy podkategorii.</param>
/// <param name="Services">Lista szablonów usług.</param>
/// <param name="EmployeePositions">Przykładowe stanowiska dla pracowników.</param>
public record CategoryRealisticData(
    List<string> BusinessNameTemplates,
    List<string> SubCategoryNames,
    List<ServiceTemplate> Services,
    List<string> EmployeePositions
);

public static class RealisticDataStore
{
    private static readonly Dictionary<string, CategoryRealisticData> Data = new()
    {
        ["Fryzjer"] = new CategoryRealisticData(
            BusinessNameTemplates: new List<string> { "Studio Fryzur {Name}", "Salon Piękności {Name}", "Atelier Włosów {Name}", "Fabryka Fryzur" },
            SubCategoryNames: new List<string> { "Strzyżenie i Modelowanie", "Koloryzacja", "Pielęgnacja i Regeneracja", "Stylizacje Okolicznościowe" },
            Services: new List<ServiceTemplate>
            {
                new("Strzyżenie męskie klasyczne", 50, 90, 30),
                new("Strzyżenie damskie z modelowaniem", 100, 250, 60),
                new("Strzyżenie grzywki", 20, 50, 15),
                new("Baleyage / Ombre / Sombre", 350, 700, 180),
                new("Refleksy", 250, 500, 120),
                new("Koloryzacja jednolita", 200, 400, 90),
                new("Keratynowe prostowanie włosów", 400, 900, 210),
                new("Regeneracja Olaplex", 150, 300, 60),
                new("Upięcie okolicznościowe / Kok", 150, 350, 75),
                new("Strzyżenie męskie (skin fade)", 70, 120, 45),
                new("Strzyżenie dziecięce (do lat 10)", 40, 70, 30),
                new("Botoks na włosy", 200, 400, 90),
                new("Zabieg laminacji włosów", 180, 350, 75),
                new("Tonowanie włosów po rozjaśnianiu", 100, 200, 45),
                new("Farbowanie odrostu", 150, 250, 75),
                new("Pasemka / Highlights", 300, 600, 150),
                new("Fryzura próbna ślubna", 150, 300, 90),
                new("Masaż skóry głowy z ampułką", 60, 120, 25),
                new("Podcięcie końcówek", 50, 90, 30)
            },
            EmployeePositions: new List<string> { "Stylista Fryzur", "Kolorysta", "Młodszy Stylista", "Mistrz Fryzjerstwa" }
        ),

        ["Barber"] = new CategoryRealisticData(
            BusinessNameTemplates: new List<string> { "Barber Shop {Name}", "U Brodacza", "Męski Zakątek", "The Razor's Edge" },
            SubCategoryNames: new List<string> { "Strzyżenie i Golenie", "Pielęgnacja Brody i Wąsów", "Usługi Dodatkowe" },
            Services: new List<ServiceTemplate>
            {
                new("Strzyżenie włosów", 60, 120, 45),
                new("Strzyżenie maszynką (buzz cut)", 40, 70, 20),
                new("Combo (włosy + broda)", 100, 180, 75),
                new("Golenie brody brzytwą na gorąco", 50, 90, 45),
                new("Trymowanie i pielęgnacja brody", 50, 100, 30),
                new("Cover (odsiwianie włosów/brody)", 80, 150, 45),
                new("Woskowanie (nos / uszy)", 20, 40, 15),
                new("Golenie głowy brzytwą", 70, 120, 45),
                new("Black mask (czarna maska na twarz)", 40, 70, 25),
                new("Gorący ręcznik i pielęgnacja twarzy", 50, 90, 30),
                new("Farbowanie brody", 60, 100, 40),
                new("Strzyżenie włosów długich męskich", 80, 140, 60),
                new("Zaczeska (combover) z przedziałkiem", 70, 130, 50),
                new("Wzorki na włosach (hair tattoo)", 30, 80, 20),
                new("Męski manicure", 50, 90, 40),
                new("Peeling skóry głowy", 40, 70, 20),
                new("Depilacja woskiem (brwi/policzki)", 30, 60, 20)
            },
            EmployeePositions: new List<string> { "Barber", "Młodszy Barber", "Golibroda", "Head Barber" }
        ),

        ["Masaż"] = new CategoryRealisticData(
            BusinessNameTemplates: new List<string> { "Studio Masażu {Name}", "Dotyk Relaksu", "Fizjo & Body", "Oaza Spokoju" },
            SubCategoryNames: new List<string> { "Masaże Relaksacyjne", "Masaże Lecznicze", "Masaże Orientalne", "Zabiegi na Ciało" },
            Services: new List<ServiceTemplate>
            {
                new("Masaż klasyczny całościowy", 120, 200, 60),
                new("Masaż relaksacyjny", 130, 220, 60),
                new("Masaż gorącymi kamieniami", 180, 300, 75),
                new("Masaż sportowy", 150, 250, 50),
                new("Drenaż limfatyczny", 140, 220, 60),
                new("Masaż pleców i karku", 80, 140, 30),
                new("Refleksologia stóp", 90, 150, 45),
                new("Peeling ciała", 100, 180, 40),
                new("Masaż Lomi Lomi Nui", 200, 350, 90),
                new("Masaż Ajurwedyjski Abhyanga", 220, 380, 90),
                new("Masaż stemplami ziołowymi", 180, 320, 75),
                new("Masaż bambusami", 170, 280, 60),
                new("Masaż dla kobiet w ciąży", 140, 220, 60),
                new("Masaż bańką chińską (antycellulitowy)", 100, 180, 45),
                new("Masaż twarzy Kobido", 200, 350, 75),
                new("Masaż stóp z elementami akupresury", 90, 160, 50),
                new("Aromaterapeutyczny masaż świecą", 160, 260, 60),
                new("Masaż czekoladą", 180, 300, 75)
            },
            EmployeePositions: new List<string> { "Masażysta", "Technik masażysta", "Fizjoterapeuta", "Terapeuta Manualny" }
        ),

        ["Fizjoterapia"] = new CategoryRealisticData(
            BusinessNameTemplates: new List<string> { "Gabinet Fizjoterapii {Name}", "Reha-Center", "Fizjo-Sport", "Centrum Rehabilitacji" },
            SubCategoryNames: new List<string> { "Konsultacje", "Terapia Manualna", "Fizykoterapia", "Kinezyterapia" },
            Services: new List<ServiceTemplate>
            {
                new("Konsultacja fizjoterapeutyczna", 150, 250, 45),
                new("Terapia manualna kręgosłupa", 140, 220, 50),
                new("Masaż leczniczy", 120, 200, 60),
                new("Kinesiotaping (aplikacja)", 40, 80, 20),
                new("Terapia punktów spustowych", 100, 180, 45),
                new("Ultradźwięki", 50, 90, 20),
                new("Laseroterapia", 60, 100, 15),
                new("Terapia powięziowa FDM", 150, 250, 45),
                new("Fizjoterapia uroginekologiczna (konsultacja)", 180, 300, 60),
                new("Rehabilitacja po urazie stawu skokowego", 130, 200, 50),
                new("Terapia falą uderzeniową (jeden obszar)", 80, 150, 20),
                new("Igłoterapia sucha (dry needling)", 100, 180, 30),
                new("Indywidualne ćwiczenia korekcyjne", 120, 200, 55),
                new("Terapia przeciwobrzękowa", 110, 190, 45),
                new("Rehabilitacja sportowa", 140, 240, 60),
                new("Terapia wisceralna (masaż brzucha)", 160, 260, 50),
                new("Instruktaż ćwiczeń do domu", 100, 180, 40)
            },
            EmployeePositions: new List<string> { "Fizjoterapeuta", "Magister Fizjoterapii", "Terapeuta Manualny", "Rehabilitant" }
        ),

        ["Kosmetyczka"] = new CategoryRealisticData(
            BusinessNameTemplates: new List<string> { "Salon Kosmetyczny {Name}", "Strefa Piękna", "Beauty Room {Name}", "Instytut Urody" },
            SubCategoryNames: new List<string> { "Pielęgnacja Twarzy", "Pielęgnacja Dłoni i Stóp", "Stylizacja Rzęs i Brwi", "Depilacja" },
            Services: new List<ServiceTemplate>
            {
                new("Oczyszczanie manualne twarzy", 150, 250, 90),
                new("Mikrodermabrazja diamentowa", 180, 300, 75),
                new("Peeling kawitacyjny z maską", 120, 200, 60),
                new("Manicure hybrydowy", 80, 150, 60),
                new("Pedicure klasyczny", 100, 180, 75),
                new("Regulacja i henna brwi", 40, 80, 30),
                new("Laminacja i lifting rzęs", 120, 200, 75),
                new("Depilacja woskiem - całe nogi", 90, 160, 45),
                new("Zabieg bankietowy (liftingujący)", 200, 350, 75),
                new("Eksfoliacja kwasem migdałowym", 160, 280, 60),
                new("Zabieg rozświetlający z witaminą C", 180, 300, 70),
                new("Makijaż okolicznościowy", 150, 300, 90),
                new("Makijaż ślubny (próbny)", 180, 350, 120),
                new("Przedłużanie rzęs metodą 1:1", 150, 250, 120),
                new("Uzupełnianie rzęs (do 3 tyg.)", 100, 180, 90),
                new("Pedicure hybrydowy", 120, 200, 90),
                new("Zabieg parafinowy na dłonie", 50, 90, 30),
                new("Przekłuwanie uszu (z kolczykami)", 80, 150, 30)
            },
            EmployeePositions: new List<string> { "Kosmetyczka", "Kosmetolog", "Stylistka Paznokci", "Linergistka" }
        ),

        ["Trening Personalny"] = new CategoryRealisticData(
            BusinessNameTemplates: new List<string> { "{Name} Trening Personalny", "Studio Treningu", "Forma na Plus", "Kuźnia Formy" },
            SubCategoryNames: new List<string> { "Trening Indywidualny", "Konsultacje", "Pakiety Treningowe" },
            Services: new List<ServiceTemplate>
            {
                new("Trening personalny (pojedyncza sesja)", 120, 250, 60),
                new("Pakiet 4 treningów personalnych", 450, 900, 60),
                new("Pakiet 8 treningów personalnych", 800, 1600, 60),
                new("Konsultacja treningowa z ułożeniem planu", 200, 400, 90),
                new("Trening wprowadzający na siłowni", 100, 180, 75),
                new("Analiza składu ciała Tanita", 50, 100, 20),
                new("Trening funkcjonalny", 120, 220, 60),
                new("Trening obwodowy", 110, 200, 55),
                new("Trening dla dwojga (cena za parę)", 180, 300, 60),
                new("Przygotowanie do testów sprawnościowych", 140, 250, 75),
                new("Trening medyczny (po kontuzji)", 150, 280, 60),
                new("Trening dla seniorów", 90, 160, 50),
                new("Korekcja wad postawy", 130, 220, 60),
                new("Trening online (pojedyncza sesja)", 100, 180, 60),
                new("Ułożenie planu dietetycznego", 250, 500, 60),
                new("Trening w małej grupie (3-4 os.)", 60, 100, 60)
            },
            EmployeePositions: new List<string> { "Trener Personalny", "Trener Przygotowania Motorycznego", "Instruktor Siłowni", "Dietetyk" }
        ),

        ["Groomer"] = new CategoryRealisticData(
            BusinessNameTemplates: new List<string> { "Salon dla Psa '{Name}'", "Psi Fryzjer", "Czysty Pupilek", "Spa dla Czworonoga" },
            SubCategoryNames: new List<string> { "Strzyżenie i Kąpiel", "Pielęgnacja", "Usługi Dodatkowe" },
            Services: new List<ServiceTemplate>
            {
                new("Strzyżenie psa (mała rasa)", 80, 140, 90),
                new("Strzyżenie psa (duża rasa)", 120, 250, 120),
                new("Kąpiel pielęgnacyjna", 60, 120, 60),
                new("Rozczesywanie skołtunionej sierści", 50, 150, 60),
                new("Trymowanie", 100, 200, 90),
                new("Obcinanie pazurków", 20, 40, 15),
                new("Wyczesywanie martwego włosa (dla psów liniejących)", 90, 180, 75),
                new("Kąpiel lecznicza (preparaty weterynaryjne)", 80, 150, 60),
                new("Strzyżenie kota (bez narkozy)", 120, 220, 90),
                new("Czyszczenie uszu i oczu", 20, 50, 15),
                new("Pakiet 'Szczeniak' (pierwsza wizyta adaptacyjna)", 70, 120, 60),
                new("Farbowanie ogona lub uszu (bezpieczne farby)", 50, 100, 45),
                new("Przygotowanie psa do wystawy", 150, 300, 120),
                new("Pielęgnacja opuszek łap (z balsamem)", 30, 50, 20),
                new("Wizyta adaptacyjna", 50, 90, 30),
                new("Porady pielęgnacyjne", 40, 80, 20)
            },
            EmployeePositions: new List<string> { "Groomer", "Asystent Groomera", "Psi Fryzjer", "Behawiorysta" }
        ),

        ["Tatuaż"] = new CategoryRealisticData(
            BusinessNameTemplates: new List<string> { "Studio Tatuażu {Name}", "Inkognito", "Sztuka na Skórze", "Blackwork Studio" },
            SubCategoryNames: new List<string> { "Sesje Tatuażu", "Projekty", "Konsultacje" },
            Services: new List<ServiceTemplate>
            {
                new("Konsultacja i wycena projektu", 0, 100, 30),
                new("Mały tatuaż (do 1h pracy)", 250, 500, 60),
                new("Średni tatuaż (sesja 3h)", 600, 1200, 180),
                new("Duży tatuaż (sesja 6h - cały dzień)", 1200, 2500, 360),
                new("Poprawki starego tatuażu (cover-up)", 500, 1500, 120),
                new("Styl graficzny / sketch (sesja 3h)", 600, 1200, 180),
                new("Realizm czarno-szary (sesja 3h)", 700, 1400, 180),
                new("Dotwork / Geometria (sesja 3h)", 650, 1300, 180),
                new("Tatuaż typu 'ignorant'", 200, 450, 60),
                new("Napis / cytat (indywidualna wycena)", 200, 600, 60),
                new("Projekt indywidualny (opłata za projekt)", 150, 500, 60),
                new("Pierwszy mały tatuaż (promocja)", 200, 350, 60),
                new("Zakrycie blizny (konsultacja + wycena)", 100, 200, 30),
                new("Sesja 'handpoke' (do 2h)", 400, 800, 120),
                new("Dodanie cieniowania do istniejącego tatuażu", 300, 700, 90)
            },
            EmployeePositions: new List<string> { "Artysta Tatuażu", "Tatuażysta", "Manager Studia", "Apprentice" }
        ),

        ["Medycyna Estetyczna"] = new CategoryRealisticData(
            BusinessNameTemplates: new List<string> { "Klinika {Name}", "Esthetic Lab", "Centrum Medycyny Estetycznej" },
            SubCategoryNames: new List<string> { "Zabiegi na Twarz", "Modelowanie Sylwetki", "Laseroterapia" },
            Services: new List<ServiceTemplate>
            {
                new("Konsultacja z lekarzem medycyny estetycznej", 200, 350, 30),
                new("Powiększanie ust kwasem hialuronowym (1ml)", 800, 1400, 60),
                new("Botoks - jedna okolica (np. kurze łapki)", 500, 900, 45),
                new("Mezoterapia igłowa (twarz)", 400, 800, 60),
                new("Depilacja laserowa - pachy", 200, 400, 30),
                new("Depilacja laserowa - bikini", 300, 600, 45),
                new("Wolumetria twarzy (kwas hialuronowy)", 1000, 1800, 75),
                new("Osocze bogatopłytkowe ('wampirzy lifting')", 600, 1200, 60),
                new("Lipoliza iniekcyjna (podbródek)", 300, 600, 45),
                new("Usuwanie przebarwień laserem (jeden zabieg)", 400, 900, 40),
                new("Zamykanie naczynek na twarzy", 300, 700, 45),
                new("Leczenie nadpotliwości (botoks)", 1200, 2000, 60),
                new("Stymulatory tkankowe (np. Profhilo)", 900, 1600, 60),
                new("Leczenie bruksizmu (botoks)", 1000, 1800, 45),
                new("Peeling medyczny (np. PRX-T33)", 350, 600, 45),
                new("Wypełnianie doliny łez", 1000, 1600, 75)
            },
            EmployeePositions: new List<string> { "Lekarz Medycyny Estetycznej", "Kosmetolog", "Pielęgniarka", "Recepcjonistka" }
        ),

        ["Psycholog"] = new CategoryRealisticData(
            BusinessNameTemplates: new List<string> { "Gabinet Psychologiczny {Name}", "Poradnia Zdrowia Psychicznego", "Ośrodek Terapii" },
            SubCategoryNames: new List<string> { "Terapia Indywidualna", "Terapia Par", "Konsultacje" },
            Services: new List<ServiceTemplate>
            {
                new("Konsultacja psychologiczna (pierwsza wizyta)", 180, 300, 50),
                new("Sesja psychoterapii indywidualnej", 160, 280, 50),
                new("Sesja terapii dla par", 250, 400, 75),
                new("Interwencja kryzysowa", 200, 350, 60),
                new("Terapia poznawczo-behawioralna (CBT) - sesja", 170, 280, 50),
                new("Terapia psychodynamiczna - sesja", 170, 280, 50),
                new("Konsultacja seksuologiczna", 200, 350, 60),
                new("Diagnoza psychologiczna (pakiet spotkań)", 600, 1200, 50),
                new("Terapia dla młodzieży (13-18 lat)", 150, 250, 50),
                new("Warsztaty radzenia sobie ze stresem", 100, 200, 120),
                new("Poradnictwo zawodowe", 180, 300, 60),
                new("Terapia uzależnień", 160, 260, 50),
                new("Wsparcie psychologiczne online", 150, 250, 50),
                new("Terapia grupowa (jedno spotkanie)", 80, 150, 90)
            },
            EmployeePositions: new List<string> { "Psycholog", "Psychoterapeuta", "Psycholog Dziecięcy", "Seksuolog" }
        ),

        ["Pilates"] = new CategoryRealisticData(
            BusinessNameTemplates: new List<string> { "Studio Pilates {Name}", "Core Studio", "Ruch i Oddech" },
            SubCategoryNames: new List<string> { "Zajęcia Grupowe", "Zajęcia Indywidualne", "Warsztaty" },
            Services: new List<ServiceTemplate>
            {
                new("Pilates na matach (wejście jednorazowe)", 40, 70, 55),
                new("Karnet 4 wejścia - Pilates na matach", 140, 240, 55),
                new("Trening indywidualny na reformerze", 150, 250, 60),
                new("Zajęcia dla kobiet w ciąży", 50, 80, 50),
                new("Pilates na krzesłach (dla seniorów)", 40, 70, 50),
                new("Pilates z dużymi piłkami", 45, 75, 55),
                new("Zajęcia na reformerach (grupa)", 80, 150, 60),
                new("Zdrowy kręgosłup (zajęcia grupowe)", 40, 70, 55),
                new("Stretching i mobilność", 40, 60, 50),
                new("Karnet 8 wejść - na matach", 240, 400, 55),
                new("Karnet OPEN miesięczny", 250, 450, 55),
                new("Lekcja wprowadzająca (indywidualna)", 120, 200, 60),
                new("Pilates dla biegaczy", 50, 80, 60),
                new("Zajęcia online (na żywo)", 30, 50, 55)
            },
            EmployeePositions: new List<string> { "Instruktor Pilates", "Fizjoterapeuta", "Trener Personalny" }
        ),

        ["Joga"] = new CategoryRealisticData(
            BusinessNameTemplates: new List<string> { "Studio Jogi {Name}", "Ashtanga Space", "Vinyasa Flow" },
            SubCategoryNames: new List<string> { "Joga Grupowa", "Joga Indywidualna", "Medytacja" },
            Services: new List<ServiceTemplate>
            {
                new("Joga Vinyasa (wejście jednorazowe)", 40, 70, 75),
                new("Karnet OPEN na wszystkie zajęcia", 200, 350, 75),
                new("Zajęcia jogi indywidualnej", 140, 250, 75),
                new("Kurs jogi dla początkujących (8 spotkań)", 300, 500, 90),
                new("Sesja medytacji prowadzonej", 30, 60, 45),
                new("Ashtanga joga (prowadzona)", 50, 80, 90),
                new("Joga Iyengara (z użyciem sprzętu)", 50, 80, 90),
                new("Yin Joga (głębokie rozciąganie)", 45, 75, 75),
                new("Joga nidra (głęboka relaksacja)", 40, 70, 60),
                new("Joga dla kręgosłupa", 45, 75, 75),
                new("Warsztaty weekendowe (cena za dzień)", 150, 300, 360),
                new("Zajęcia z pranayamy (techniki oddechowe)", 30, 60, 45),
                new("Joga w plenerze (sezonowo)", 30, 50, 75),
                new("Aerial joga (z chustami)", 60, 100, 75),
                new("Joga dla dzieci", 35, 60, 45)
            },
            EmployeePositions: new List<string> { "Nauczyciel Jogi", "Jogin", "Instruktor Medytacji" }
        ),
    };

    public static readonly List<List<int>> RatingProfiles = new()
    {
        new() { 5, 5, 5, 5, 5, 5, 5, 5, 5, 4 },
        new() { 5, 5, 5, 5, 5, 5, 4, 4, 3, 3 },
        new() { 5, 5, 5, 5, 4, 4, 4, 3, 2, 1 },
        new() { 5, 5, 5, 5, 5, 4, 4, 4, 3, 2 },
        new() { 5, 5, 5, 5, 2, 2, 1, 1, 1, 1 },
        new() { 4, 4, 4, 4, 4, 4, 5, 3, 3, 2 },
        new() { 5, 5, 4 },
        new() { 5, 4, 3, 2, 5, 4, 3, 2, 4, 3 },
        new() { 4, 3, 3, 2, 2, 2, 1, 5, 1, 2 },
        new() { 5, 5, 5, 5, 5 }
    };

    public static Dictionary<string, CategoryRealisticData> GetData() => Data;

    public static CategoryRealisticData GetRandomRealisticData(Faker faker)
    {
        return faker.PickRandom(Data.Values.ToList());
    }
}