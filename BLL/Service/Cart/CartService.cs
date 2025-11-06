using BLL.Service.Cart.Interface;
using DAL.DTOs.Cart;
using DAL.Models.Enum;
using DAL.Repositories.Cart.Interface;
using DAL.Repositories.Coupon.Interface;
using DAL.Repositories.Product.Interface;
using DAL.Utils.AutoMapper;
using CartItemModel = DAL.Models.CartItem;
using CartModel = DAL.Models.Cart;

namespace BLL.Service.Cart
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICouponRepository _couponRepository;

        public CartService(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            ICouponRepository couponRepository
        )
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _couponRepository = couponRepository;
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

        public async Task<CartResponseDTO> ApplyCouponAsync(int userId, int couponId)
        {
            // Get cart
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                throw new ArgumentException("Giỏ hàng không tồn tại");
            }

            // Get coupon
            var coupon = await _couponRepository.GetByIdAndUserIdAsync(couponId, userId);
            if (coupon == null)
            {
                throw new ArgumentException("Coupon không tồn tại hoặc không thuộc về bạn");
            }

            // Validate coupon
            var now = DateTime.Now;
            if (now < coupon.StartDate || now > coupon.EndDate)
            {
                throw new ArgumentException("Coupon đã hết hạn hoặc chưa đến thời gian sử dụng");
            }

            if (coupon.UsedCount >= coupon.UsageLimit)
            {
                throw new ArgumentException("Coupon đã hết lượt sử dụng");
            }

            // Recalculate subtotal
            await UpdateCartTotalsAsync(cart.Id);
            cart = await _cartRepository.GetCartWithCouponAsync(cart.Id);
            if (cart == null)
            {
                throw new ArgumentException("Giỏ hàng không tồn tại");
            }

            // Validate min order amount
            if (cart.Subtotal < coupon.MinOrderAmount)
            {
                throw new ArgumentException(
                    $"Đơn hàng phải có giá trị tối thiểu {coupon.MinOrderAmount.ToString("N0")} ₫ để sử dụng coupon này"
                );
            }

            // Calculate discount based on discount type
            // PERCENT: Tính theo phần trăm của subtotal
            // FIXED_AMOUNT: Trừ trực tiếp số tiền cố định
            decimal discountAmount = 0;
            if (coupon.DiscountType == DiscountType.PERCENT)
            {
                // Giảm giá phần trăm: subtotal * (discountValue / 100)
                discountAmount = cart.Subtotal * (coupon.DiscountValue / 100m);
                // Đảm bảo không giảm quá subtotal
                if (discountAmount > cart.Subtotal)
                {
                    discountAmount = cart.Subtotal;
                }
            }
            else if (coupon.DiscountType == DiscountType.FIXED_AMOUNT)
            {
                // Giảm giá tiền mặt: trừ trực tiếp số tiền cố định
                discountAmount = coupon.DiscountValue;
                // Đảm bảo không giảm quá subtotal
                if (discountAmount > cart.Subtotal)
                {
                    discountAmount = cart.Subtotal;
                }
            }

            // Apply coupon to cart
            cart.CouponId = couponId;
            cart.DiscountAmount = discountAmount;
            cart.UpdatedAt = DateTime.Now;

            await _cartRepository.UpdateCartAsync(cart);

            return await GetCartByUserIdAsync(userId);
        }

        public async Task<CartResponseDTO> RemoveCouponAsync(int userId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                throw new ArgumentException("Giỏ hàng không tồn tại");
            }

            cart.CouponId = null;
            cart.DiscountAmount = 0;
            cart.UpdatedAt = DateTime.Now;

            await _cartRepository.UpdateCartAsync(cart);

            return await GetCartByUserIdAsync(userId);
        }

        private async Task UpdateCartTotalsAsync(int cartId)
        {
            var cart = await _cartRepository.GetCartWithCouponAsync(cartId);

            if (cart != null)
            {
                cart.TotalItems = cart.CartItems.Sum(ci => ci.Quantity);
                cart.Subtotal = cart.CartItems.Sum(ci => ci.Subtotal);

                // Recalculate discount if coupon exists
                // Tính lại discount mỗi khi subtotal thay đổi (cập nhật số lượng, thêm/xóa sản phẩm)
                if (cart.CouponId.HasValue && cart.Coupon != null)
                {
                    var coupon = cart.Coupon;
                    decimal discountAmount = 0;
                    if (coupon.DiscountType == DiscountType.PERCENT)
                    {
                        // Giảm giá phần trăm: tính theo % của subtotal
                        discountAmount = cart.Subtotal * (coupon.DiscountValue / 100m);
                        if (discountAmount > cart.Subtotal)
                        {
                            discountAmount = cart.Subtotal;
                        }
                    }
                    else if (coupon.DiscountType == DiscountType.FIXED_AMOUNT)
                    {
                        // Giảm giá tiền mặt: trừ trực tiếp số tiền cố định
                        discountAmount = coupon.DiscountValue;
                        if (discountAmount > cart.Subtotal)
                        {
                            discountAmount = cart.Subtotal;
                        }
                    }
                    cart.DiscountAmount = discountAmount;
                }
                else
                {
                    cart.DiscountAmount = 0;
                }

                cart.UpdatedAt = DateTime.Now;

                await _cartRepository.UpdateCartAsync(cart);
            }
        }
    }
}
