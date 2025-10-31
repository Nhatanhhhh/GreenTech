using DAL.DTOs.Cart;

namespace BLL.Service.Cart.Interface
{
    public interface ICartService
    {
        Task<CartResponseDTO> GetCartByUserIdAsync(int userId);
        Task<CartResponseDTO> AddToCartAsync(AddToCartDTO addToCartDTO);
        Task<CartResponseDTO> UpdateCartItemAsync(UpdateCartItemDTO updateCartItemDTO);
        Task<bool> RemoveCartItemAsync(int cartItemId, int userId);
        Task<bool> ClearCartAsync(int userId);
        Task<decimal> CalculateCartTotalAsync(int cartId);
        Task<bool> ValidateCartItemAsync(int cartItemId, int userId);
    }
}
