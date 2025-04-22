using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLocal.Data.Data.CMS
{
    public class OdnosnikCms
    {
        [Key]
        public int IdOdnosnika { get; set; }

        [Required(ErrorMessage = "Wprowadź nazwę.")]
        [MaxLength(50)]
        public required string Nazwa { get; set; }

        [Required(ErrorMessage = "Wprowadź odnośnik, np. Usługi, Specjaliści.")]
        [MaxLength(50)]
        public required string Odnosnik { get; set; }
    }
}
