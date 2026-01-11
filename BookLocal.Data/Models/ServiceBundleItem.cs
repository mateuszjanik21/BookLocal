using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class ServiceBundleItem
    {
        [Key]
        public int ServiceBundleItemId { get; set; }

        public int ServiceBundleId { get; set; }
        [ForeignKey("ServiceBundleId")]
        public virtual ServiceBundle ServiceBundle { get; set; }

        public int ServiceVariantId { get; set; }
        [ForeignKey("ServiceVariantId")]
        public virtual ServiceVariant ServiceVariant { get; set; }
    }
}