namespace DAL.DTOs.Wallet
{
    public class WalletTransactionDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string PaymentGateway { get; set; }
        public string GatewayTransactionId { get; set; }
        public int? OrderId { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
