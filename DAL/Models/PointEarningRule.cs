using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("point_earning_rules")]
    public class PointEarningRule
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(255)]
        public string Name { get; set; }

        [Column("points_per_amount", TypeName = "decimal(10,2)")]
        public decimal PointsPerAmount { get; set; } = 1;

        [Column("min_order_amount", TypeName = "decimal(12,2)")]
        public decimal MinOrderAmount { get; set; } = 0;

        [Column("max_points_per_order")]
        public int? MaxPointsPerOrder { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("valid_from")]
        public DateTime ValidFrom { get; set; }

        [Column("valid_until")]
        public DateTime? ValidUntil { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("CreatedBy")]
        public virtual User Creator { get; set; }

        public virtual ICollection<PointTransaction> PointTransactions { get; set; }
    }
}
