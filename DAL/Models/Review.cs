using DAL.Models.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("reviews")]
    public class Review
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("order_item_id")]
        public int OrderItemId { get; set; }

        [Required]
        [Column("rating")]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("media_urls")]
        public string MediaUrls { get; set; }

        [Column("helpful_count")]
        public int HelpfulCount { get; set; } = 0;

        [Column("is_anonymous")]
        public bool IsAnonymous { get; set; } = false;

        [Column("status")]
        public ReviewStatus Status { get; set; } = ReviewStatus.APPROVED;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("OrderItemId")]
        public virtual OrderItem OrderItem { get; set; }

        public virtual ICollection<ReviewReply> ReviewReplies { get; set; }
        public virtual ICollection<ReviewVote> ReviewVotes { get; set; }
    }
}
