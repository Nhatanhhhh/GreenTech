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
    [Table("notifications")]
    public class Notification
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("title")]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        [Column("message")]
        public string Message { get; set; }

        [Required]
        [Column("type")]
        public NotificationType Type { get; set; }

        [Column("reference_id")]
        public int? ReferenceId { get; set; }

        [Column("priority")]
        public NotificationPriority Priority { get; set; } = NotificationPriority.MEDIUM;

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("read_at")]
        public DateTime? ReadAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }

}
