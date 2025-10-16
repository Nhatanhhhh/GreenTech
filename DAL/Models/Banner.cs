using DAL.Models.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    [Table("banners")]
    public class Banner
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("title")]
        [MaxLength(255)]
        public string Title { get; set; }

        [Column("description")]
        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [Column("image_url")]
        [MaxLength(500)]
        public string ImageUrl { get; set; }

        [Column("link_url")]
        [MaxLength(500)]
        public string LinkUrl { get; set; }

        [Required]
        [Column("position")]
        public BannerPosition Position { get; set; }

        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("click_count")]
        public int ClickCount { get; set; } = 0;

        [Required]
        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("CreatedBy")]
        public virtual User Creator { get; set; }
    }
}
