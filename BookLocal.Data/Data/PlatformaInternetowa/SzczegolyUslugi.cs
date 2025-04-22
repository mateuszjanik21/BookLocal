using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Data.PlatformaInternetowa
{
    public class SzczegolyUslugi
    {
        [Key]
        public int IdSzczegolowUslugi { get; set; }

        [Required(ErrorMessage = "Wprowadź opis szczegółów usługi.")]
        [DataType(DataType.MultilineText)]
        public required string Opis { get; set; }

        [Required(ErrorMessage = "Podaj cenę.")]
        [Range(0, 10000, ErrorMessage = "Cena musi być większa lub równa 0.")]
        [Column(TypeName = "decimal(18, 2)")]
        public required decimal Cena { get; set; }

        [Required(ErrorMessage = "Podaj czas trwania usługi w minutach.")]
        [Range(1, 600, ErrorMessage = "Czas trwania musi być między 1 a 600 minut.")]
        [Display(Name = "Czas trwania (minuty)")]
        public int CzasTrwaniaMinuty { get; set; }

        [Required(ErrorMessage = "Wskaż usługę.")]
        public required int UslugaId { get; set; }

        [ForeignKey("UslugaId")]
        public virtual Usluga? Usluga { get; set; }

        [Required(ErrorMessage = "Wskaż pracownika oferującego te szczegóły.")]
        public required int PracownikId { get; set; }

        [ForeignKey("PracownikId")]
        public virtual Pracownik? Pracownik { get; set; }
    }
}
