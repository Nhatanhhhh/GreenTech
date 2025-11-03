using DAL.Models.Enum;

namespace DAL.DTOs.Order
{
    public class OrderResponseDTO
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public int? CouponId { get; set; }
        public string CouponCode { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentGateway? PaymentGateway { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total { get; set; }
        public decimal WalletAmountUsed { get; set; }
        public string GatewayTransactionId { get; set; }
        public string ShippingAddress { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public int PointsEarned { get; set; }
        public DateTime? PointsAwardedAt { get; set; }
        public string Note { get; set; }
        public string? CancelledReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderItemResponseDTO> OrderItems { get; set; } =
            new List<OrderItemResponseDTO>();
    }
}
