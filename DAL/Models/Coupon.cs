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
    [Table("coupons")]
    public class Coupon
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("code")]
        [MaxLength(50)]
        public string Code { get; set; }

        [Column("template_id")]
        public int? TemplateId { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [Column("discount_type")]
        public DiscountType DiscountType { get; set; }

        [Required]
        [Column("discount_value", TypeName = "decimal(10,2)")]
        public decimal DiscountValue { get; set; }

        [Column("min_order_amount", TypeName = "decimal(12,2)")]
        public decimal MinOrderAmount { get; set; } = 0;

        [Column("usage_limit")]
        public int UsageLimit { get; set; } = 1;

        [Column("used_count")]
        public int UsedCount { get; set; } = 0;

        [Column("source")]
        public CouponSource Source { get; set; } = CouponSource.MANUAL;

        [Column("points_used")]
        public int PointsUsed { get; set; } = 0;

        [Required]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("TemplateId")]
        public virtual CouponTemplate CouponTemplate { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
