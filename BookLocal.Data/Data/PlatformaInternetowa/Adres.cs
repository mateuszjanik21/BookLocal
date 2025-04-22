using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Data.PlatformaInternetowa
{
    public class Adres
    {
        [Key]
        public int IdAdresu { get; set; }

        [Required(ErrorMessage = "Wprowadź nazwę ulicy.")]
        [MaxLength(100)]
        public required string Ulica { get; set; }

        [Required(ErrorMessage = "Wprowadź numer domu.")]
        [MaxLength(10)]
        public required string NrDomu { get; set; }

        [MaxLength(10)]
        public string? NrLokalu { get; set; }

        [Required(ErrorMessage = "Wprowadź kod pocztowy.")]
        [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "Nieprawidłowy format kodu pocztowego (np. 00-000).")]
        [MaxLength(6)]
        public required string KodPocztowy { get; set; }

        [Required(ErrorMessage = "Wprowadź miejscowość.")]
        [MaxLength(50)]
        public required string Miejscowosc { get; set; }

        [Required(ErrorMessage = "Wprowadź gminę.")]
        [MaxLength(50)]
        public required string Gmina { get; set; }

        [Required(ErrorMessage = "Wprowadź powiat.")]
        [MaxLength(50)]
        public required string Powiat { get; set; }

        [Required(ErrorMessage = "Wprowadź województwo.")]
        [MaxLength(50)]
        public required string Wojewodztwo { get; set; }

        [Required(ErrorMessage = "Wprowadź kraj.")]
        [MaxLength(50)]
        public required string Kraj { get; set; } = "Polska";

        [Required(ErrorMessage = "Wprowadź nazwę urzędu pocztowego.")]
        [MaxLength(50)]
        public required string Poczta { get; set; }

        public int? UzytkownikId { get; set; }

        [ForeignKey("UzytkownikId")]
        public virtual Uzytkownik? Uzytkownik { get; set; }

        public int? PracownikId { get; set; }
        [ForeignKey("PracownikId")]
        public virtual Pracownik? Pracownik { get; set; } 
    }
}
