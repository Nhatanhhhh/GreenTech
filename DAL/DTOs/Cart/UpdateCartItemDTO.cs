using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.Cart
{
    public class UpdateCartItemDTO
    {
        [Required(ErrorMessage = "Cart Item ID là bắt buộc")]
        public int CartItemId { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }
    }
}
