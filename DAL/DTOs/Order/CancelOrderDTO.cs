using System.ComponentModel.DataAnnotations;
using DAL.ValidationAttributes;

namespace DAL.DTOs.Order
{
    public class CancelOrderDTO
    {
        [Required]
        [OrderExists]
        [OrderOwnership(
            UserIdPropertyName = "UserId",
            ErrorMessage = "Bạn không có quyền hủy đơn hàng này. Chỉ có thể hủy đơn hàng của chính bạn."
        )]
        public int OrderId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; }

        public int? UserId { get; set; } // Customer ID nếu customer tự hủy
    }
}
