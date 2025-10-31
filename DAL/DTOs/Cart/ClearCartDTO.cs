using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.Cart
{
    public class ClearCartDTO
    {
        [Required(ErrorMessage = "User ID là bắt buộc")]
        public int UserId { get; set; }
    }
}
