using CartItemModel = DAL.Models.CartItem;
using CartModel = DAL.Models.Cart;

namespace DAL.Repositories.Cart.Interface
{
    public interface ICartRepository
    {
        Task<CartModel> GetCartByUserIdAsync(int userId);
        Task<CartModel> CreateCartAsync(CartModel cart);
        Task<CartModel> UpdateCartAsync(CartModel cart);
        Task<CartItemModel> AddCartItemAsync(CartItemModel cartItem);
        Task<CartItemModel> UpdateCartItemAsync(CartItemModel cartItem);
        Task<bool> RemoveCartItemAsync(int cartItemId);
        Task<bool> ClearCartAsync(int userId);
        Task<CartItemModel> GetCartItemByIdAsync(int cartItemId);
        Task<CartItemModel> GetCartItemByProductIdAsync(int cartId, int productId);
        Task<IEnumerable<CartItemModel>> GetCartItemsByCartIdAsync(int cartId);
        Task<bool> CartExistsAsync(int userId);
        Task<bool> CartItemExistsAsync(int cartItemId);
    }
}
