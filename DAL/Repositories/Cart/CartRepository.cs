using DAL.Context;
using DAL.Repositories.Cart.Interface;
using Microsoft.EntityFrameworkCore;
using CartItemModel = DAL.Models.CartItem;
using CartModel = DAL.Models.Cart;

namespace DAL.Repositories.Cart
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CartModel> GetCartByUserIdAsync(int userId)
        {
            return await _context
                .Carts.Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .Include(c => c.Coupon)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<CartModel> CreateCartAsync(CartModel cart)
        {
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task<CartModel> UpdateCartAsync(CartModel cart)
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task<CartItemModel> AddCartItemAsync(CartItemModel cartItem)
        {
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();
            return cartItem;
        }

        public async Task<CartItemModel> UpdateCartItemAsync(CartItemModel cartItem)
        {
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();
            return cartItem;
        }

        public async Task<bool> RemoveCartItemAsync(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null)
                return false;

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart == null)
                return false;

            _context.CartItems.RemoveRange(cart.CartItems);

            // Reset cart totals
            cart.TotalItems = 0;
            cart.Subtotal = 0;
            cart.DiscountAmount = 0;
            cart.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CartItemModel> GetCartItemByIdAsync(int cartItemId)
        {
            return await _context
                .CartItems.Include(ci => ci.Product)
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
        }

        public async Task<CartItemModel> GetCartItemByProductIdAsync(int cartId, int productId)
        {
            return await _context
                .CartItems.Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);
        }

        public async Task<IEnumerable<CartItemModel>> GetCartItemsByCartIdAsync(int cartId)
        {
            return await _context
                .CartItems.Include(ci => ci.Product)
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();
        }

        public async Task<bool> CartExistsAsync(int userId)
        {
            return await _context.Carts.AnyAsync(c => c.UserId == userId);
        }

        public async Task<bool> CartItemExistsAsync(int cartItemId)
        {
            return await _context.CartItems.AnyAsync(ci => ci.Id == cartItemId);
        }
    }
}
