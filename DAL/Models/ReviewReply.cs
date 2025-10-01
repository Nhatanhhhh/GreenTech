using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("review_replies")]
    public class ReviewReply
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("review_id")]
        public int ReviewId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("content")]
        public string Content { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("ReviewId")]
        public virtual Review Review { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
