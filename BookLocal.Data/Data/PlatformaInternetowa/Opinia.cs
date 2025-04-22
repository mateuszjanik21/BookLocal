using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Data.PlatformaInternetowa
{
    public class Opinia
    {
        [Key]
        public int IdOpinii { get; set; }

        [Required(ErrorMessage = "Wprowadź treść opinii.")]
        [DataType(DataType.MultilineText)]
        public required string Tresc { get; set; }

        [Required(ErrorMessage = "Wprowadź ocenę w skali 1-5.")]
        [Range(1, 5, ErrorMessage = "Ocena musi być w przedziale 1–5.")]
        public required int Ocena { get; set; }

        [Required(ErrorMessage = "Wskaż użytkownika.")]
        public required int UzytkownikId { get; set; }

        [ForeignKey("UzytkownikId")]
        public virtual Uzytkownik? Uzytkownik { get; set; }
        public int? OcenianyPracownikId { get; set; }

        [ForeignKey("OcenianyPracownikId")]
        public virtual Pracownik? OcenianyPracownik { get; set; }
        public int? RezerwacjaId { get; set; }

        [ForeignKey("RezerwacjaId")]
        public virtual Rezerwacja? Rezerwacja { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DataDodania { get; set; } = DateTime.UtcNow;
    }
}
