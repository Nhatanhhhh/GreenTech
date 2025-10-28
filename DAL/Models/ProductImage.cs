using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    [Table("product_images")]
    public class ProductImage
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required]
        [Column("image_url")]
        [MaxLength(500)]
        public string ImageUrl { get; set; }

        [Column("alt_text")]
        [MaxLength(255)]
        public string AltText { get; set; }

        [Column("is_primary")]
        public bool IsPrimary { get; set; } = false;

        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
