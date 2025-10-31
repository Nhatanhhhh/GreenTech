namespace DAL.DTOs.Wallet
{
    public class TopUpResponseDTO
    {
        public int WalletTransactionId { get; set; }
        public string GatewayTransactionId { get; set; }
        public string Gateway { get; set; }
        public decimal Amount { get; set; }
        public string PayUrl { get; set; }
    }
}
