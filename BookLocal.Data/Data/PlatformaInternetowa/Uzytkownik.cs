using System.ComponentModel.DataAnnotations;

namespace BookLocal.Data.Data.PlatformaInternetowa
{
    public class Uzytkownik
    {
        [Key]
        public int IdUzytkownika { get; set; }

        [Required(ErrorMessage = "Wprowadź login.")]
        [MaxLength(50)]
        public required string Login { get; set; }

        [Required(ErrorMessage = "Wprowadź hasło.")]
        public required string HasloHash { get; set; }

        [Required(ErrorMessage = "Wprowadź imię.")]
        [MaxLength(50)]
        public required string Imie { get; set; }

        [Required(ErrorMessage = "Wprowadź nazwisko.")]
        [MaxLength(70)]
        public required string Nazwisko { get; set; }

        [Phone]
        [MaxLength(20)]
        [Display(Name = "Telefon")]
        public string? TelefonKontaktowy { get; set; }

        [Required(ErrorMessage = "Wprowadź adres e-mail.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy adres e-mail.")]
        [MaxLength(100)]
        public required string Email { get; set; }

        public virtual ICollection<Rezerwacja> Rezerwacje { get; set; } = new List<Rezerwacja>();
        public virtual ICollection<Opinia> WystawioneOpinie { get; set; } = new List<Opinia>();
        public virtual ICollection<Adres> Adresy { get; set; } = new List<Adres>();
        public virtual ICollection<Konwersacja> Konwersacje { get; set; } = new List<Konwersacja>();
    }
}
