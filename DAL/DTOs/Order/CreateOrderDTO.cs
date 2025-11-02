using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.Order
{
    public class CreateOrderDTO
    {
        [Required]
        public int UserId { get; set; }

        public int? CouponId { get; set; }

        [Required]
        [MaxLength(255)]
        public string CustomerName { get; set; }

        [Required]
        [MaxLength(15)]
        public string CustomerPhone { get; set; }

        [Required]
        public string ShippingAddress { get; set; }

        public string? Note { get; set; }

        [Required]
        public decimal ShippingFee { get; set; }

        [Required]
        public decimal WalletAmountUsed { get; set; }
    }
}
