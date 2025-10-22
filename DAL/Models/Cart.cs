using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    [Table("carts")]
    public class Cart
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("session_id")]
        [MaxLength(255)]
        public string? SessionId { get; set; }

        [Column("coupon_id")]
        public int? CouponId { get; set; }

        [Column("total_items")]
        public int TotalItems { get; set; } = 0;

        [Column("subtotal", TypeName = "decimal(12,2)")]
        public decimal Subtotal { get; set; } = 0;

        [Column("discount_amount", TypeName = "decimal(12,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("CouponId")]
        public virtual Coupon Coupon { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
