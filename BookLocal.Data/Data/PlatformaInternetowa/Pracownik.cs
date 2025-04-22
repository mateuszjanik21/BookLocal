using BookLocal.Data.Data.CMS;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Data.PlatformaInternetowa
{
    public class Pracownik
    {
        [Key]
        public int IdPracownika { get; set; }

        [Required(ErrorMessage = "Wprowadź imię pracownika.")]
        [MaxLength(50)]
        public required string Imie { get; set; }

        [Required(ErrorMessage = "Wprowadź nazwisko pracownika.")]
        [MaxLength(70)]
        public required string Nazwisko { get; set; }

        [DataType(DataType.MultilineText)]
        public string? Bio { get; set; }

        [MaxLength(255)]
        [DataType(DataType.ImageUrl)]
        [Display(Name = "Zdjęcie")]
        public string? ZdjecieUrl { get; set; }

        [MaxLength(100)]
        public string? Stanowisko { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        [Display(Name = "Email")]
        public string? EmailKontaktowy { get; set; }

        [Phone]
        [MaxLength(20)]
        [Display(Name = "Telefon")]
        public string? TelefonKontaktowy { get; set; }

        [Required(ErrorMessage = "Wskaż firmę, dla której pracuje pracownik.")]
        public int FirmaId { get; set; }

        [ForeignKey("FirmaId")]
        public virtual Firma? Firma { get; set; }

        [Display(Name = "Status aktywności")]
        public bool CzyAktywny { get; set; } = true;
        public virtual ICollection<SzczegolyUslugi> OferowaneUslugiSzczegoly { get; set; } = new List<SzczegolyUslugi>();
        public virtual ICollection<Rezerwacja> WykonaneRezerwacje { get; set; } = new List<Rezerwacja>();
        public virtual ICollection<Opinia> OtrzymaneOpinie { get; set; } = new List<Opinia>();
        public virtual ICollection<Adres> Adresy { get; set; } = new List<Adres>();
        public virtual ICollection<Konwersacja> Konwersacje { get; set; } = new List<Konwersacja>();
        public virtual ICollection<TransakcjaRozliczeniowa> TransakcjeRozliczeniowe { get; set; } = new List<TransakcjaRozliczeniowa>();
        public virtual ICollection<Pensja> WyplatyPensji { get; set; } = new List<Pensja>();
    }
}
