using Microsoft.EntityFrameworkCore.Migrations;

namespace BookLocal.Data.Migrations
{
    /// <inheritdoc />
    public partial class DataBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NapisPowitalny",
                columns: table => new
                {
                    IdLoga = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kolejnosc = table.Column<int>(type: "int", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AltText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LinkUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NapisPowitalny", x => x.IdLoga);
                });

            migrationBuilder.CreateTable(
                name: "Przedsiebiorca",
                columns: table => new
                {
                    IdPrzedsiebiorcy = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Login = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HasloHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Imie = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nazwisko = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    CzyAktywny = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Przedsiebiorca", x => x.IdPrzedsiebiorcy);
                });

            migrationBuilder.CreateTable(
                name: "Usluga",
                columns: table => new
                {
                    IdUslugi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nazwa = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZdjecieUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usluga", x => x.IdUslugi);
                });

            migrationBuilder.CreateTable(
                name: "Uzytkownik",
                columns: table => new
                {
                    IdUzytkownika = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Login = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HasloHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Imie = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nazwisko = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    TelefonKontaktowy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uzytkownik", x => x.IdUzytkownika);
                });

            migrationBuilder.CreateTable(
                name: "Adres",
                columns: table => new
                {
                    IdAdresu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ulica = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NrDomu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NrLokalu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    KodPocztowy = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Miejscowosc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Gmina = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Powiat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Wojewodztwo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Kraj = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Poczta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UzytkownikId = table.Column<int>(type: "int", nullable: true),
                    PracownikId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adres", x => x.IdAdresu);
                    table.ForeignKey(
                        name: "FK_Adres_Uzytkownik_UzytkownikId",
                        column: x => x.UzytkownikId,
                        principalTable: "Uzytkownik",
                        principalColumn: "IdUzytkownika");
                });

            migrationBuilder.CreateTable(
                name: "Firma",
                columns: table => new
                {
                    IdFirmy = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nazwa = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WlascicielId = table.Column<int>(type: "int", nullable: false),
                    AdresId = table.Column<int>(type: "int", nullable: true),
                    NIP = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    REGON = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Firma", x => x.IdFirmy);
                    table.ForeignKey(
                        name: "FK_Firma_Adres_AdresId",
                        column: x => x.AdresId,
                        principalTable: "Adres",
                        principalColumn: "IdAdresu");
                    table.ForeignKey(
                        name: "FK_Firma_Przedsiebiorca_WlascicielId",
                        column: x => x.WlascicielId,
                        principalTable: "Przedsiebiorca",
                        principalColumn: "IdPrzedsiebiorcy",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pracownik",
                columns: table => new
                {
                    IdPracownika = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Imie = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nazwisko = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZdjecieUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Stanowisko = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmailKontaktowy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TelefonKontaktowy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FirmaId = table.Column<int>(type: "int", nullable: false),
                    CzyAktywny = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pracownik", x => x.IdPracownika);
                    table.ForeignKey(
                        name: "FK_Pracownik_Firma_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firma",
                        principalColumn: "IdFirmy",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Konwersacja",
                columns: table => new
                {
                    IdKonwersacji = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UzytkownikId = table.Column<int>(type: "int", nullable: false),
                    PracownikId = table.Column<int>(type: "int", nullable: false),
                    Temat = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DataUtworzenia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataOstatniejWiadomosci = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Konwersacja", x => x.IdKonwersacji);
                    table.ForeignKey(
                        name: "FK_Konwersacja_Pracownik_PracownikId",
                        column: x => x.PracownikId,
                        principalTable: "Pracownik",
                        principalColumn: "IdPracownika",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Konwersacja_Uzytkownik_UzytkownikId",
                        column: x => x.UzytkownikId,
                        principalTable: "Uzytkownik",
                        principalColumn: "IdUzytkownika",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pensja",
                columns: table => new
                {
                    IdPensjii = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PracownikId = table.Column<int>(type: "int", nullable: false),
                    KwotaPodstawowa = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Premia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Potracenia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OkresOd = table.Column<DateOnly>(type: "date", nullable: false),
                    OkresDo = table.Column<DateOnly>(type: "date", nullable: false),
                    StatusWyplaty = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DataWyplaty = table.Column<DateOnly>(type: "date", nullable: true),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataUtworzeniaZapisu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ZarzadzajacyPrzedsiębiorcaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pensja", x => x.IdPensjii);
                    table.ForeignKey(
                        name: "FK_Pensja_Pracownik_PracownikId",
                        column: x => x.PracownikId,
                        principalTable: "Pracownik",
                        principalColumn: "IdPracownika",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pensja_Przedsiebiorca_ZarzadzajacyPrzedsiębiorcaId",
                        column: x => x.ZarzadzajacyPrzedsiębiorcaId,
                        principalTable: "Przedsiebiorca",
                        principalColumn: "IdPrzedsiebiorcy");
                });

            migrationBuilder.CreateTable(
                name: "SekcjaCms",
                columns: table => new
                {
                    IdSekcji = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KluczSekcji = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Kolejnosc = table.Column<int>(type: "int", nullable: false),
                    Ikona = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Tytul = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinkUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LinkText = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastModifiedByPracownikId = table.Column<int>(type: "int", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SekcjaCms", x => x.IdSekcji);
                    table.ForeignKey(
                        name: "FK_SekcjaCms_Pracownik_LastModifiedByPracownikId",
                        column: x => x.LastModifiedByPracownikId,
                        principalTable: "Pracownik",
                        principalColumn: "IdPracownika");
                });

            migrationBuilder.CreateTable(
                name: "SzczegolyUslugi",
                columns: table => new
                {
                    IdSzczegolowUslugi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cena = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CzasTrwaniaMinuty = table.Column<int>(type: "int", nullable: false),
                    UslugaId = table.Column<int>(type: "int", nullable: false),
                    PracownikId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SzczegolyUslugi", x => x.IdSzczegolowUslugi);
                    table.ForeignKey(
                        name: "FK_SzczegolyUslugi_Pracownik_PracownikId",
                        column: x => x.PracownikId,
                        principalTable: "Pracownik",
                        principalColumn: "IdPracownika",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SzczegolyUslugi_Usluga_UslugaId",
                        column: x => x.UslugaId,
                        principalTable: "Usluga",
                        principalColumn: "IdUslugi",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZawartoscCms",
                columns: table => new
                {
                    IdZawartosci = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sekcja = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tresc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PracownikId = table.Column<int>(type: "int", nullable: false),
                    DataModyfikacji = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZawartoscCms", x => x.IdZawartosci);
                    table.ForeignKey(
                        name: "FK_ZawartoscCms_Pracownik_PracownikId",
                        column: x => x.PracownikId,
                        principalTable: "Pracownik",
                        principalColumn: "IdPracownika",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Wiadomosc",
                columns: table => new
                {
                    IdWiadomosci = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KonwersacjaId = table.Column<int>(type: "int", nullable: false),
                    NadawcaUzytkownikId = table.Column<int>(type: "int", nullable: true),
                    NadawcaPrzedsiębiorcaId = table.Column<int>(type: "int", nullable: true),
                    Tresc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataWyslania = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CzyOdczytana = table.Column<bool>(type: "bit", nullable: false),
                    DataOdczytania = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wiadomosc", x => x.IdWiadomosci);
                    table.ForeignKey(
                        name: "FK_Wiadomosc_Konwersacja_KonwersacjaId",
                        column: x => x.KonwersacjaId,
                        principalTable: "Konwersacja",
                        principalColumn: "IdKonwersacji",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wiadomosc_Przedsiebiorca_NadawcaPrzedsiębiorcaId",
                        column: x => x.NadawcaPrzedsiębiorcaId,
                        principalTable: "Przedsiebiorca",
                        principalColumn: "IdPrzedsiebiorcy");
                    table.ForeignKey(
                        name: "FK_Wiadomosc_Uzytkownik_NadawcaUzytkownikId",
                        column: x => x.NadawcaUzytkownikId,
                        principalTable: "Uzytkownik",
                        principalColumn: "IdUzytkownika");
                });

            migrationBuilder.CreateTable(
                name: "Rezerwacja",
                columns: table => new
                {
                    IdRezerwacji = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataRezerwacji = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ImieKlienta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NazwiskoKlienta = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: true),
                    TelefonKlienta = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UzytkownikId = table.Column<int>(type: "int", nullable: true),
                    WykonujacyPracownikId = table.Column<int>(type: "int", nullable: true),
                    ObslugujacyPrzedsiębiorcaId = table.Column<int>(type: "int", nullable: true),
                    SzczegolyUslugiId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rezerwacja", x => x.IdRezerwacji);
                    table.ForeignKey(
                        name: "FK_Rezerwacja_Pracownik_WykonujacyPracownikId",
                        column: x => x.WykonujacyPracownikId,
                        principalTable: "Pracownik",
                        principalColumn: "IdPracownika");
                    table.ForeignKey(
                        name: "FK_Rezerwacja_Przedsiebiorca_ObslugujacyPrzedsiębiorcaId",
                        column: x => x.ObslugujacyPrzedsiębiorcaId,
                        principalTable: "Przedsiebiorca",
                        principalColumn: "IdPrzedsiebiorcy");
                    table.ForeignKey(
                        name: "FK_Rezerwacja_SzczegolyUslugi_SzczegolyUslugiId",
                        column: x => x.SzczegolyUslugiId,
                        principalTable: "SzczegolyUslugi",
                        principalColumn: "IdSzczegolowUslugi");
                    table.ForeignKey(
                        name: "FK_Rezerwacja_Uzytkownik_UzytkownikId",
                        column: x => x.UzytkownikId,
                        principalTable: "Uzytkownik",
                        principalColumn: "IdUzytkownika");
                });

            migrationBuilder.CreateTable(
                name: "Opinia",
                columns: table => new
                {
                    IdOpinii = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tresc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ocena = table.Column<int>(type: "int", nullable: false),
                    UzytkownikId = table.Column<int>(type: "int", nullable: false),
                    OcenianyPracownikId = table.Column<int>(type: "int", nullable: true),
                    RezerwacjaId = table.Column<int>(type: "int", nullable: true),
                    DataDodania = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opinia", x => x.IdOpinii);
                    table.ForeignKey(
                        name: "FK_Opinia_Pracownik_OcenianyPracownikId",
                        column: x => x.OcenianyPracownikId,
                        principalTable: "Pracownik",
                        principalColumn: "IdPracownika");
                    table.ForeignKey(
                        name: "FK_Opinia_Rezerwacja_RezerwacjaId",
                        column: x => x.RezerwacjaId,
                        principalTable: "Rezerwacja",
                        principalColumn: "IdRezerwacji");
                    table.ForeignKey(
                        name: "FK_Opinia_Uzytkownik_UzytkownikId",
                        column: x => x.UzytkownikId,
                        principalTable: "Uzytkownik",
                        principalColumn: "IdUzytkownika",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransakcjaRozliczeniowa",
                columns: table => new
                {
                    IdTransakcji = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PracownikId = table.Column<int>(type: "int", nullable: false),
                    RezerwacjaId = table.Column<int>(type: "int", nullable: true),
                    KwotaBrutto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProwizjaPlatformy = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProwizjaFirmy = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KwotaNettoDlaPracownika = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StatusRozliczenia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DataUtworzenia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataOstatniejZmianyStatusu = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZatwierdzajacyPrzedsiębiorcaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransakcjaRozliczeniowa", x => x.IdTransakcji);
                    table.ForeignKey(
                        name: "FK_TransakcjaRozliczeniowa_Pracownik_PracownikId",
                        column: x => x.PracownikId,
                        principalTable: "Pracownik",
                        principalColumn: "IdPracownika",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransakcjaRozliczeniowa_Przedsiebiorca_ZatwierdzajacyPrzedsiębiorcaId",
                        column: x => x.ZatwierdzajacyPrzedsiębiorcaId,
                        principalTable: "Przedsiebiorca",
                        principalColumn: "IdPrzedsiebiorcy");
                    table.ForeignKey(
                        name: "FK_TransakcjaRozliczeniowa_Rezerwacja_RezerwacjaId",
                        column: x => x.RezerwacjaId,
                        principalTable: "Rezerwacja",
                        principalColumn: "IdRezerwacji");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Adres_PracownikId",
                table: "Adres",
                column: "PracownikId");

            migrationBuilder.CreateIndex(
                name: "IX_Adres_UzytkownikId",
                table: "Adres",
                column: "UzytkownikId");

            migrationBuilder.CreateIndex(
                name: "IX_Firma_AdresId",
                table: "Firma",
                column: "AdresId");

            migrationBuilder.CreateIndex(
                name: "IX_Firma_WlascicielId",
                table: "Firma",
                column: "WlascicielId");

            migrationBuilder.CreateIndex(
                name: "IX_Konwersacja_PracownikId",
                table: "Konwersacja",
                column: "PracownikId");

            migrationBuilder.CreateIndex(
                name: "IX_Konwersacja_UzytkownikId",
                table: "Konwersacja",
                column: "UzytkownikId");

            migrationBuilder.CreateIndex(
                name: "IX_Opinia_OcenianyPracownikId",
                table: "Opinia",
                column: "OcenianyPracownikId");

            migrationBuilder.CreateIndex(
                name: "IX_Opinia_RezerwacjaId",
                table: "Opinia",
                column: "RezerwacjaId");

            migrationBuilder.CreateIndex(
                name: "IX_Opinia_UzytkownikId",
                table: "Opinia",
                column: "UzytkownikId");

            migrationBuilder.CreateIndex(
                name: "IX_Pensja_PracownikId",
                table: "Pensja",
                column: "PracownikId");

            migrationBuilder.CreateIndex(
                name: "IX_Pensja_ZarzadzajacyPrzedsiębiorcaId",
                table: "Pensja",
                column: "ZarzadzajacyPrzedsiębiorcaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pracownik_FirmaId",
                table: "Pracownik",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Rezerwacja_ObslugujacyPrzedsiębiorcaId",
                table: "Rezerwacja",
                column: "ObslugujacyPrzedsiębiorcaId");

            migrationBuilder.CreateIndex(
                name: "IX_Rezerwacja_SzczegolyUslugiId",
                table: "Rezerwacja",
                column: "SzczegolyUslugiId");

            migrationBuilder.CreateIndex(
                name: "IX_Rezerwacja_UzytkownikId",
                table: "Rezerwacja",
                column: "UzytkownikId");

            migrationBuilder.CreateIndex(
                name: "IX_Rezerwacja_WykonujacyPracownikId",
                table: "Rezerwacja",
                column: "WykonujacyPracownikId");

            migrationBuilder.CreateIndex(
                name: "IX_SekcjaCms_LastModifiedByPracownikId",
                table: "SekcjaCms",
                column: "LastModifiedByPracownikId");

            migrationBuilder.CreateIndex(
                name: "IX_SzczegolyUslugi_PracownikId",
                table: "SzczegolyUslugi",
                column: "PracownikId");

            migrationBuilder.CreateIndex(
                name: "IX_SzczegolyUslugi_UslugaId",
                table: "SzczegolyUslugi",
                column: "UslugaId");

            migrationBuilder.CreateIndex(
                name: "IX_TransakcjaRozliczeniowa_PracownikId",
                table: "TransakcjaRozliczeniowa",
                column: "PracownikId");

            migrationBuilder.CreateIndex(
                name: "IX_TransakcjaRozliczeniowa_RezerwacjaId",
                table: "TransakcjaRozliczeniowa",
                column: "RezerwacjaId");

            migrationBuilder.CreateIndex(
                name: "IX_TransakcjaRozliczeniowa_ZatwierdzajacyPrzedsiębiorcaId",
                table: "TransakcjaRozliczeniowa",
                column: "ZatwierdzajacyPrzedsiębiorcaId");

            migrationBuilder.CreateIndex(
                name: "IX_Wiadomosc_KonwersacjaId",
                table: "Wiadomosc",
                column: "KonwersacjaId");

            migrationBuilder.CreateIndex(
                name: "IX_Wiadomosc_NadawcaPrzedsiębiorcaId",
                table: "Wiadomosc",
                column: "NadawcaPrzedsiębiorcaId");

            migrationBuilder.CreateIndex(
                name: "IX_Wiadomosc_NadawcaUzytkownikId",
                table: "Wiadomosc",
                column: "NadawcaUzytkownikId");

            migrationBuilder.CreateIndex(
                name: "IX_ZawartoscCms_PracownikId",
                table: "ZawartoscCms",
                column: "PracownikId");

            migrationBuilder.AddForeignKey(
                name: "FK_Adres_Pracownik_PracownikId",
                table: "Adres",
                column: "PracownikId",
                principalTable: "Pracownik",
                principalColumn: "IdPracownika");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adres_Pracownik_PracownikId",
                table: "Adres");

            migrationBuilder.DropTable(
                name: "NapisPowitalny");

            migrationBuilder.DropTable(
                name: "Opinia");

            migrationBuilder.DropTable(
                name: "Pensja");

            migrationBuilder.DropTable(
                name: "SekcjaCms");

            migrationBuilder.DropTable(
                name: "TransakcjaRozliczeniowa");

            migrationBuilder.DropTable(
                name: "Wiadomosc");

            migrationBuilder.DropTable(
                name: "ZawartoscCms");

            migrationBuilder.DropTable(
                name: "Rezerwacja");

            migrationBuilder.DropTable(
                name: "Konwersacja");

            migrationBuilder.DropTable(
                name: "SzczegolyUslugi");

            migrationBuilder.DropTable(
                name: "Usluga");

            migrationBuilder.DropTable(
                name: "Pracownik");

            migrationBuilder.DropTable(
                name: "Firma");

            migrationBuilder.DropTable(
                name: "Adres");

            migrationBuilder.DropTable(
                name: "Przedsiebiorca");

            migrationBuilder.DropTable(
                name: "Uzytkownik");
        }
    }
}
