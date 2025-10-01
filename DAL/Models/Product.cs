using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("products")]
    public class Product
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("sku")]
        [MaxLength(100)]
        public string Sku { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(255)]
        public string Name { get; set; }

        [Column("slug")]
        [MaxLength(255)]
        public string Slug { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("short_description")]
        [MaxLength(500)]
        public string ShortDescription { get; set; }

        [Required]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Required]
        [Column("supplier_id")]
        public int SupplierId { get; set; }

        [Column("cost_price", TypeName = "decimal(12,2)")]
        public decimal CostPrice { get; set; } = 0;

        [Column("sell_price", TypeName = "decimal(12,2)")]
        public decimal SellPrice { get; set; } = 0;

        [Column("quantity")]
        public int Quantity { get; set; } = 0;

        [Column("image")]
        [MaxLength(255)]
        public string Image { get; set; }

        [Column("care_instructions")]
        public string CareInstructions { get; set; }

        [Column("plant_size")]
        [MaxLength(100)]
        public string PlantSize { get; set; }

        [Column("weight", TypeName = "decimal(8,2)")]
        public decimal? Weight { get; set; }

        [Column("dimensions")]
        [MaxLength(100)]
        public string Dimensions { get; set; }

        [Column("tags")]
        [MaxLength(500)]
        public string Tags { get; set; }

        [Column("points_earned")]
        public int PointsEarned { get; set; } = 1;

        [Column("is_featured")]
        public bool IsFeatured { get; set; } = false;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("seo_title")]
        [MaxLength(255)]
        public string SeoTitle { get; set; }

        [Column("seo_description")]
        [MaxLength(500)]
        public string SeoDescription { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; }

        public virtual ICollection<ProductImage> ProductImages { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ProductRatingStat ProductRatingStat { get; set; }
    }
}
