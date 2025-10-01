using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("review_votes")]
    public class ReviewVote
    {
        [Column("review_id")]
        public int ReviewId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("is_helpful")]
        public bool IsHelpful { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("ReviewId")]
        public virtual Review Review { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }

}
