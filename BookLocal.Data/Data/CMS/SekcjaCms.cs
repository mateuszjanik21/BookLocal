using BookLocal.Data.Data.PlatformaInternetowa;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Data.CMS
{
    public class SekcjaCms
    {
        [Key]
        public int IdSekcji { get; set; }

        [Required]
        [MaxLength(100)]
        public required string KluczSekcji { get; set; }

        [Required]
        public required int Kolejnosc { get; set; } = 0;

        public int? LastModifiedByPracownikId { get; set; }
        [ForeignKey(nameof(LastModifiedByPracownikId))]
        public virtual Pracownik? LastModifiedByPracownik { get; set; }
        public DateTime? LastModifiedDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ZawartoscCms> PowiazaneZawartosci { get; set; } = new List<ZawartoscCms>();
    }
}
