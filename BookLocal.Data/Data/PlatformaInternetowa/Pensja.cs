using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookLocal.Data.Data.PlatformaInternetowa
{
    public class Pensja
    {
        [Key]
        public int IdPensjii { get; set; }

        [Required(ErrorMessage = "Należy wskazać pracownika, którego dotyczy pensja.")]
        public required int PracownikId { get; set; }

        [ForeignKey("PracownikId")]
        public virtual Pracownik? Pracownik { get; set; }

        [Required(ErrorMessage = "Podaj kwotę podstawową pensji.")]
        [Column(TypeName = "decimal(18, 2)")]
        public required decimal KwotaPodstawowa { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Premia { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Potracenia { get; set; } = 0;

        [Required(ErrorMessage = "Podaj datę początku okresu rozliczeniowego.")]
        [DataType(DataType.Date)]
        public required DateOnly OkresOd { get; set; }

        [Required(ErrorMessage = "Podaj datę końca okresu rozliczeniowego.")]
        [DataType(DataType.Date)]
        public required DateOnly OkresDo { get; set; }

        [Required(ErrorMessage = "Podaj status wypłaty.")]
        [MaxLength(50)]
        public required string StatusWyplaty { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? DataWyplaty { get; set; }

        [DataType(DataType.MultilineText)]
        public string? Uwagi { get; set; } 
        public DateTime DataUtworzeniaZapisu { get; set; } = DateTime.UtcNow;

        public int? ZarzadzajacyPrzedsiębiorcaId { get; set; }

        [ForeignKey("ZarzadzajacyPrzedsiębiorcaId")]
        public virtual Przedsiebiorca? ZarzadzajacyPrzedsiębiorca { get; set; }
    }
}
