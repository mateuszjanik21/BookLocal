using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookLocal.Data.Data.PlatformaInternetowa
{
    public class TransakcjaRozliczeniowa
    {
        [Key]
        public int IdTransakcji { get; set; }

        [Required(ErrorMessage = "Należy wskazać pracownika, którego dotyczy rozliczenie.")]
        public required int PracownikId { get; set; }

        [ForeignKey("PracownikId")]
        public virtual Pracownik? Pracownik { get; set; }

        public int? RezerwacjaId { get; set; }
        [ForeignKey("RezerwacjaId")]
        public virtual Rezerwacja? Rezerwacja { get; set; }

        [Required(ErrorMessage = "Należy podać kwotę brutto transakcji.")]
        [Column(TypeName = "decimal(18, 2)")]
        public required decimal KwotaBrutto { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ProwizjaPlatformy { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ProwizjaFirmy { get; set; } = 0; 

        [Required(ErrorMessage = "Należy obliczyć kwotę netto dla pracownika.")]
        [Column(TypeName = "decimal(18, 2)")]
        public required decimal KwotaNettoDlaPracownika { get; set; }

        [Required(ErrorMessage = "Należy określić status rozliczenia.")]
        [MaxLength(50)]
        public required string StatusRozliczenia { get; set; }

        public DateTime DataUtworzenia { get; set; } = DateTime.UtcNow;
        public DateTime? DataOstatniejZmianyStatusu { get; set; }

        [DataType(DataType.MultilineText)]
        public string? Uwagi { get; set; }

        public int? ZatwierdzajacyPrzedsiębiorcaId { get; set; }
        [ForeignKey("ZatwierdzajacyPrzedsiębiorcaId")]
        public virtual Przedsiebiorca? ZatwierdzajacyPrzedsiębiorca { get; set; }
    }
}
