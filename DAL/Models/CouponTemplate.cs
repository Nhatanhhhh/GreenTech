using DAL.Models.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;

namespace DAL.Models
{
    [Table("coupon_templates")]
    public class CouponTemplate
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(255)]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Required]
        [Column("discount_type")]
        public DiscountType DiscountType { get; set; }

        [Required]
        [Column("discount_value", TypeName = "decimal(10,2)")]
        public decimal DiscountValue { get; set; }

        [Column("min_order_amount", TypeName = "decimal(12,2)")]
        public decimal MinOrderAmount { get; set; } = 0;

        [Required]
        [Column("points_cost")]
        public int PointsCost { get; set; }

        [Column("usage_limit_per_user")]
        public int UsageLimitPerUser { get; set; } = 1;

        [Column("total_usage_limit")]
        public int? TotalUsageLimit { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("valid_days")]
        public int ValidDays { get; set; } = 30;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual ICollection<Coupon> Coupons { get; set; }
    }
}
