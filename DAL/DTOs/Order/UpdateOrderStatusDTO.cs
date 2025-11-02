using System.ComponentModel.DataAnnotations;
using DAL.Models.Enum;
using DAL.ValidationAttributes;

namespace DAL.DTOs.Order
{
    public class UpdateOrderStatusDTO
    {
        [Required]
        [OrderExists]
        public int OrderId { get; set; }

        [Required]
        public OrderStatus Status { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; } // Optional field - can be null or empty

        public int? UpdatedBy { get; set; }
    }
}
