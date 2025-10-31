using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.Cart
{
    public class AddToCartDTO
    {
        [Required(ErrorMessage = "Product ID là bắt buộc")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; } = 1;

        [Required(ErrorMessage = "User ID là bắt buộc")]
        public int UserId { get; set; }
    }
}
