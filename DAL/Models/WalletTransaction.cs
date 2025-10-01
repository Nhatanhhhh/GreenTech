using DAL.Models.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    [Table("wallet_transactions")]
    public class WalletTransaction
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("transaction_type")]
        public TransactionType TransactionType { get; set; }

        [Required]
        [Column("amount", TypeName = "decimal(12,2)")]
        public decimal Amount { get; set; }

        [Column("payment_gateway")]
        public PaymentGateway? PaymentGateway { get; set; }

        [Required]
        [Column("gateway_transaction_id")]
        [MaxLength(255)]
        public string GatewayTransactionId { get; set; }

        [Column("order_id")]
        public int? OrderId { get; set; }

        [Column("status")]
        public TransactionStatus Status { get; set; } = TransactionStatus.PENDING;

        [Column("description")]
        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [Column("balance_before", TypeName = "decimal(12,2)")]
        public decimal BalanceBefore { get; set; }

        [Required]
        [Column("balance_after", TypeName = "decimal(12,2)")]
        public decimal BalanceAfter { get; set; }

        [Column("processed_at")]
        public DateTime? ProcessedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }
}
