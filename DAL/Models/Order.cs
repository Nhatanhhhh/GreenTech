using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DAL.Models.Enum;

namespace DAL.Models
{
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("order_number")]
        [MaxLength(50)]
        public string OrderNumber { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("coupon_id")]
        public int? CouponId { get; set; }

        [Column("status")]
        public OrderStatus Status { get; set; } = OrderStatus.PENDING;

        [Column("payment_status")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.PENDING;

        [Column("payment_gateway")]
        public PaymentGateway? PaymentGateway { get; set; }

        [Required]
        [Column("subtotal", TypeName = "decimal(12,2)")]
        public decimal Subtotal { get; set; }

        [Column("discount_amount", TypeName = "decimal(12,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column("shipping_fee", TypeName = "decimal(12,2)")]
        public decimal ShippingFee { get; set; } = 0;

        [Required]
        [Column("total", TypeName = "decimal(12,2)")]
        public decimal Total { get; set; }

        [Required]
        [Column("wallet_amount_used", TypeName = "decimal(12,2)")]
        public decimal WalletAmountUsed { get; set; }

        [Column("gateway_transaction_id")]
        [MaxLength(255)]
        public string GatewayTransactionId { get; set; }

        [Required]
        [Column("shipping_address")]
        public string ShippingAddress { get; set; }

        [Required]
        [Column("customer_name")]
        [MaxLength(255)]
        public string CustomerName { get; set; }

        [Required]
        [Column("customer_phone")]
        [MaxLength(15)]
        public string CustomerPhone { get; set; }

        [Column("points_earned")]
        public int PointsEarned { get; set; } = 0;

        [Column("points_awarded_at")]
        public DateTime? PointsAwardedAt { get; set; }

        [Column("note")]
        public string Note { get; set; }

        [Column("cancelled_reason")]
        [MaxLength(500)]
        public string CancelledReason { get; set; }

        [Column("cancelled_at")]
        public DateTime? CancelledAt { get; set; }

        [Column("shipped_at")]
        public DateTime? ShippedAt { get; set; }

        [Column("delivered_at")]
        public DateTime? DeliveredAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("CouponId")]
        public virtual Coupon Coupon { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<WalletTransaction> WalletTransactions { get; set; }
    }
}
