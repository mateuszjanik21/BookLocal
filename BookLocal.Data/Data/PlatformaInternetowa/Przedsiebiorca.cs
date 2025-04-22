using System.ComponentModel.DataAnnotations;
using System.Runtime.Intrinsics.X86;

namespace BookLocal.Data.Data.PlatformaInternetowa
{
    public class Przedsiebiorca
    {
        [Key]
        public int IdPrzedsiebiorcy { get; set; }

        [Required(ErrorMessage = "Wprowadź login.")]
        [MaxLength(50)]
        public required string Login { get; set; }
        [Required(ErrorMessage = "Wprowadź hasło.")]
        public required string HasloHash { get; set; }
        [Required(ErrorMessage = "Wprowadź e-mail kontaktowy.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy adres e-mail.")]
        [MaxLength(100)]
        public required string Email { get; set; }
        [Required(ErrorMessage = "Wprowadź imię.")]
        [MaxLength(50)]
        public required string Imie { get; set; }
        [Required(ErrorMessage = "Wprowadź nazwisko.")]
        [MaxLength(70)]
        public required string Nazwisko { get; set; }
        public bool CzyAktywny { get; set; } = true;
        public virtual ICollection<Firma> ZarzadzaneFirmy { get; set; } = new List<Firma>();
        public virtual ICollection<Wiadomosc> WyslaneWiadomosci { get; set; } = new List<Wiadomosc>();
    }
}
