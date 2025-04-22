using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookLocal.Data.Data.PlatformaInternetowa
{
    public class Wiadomosc
    {
        [Key] 
        public int IdWiadomosci { get; set; }

        [Required] 
        public required int KonwersacjaId { get; set; }

        [ForeignKey("KonwersacjaId")] 
        public virtual Konwersacja? Konwersacja { get; set; }
        public int? NadawcaUzytkownikId { get; set; }

        [ForeignKey("NadawcaUzytkownikId")] 
        public virtual Uzytkownik? NadawcaUzytkownik { get; set; }
        public int? NadawcaPrzedsiębiorcaId { get; set; }

        [ForeignKey("NadawcaPrzedsiębiorcaId")] 
        public virtual Przedsiebiorca? NadawcaPrzedsiębiorca { get; set; }

        [Required]
        [DataType(DataType.MultilineText)] 
        public required string Tresc { get; set; }

        public DateTime DataWyslania { get; set; } = DateTime.UtcNow;
        public bool CzyOdczytana { get; set; } = false;
        public DateTime? DataOdczytania { get; set; }
    }
}
