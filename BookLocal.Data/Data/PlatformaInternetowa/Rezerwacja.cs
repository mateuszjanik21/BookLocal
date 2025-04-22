using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Data.PlatformaInternetowa
{
    public class Rezerwacja
    {
        [Key]
        public int IdRezerwacji { get; set; }

        [Required(ErrorMessage = "Wprowadź datę rezerwacji.")]
        [DataType(DataType.DateTime)]
        public required DateTime DataRezerwacji { get; set; }

        [Required(ErrorMessage = "Wprowadź status.")]
        [MaxLength(50)]
        public required string Status { get; set; }

        [MaxLength(50)]
        public string? ImieKlienta { get; set; }

        [MaxLength(70)]
        public string? NazwiskoKlienta { get; set; } 

        [MaxLength(20)]
        [Phone]
        public string? TelefonKlienta { get; set; } 

        public int? UzytkownikId { get; set; } 
        [ForeignKey(nameof(UzytkownikId))]
        public virtual Uzytkownik? Uzytkownik { get; set; }

        public int? WykonujacyPracownikId { get; set; }
        [ForeignKey("WykonujacyPracownikId")]
        public virtual Pracownik? WykonujacyPracownik { get; set; }

        public int? ObslugujacyPrzedsiębiorcaId { get; set; }
        [ForeignKey("ObslugujacyPrzedsiębiorcaId")]
        public virtual Przedsiebiorca? ObslugujacyPrzedsiębiorca { get; set; }

        public int? SzczegolyUslugiId { get; set; }
        [ForeignKey("SzczegolyUslugiId")]
        public virtual SzczegolyUslugi? SzczegolyUslugi { get; set; }
    }
}
