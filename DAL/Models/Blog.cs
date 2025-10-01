using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("blogs")]
    public class Blog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("title")]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        [Column("slug")]
        [MaxLength(255)]
        public string Slug { get; set; }

        [Column("excerpt")]
        [MaxLength(500)]
        public string Excerpt { get; set; }

        [Required]
        [Column("content")]
        public string Content { get; set; }

        [Column("featured_image")]
        [MaxLength(255)]
        public string FeaturedImage { get; set; }

        [Required]
        [Column("author_id")]
        public int AuthorId { get; set; }

        [Column("category_id")]
        public int? CategoryId { get; set; }

        [Column("tags")]
        [MaxLength(500)]
        public string Tags { get; set; }

        [Column("view_count")]
        public int ViewCount { get; set; } = 0;

        [Column("is_featured")]
        public bool IsFeatured { get; set; } = false;

        [Column("is_published")]
        public bool IsPublished { get; set; } = false;

        [Column("published_at")]
        public DateTime? PublishedAt { get; set; }

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
        [ForeignKey("AuthorId")]
        public virtual User Author { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
    }
}
