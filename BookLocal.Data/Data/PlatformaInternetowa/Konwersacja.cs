using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookLocal.Data.Data.PlatformaInternetowa
{
    public class Konwersacja
    {
        [Key]
        public int IdKonwersacji { get; set; }

        [Required]
        public required int UzytkownikId { get; set; }

        [ForeignKey("UzytkownikId")]
        public virtual Uzytkownik? Uzytkownik { get; set; }

        [Required]
        public required int PracownikId { get; set; }

        [ForeignKey("PracownikId")]
        public virtual Pracownik? Pracownik { get; set; }

        [MaxLength(200)]
        public string? Temat { get; set; }

        public DateTime DataUtworzenia { get; set; } = DateTime.UtcNow;
        public DateTime DataOstatniejWiadomosci { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Wiadomosc> Wiadomosci { get; set; } = new List<Wiadomosc>();
    }
}
