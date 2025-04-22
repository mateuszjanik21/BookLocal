using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookLocal.Data.Data.PlatformaInternetowa
{
    public class Firma
    {
        [Key]
        public int IdFirmy { get; set; }

        [Required(ErrorMessage = "Wprowadź nazwę firmy.")]
        [MaxLength(150)]
        public required string Nazwa { get; set; }

        [DataType(DataType.MultilineText)]
        public string? Opis { get; set; }

        [Required(ErrorMessage = "Wskaż właściciela firmy (Przedsiębiorcę).")]
        public required int WlascicielId { get; set; }

        [ForeignKey("WlascicielId")]
        public virtual Przedsiebiorca? Wlasciciel { get; set; }
        public int? AdresId { get; set; }

        [ForeignKey("AdresId")]
        public virtual Adres? GlownyAdres { get; set; }

        public virtual ICollection<Pracownik> Pracownicy { get; set; } = new List<Pracownik>();

        [MaxLength(15)] 
        public string? NIP { get; set; }

        [MaxLength(14)] 
        public string? REGON { get; set; }
    }
}
