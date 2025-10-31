using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.Wallet
{
    public class TopUpRequestDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1000, double.MaxValue, ErrorMessage = "Số tiền nạp tối thiểu là 1,000đ")]
        public decimal Amount { get; set; }

        // Use string to be flexible from UI; service will map to enum
        [Required]
        public string Gateway { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
