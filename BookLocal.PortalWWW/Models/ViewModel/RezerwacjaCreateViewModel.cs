using System;
using System.ComponentModel.DataAnnotations;

namespace BookLocal.PortalWWW.Models.ViewModel
{
    public class RezerwacjaCreateViewModel
    {
        [Required]
        public int SzczegolyUslugiId { get; set; }
        [Required]
        public int PracownikId { get; set; }

        public string? NazwaUslugi { get; set; }
        public string? OpisSzczegolowUslugi { get; set; }
        public decimal Cena { get; set; }
        public int CzasTrwaniaMinuty { get; set; }
        public string? ImieNazwiskoPracownika { get; set; }
        public string? ZdjeciePracownikaUrl { get; set; }
        public string? StanowiskoPracownika { get; set; }

        [Required(ErrorMessage = "Proszę wybrać datę wizyty.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data Wizyty")]
        public DateTime? WybranaData { get; set; }

        [Required(ErrorMessage = "Proszę wybrać godzinę wizyty.")]
        [DataType(DataType.Time)]
        [Display(Name = "Godzina Wizyty")]
        [Range(typeof(TimeSpan), "08:00:00", "18:00:00", ErrorMessage = "Godzina wizyty musi być między 08:00 a 18:00.")]
        public TimeSpan? WybranaGodzina { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [StringLength(100)]
        [Display(Name = "Twoje Imię")]
        public string? ImieKlienta { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [StringLength(150)]
        [Display(Name = "Twoje Nazwisko")]
        public string? NazwiskoKlienta { get; set; }

        [Required(ErrorMessage = "Numer telefonu jest wymagany.")]
        [Phone(ErrorMessage = "Nieprawidłowy format numeru telefonu.")]
        [StringLength(20)]
        [Display(Name = "Twój Telefon Kontaktowy")]
        public string? TelefonKlienta { get; set; }
    }
}
