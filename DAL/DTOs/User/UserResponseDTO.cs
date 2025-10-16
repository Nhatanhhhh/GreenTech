
namespace DAL.DTOs.User
{
    public class UserResponseDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string SpecificAddress { get; set; }
        public string Avatar { get; set; }
        public int Points { get; set; }
        public decimal WalletBalance { get; set; }
        public string Status { get; set; }
    }
}
