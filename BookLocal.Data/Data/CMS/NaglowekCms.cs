using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLocal.Data.Data.CMS
{
    public class NaglowekCms
    {
        [Key]
        public int IdNapisu { get; set; }

        [Required(ErrorMessage = "Wprowadź tytuł nagłówku.")]
        [MaxLength(100)]
        public required string Naglowek { get; set; }

        [Required(ErrorMessage = "Wprowadź treść.")]
        [DataType(DataType.MultilineText)]
        public required string Tresc { get; set; }
    }
}
