using BookLocal.Data.Data.PlatformaInternetowa;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Data.CMS
{
    public class ZawartoscCms
    {
        [Key]
        public int IdZawartosci { get; set; }

        [Required(ErrorMessage = "Wprowadź nazwę sekcji/klucz.")]
        [MaxLength(100)]
        public required string Sekcja { get; set; }

        [Required(ErrorMessage = "Wprowadź treść.")]
        [DataType(DataType.MultilineText)]
        public required string Tresc { get; set; }

        public string? NazwaIkony { get; set; }

        [Required(ErrorMessage = "Wskaż autora.")]
        public required int PracownikId { get; set; }

        [ForeignKey(nameof(PracownikId))]
        public virtual Pracownik? Autor { get; set; }

        public DateTime DataModyfikacji { get; set; } = DateTime.UtcNow;

        [Display(Name = "Powiązana Sekcja CMS")]
        public int? SekcjaCmsId { get; set; }

        [ForeignKey(nameof(SekcjaCmsId))]
        public virtual SekcjaCms? SekcjaPowiazana { get; set; }
    }
}
