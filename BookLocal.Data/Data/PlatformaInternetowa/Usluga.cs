using System.ComponentModel.DataAnnotations;

namespace BookLocal.Data.Data.PlatformaInternetowa
{
    public class Usluga
    {
        [Key]
        public int IdUslugi { get; set; }

        [Required(ErrorMessage = "Wprowadź nazwę usługi.")]
        [MaxLength(150)]
        public required string Nazwa { get; set; }

        [Required(ErrorMessage = "Wprowadź opis usługi.")]
        [DataType(DataType.MultilineText)]
        public required string Opis { get; set; }

        [Required(ErrorMessage = "Wprowadź adres URL zdjęcia.")]
        [MaxLength(255)]
        [DataType(DataType.ImageUrl)]
        [Display(Name = "Zdjęcie")]
        public required string ZdjecieUrl { get; set; }

        public virtual ICollection<SzczegolyUslugi> Szczegoly { get; set; } = new List<SzczegolyUslugi>();
    }
}
