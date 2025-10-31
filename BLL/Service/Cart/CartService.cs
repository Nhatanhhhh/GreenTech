using BLL.Service.Cart.Interface;
using DAL.Context;
using DAL.DTOs.Cart;
using DAL.Repositories.Cart.Interface;
using DAL.Repositories.Product.Interface;
using DAL.Utils.AutoMapper;
using Microsoft.EntityFrameworkCore;
using CartItemModel = DAL.Models.CartItem;
using CartModel = DAL.Models.Cart;

namespace BLL.Service.Cart
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly AppDbContext _context;

        public CartService(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            AppDbContext context
        )
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _context = context;
        }

        public async Task<CartResponseDTO> GetCartByUserIdAsync(int userId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                // Create new cart if doesn't exist
                cart = new CartModel
                {
                    UserId = userId,
                    TotalItems = 0,
                    Subtotal = 0,
                    DiscountAmount = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };
                cart = await _cartRepository.CreateCartAsync(cart);
            }

            return AutoMapper.ToCartResponseDTO(cart);
        }

        public async Task<CartResponseDTO> AddToCartAsync(AddToCartDTO addToCartDTO)
        {
            // Validate product exists and is active
            var product = await _productRepository.GetByIdAsync(addToCartDTO.ProductId);
            if (product == null || !product.IsActive)
            {
                throw new ArgumentException("Sản phẩm không tồn tại hoặc đã ngừng bán");
            }

            // Check product quantity
            if (product.Quantity < addToCartDTO.Quantity)
            {
                throw new ArgumentException(
                    $"Số lượng sản phẩm không đủ. Chỉ còn {product.Quantity} sản phẩm"
                );
            }

            // Get or create cart
            var cart = await _cartRepository.GetCartByUserIdAsync(addToCartDTO.UserId);
            if (cart == null)
            {
                cart = new CartModel
                {
                    UserId = addToCartDTO.UserId,
                    TotalItems = 0,
                    Subtotal = 0,
                    DiscountAmount = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };
                cart = await _cartRepository.CreateCartAsync(cart);
            }

            // Check if product already exists in cart
            var existingCartItem = await _cartRepository.GetCartItemByProductIdAsync(
                cart.Id,
                addToCartDTO.ProductId
            );

            if (existingCartItem != null)
            {
                // Update existing cart item
                existingCartItem.Quantity += addToCartDTO.Quantity;
                existingCartItem.Subtotal = existingCartItem.Quantity * existingCartItem.UnitPrice;
                existingCartItem.UpdatedAt = DateTime.Now;

                await _cartRepository.UpdateCartItemAsync(existingCartItem);
            }
            else
            {
                // Create new cart item
                var cartItem = new CartItemModel
                {
                    CartId = cart.Id,
                    ProductId = addToCartDTO.ProductId,
                    ProductSku = product.Sku,
                    ProductName = product.Name,
                    ProductImage = product.Image,
                    Quantity = addToCartDTO.Quantity,
                    UnitPrice = product.SellPrice,
                    Subtotal = addToCartDTO.Quantity * product.SellPrice,
                    IsAvailable = product.IsActive && product.Quantity > 0,
                    AddedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };

                await _cartRepository.AddCartItemAsync(cartItem);
            }

            // Update cart totals
            await UpdateCartTotalsAsync(cart.Id);

            return await GetCartByUserIdAsync(addToCartDTO.UserId);
        }

        public async Task<CartResponseDTO> UpdateCartItemAsync(UpdateCartItemDTO updateCartItemDTO)
        {
            var cartItem = await _cartRepository.GetCartItemByIdAsync(updateCartItemDTO.CartItemId);
            if (cartItem == null)
            {
                throw new ArgumentException("Sản phẩm không tồn tại trong giỏ hàng");
            }

            // Validate product quantity
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
            if (product == null || !product.IsActive)
            {
                throw new ArgumentException("Sản phẩm không tồn tại hoặc đã ngừng bán");
            }

            if (product.Quantity < updateCartItemDTO.Quantity)
            {
                throw new ArgumentException(
                    $"Số lượng sản phẩm không đủ. Chỉ còn {product.Quantity} sản phẩm"
                );
            }

            // Update cart item
            cartItem.Quantity = updateCartItemDTO.Quantity;
            cartItem.Subtotal = cartItem.Quantity * cartItem.UnitPrice;
            cartItem.IsAvailable = product.IsActive && product.Quantity > 0;
            cartItem.UpdatedAt = DateTime.Now;

            await _cartRepository.UpdateCartItemAsync(cartItem);

            // Update cart totals
            await UpdateCartTotalsAsync(cartItem.CartId);

            return await GetCartByUserIdAsync(cartItem.Cart.UserId);
        }

        public async Task<bool> RemoveCartItemAsync(int cartItemId, int userId)
        {
            var cartItem = await _cartRepository.GetCartItemByIdAsync(cartItemId);
            if (cartItem == null || cartItem.Cart.UserId != userId)
            {
                return false;
            }

            var cartId = cartItem.CartId;
            var result = await _cartRepository.RemoveCartItemAsync(cartItemId);

            if (result)
            {
                await UpdateCartTotalsAsync(cartId);
            }

            return result;
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            return await _cartRepository.ClearCartAsync(userId);
        }

        public async Task<decimal> CalculateCartTotalAsync(int cartId)
        {
            var cartItems = await _cartRepository.GetCartItemsByCartIdAsync(cartId);
            return cartItems.Sum(ci => ci.Subtotal);
        }

        public async Task<bool> ValidateCartItemAsync(int cartItemId, int userId)
        {
            var cartItem = await _cartRepository.GetCartItemByIdAsync(cartItemId);
            return cartItem != null && cartItem.Cart.UserId == userId;
        }

        private async Task UpdateCartTotalsAsync(int cartId)
        {
            var cart = await _context
                .Carts.Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart != null)
            {
                cart.TotalItems = cart.CartItems.Sum(ci => ci.Quantity);
                cart.Subtotal = cart.CartItems.Sum(ci => ci.Subtotal);
                cart.UpdatedAt = DateTime.Now;

                await _cartRepository.UpdateCartAsync(cart);
            }
        }
    }
}
