using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("product_rating_stats")]
    public class ProductRatingStat
    {
        [Key]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("total_reviews")]
        public int TotalReviews { get; set; } = 0;

        [Column("average_rating", TypeName = "decimal(3,2)")]
        public decimal AverageRating { get; set; } = 0;

        [Column("star_1_count")]
        public int Star1Count { get; set; } = 0;

        [Column("star_2_count")]
        public int Star2Count { get; set; } = 0;

        [Column("star_3_count")]
        public int Star3Count { get; set; } = 0;

        [Column("star_4_count")]
        public int Star4Count { get; set; } = 0;

        [Column("star_5_count")]
        public int Star5Count { get; set; } = 0;

        [Column("with_media_count")]
        public int WithMediaCount { get; set; } = 0;

        [Column("with_content_count")]
        public int WithContentCount { get; set; } = 0;

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
