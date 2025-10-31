namespace DAL.DTOs.Payment
{
    public class PaymentInitResult
    {
        public string GatewayTransactionId { get; set; }
        public string PayUrl { get; set; }
    }
}
