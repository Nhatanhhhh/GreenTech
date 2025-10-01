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
    [Table("point_transactions")]
    public class PointTransaction
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("transaction_type")]
        public PointTransactionType TransactionType { get; set; }

        [Required]
        [Column("points")]
        public int Points { get; set; }

        [Required]
        [Column("reference_type")]
        public ReferenceType ReferenceType { get; set; }

        [Column("reference_id")]
        public int? ReferenceId { get; set; }

        [Column("point_earning_rule_id")]
        public int? PointEarningRuleId { get; set; }

        [Column("description")]
        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [Column("points_before")]
        public int PointsBefore { get; set; }

        [Required]
        [Column("points_after")]
        public int PointsAfter { get; set; }

        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("PointEarningRuleId")]
        public virtual PointEarningRule PointEarningRule { get; set; }
    }
}
