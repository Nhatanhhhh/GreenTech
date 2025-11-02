using BLL.Service.Cart.Interface;
using BLL.Service.Order.Interface;
using BLL.Service.Point.Interface;
using DAL.Context;
using DAL.DTOs.Cart;
using DAL.DTOs.Order;
using DAL.Models;
using DAL.Models.Enum;
using DAL.Repositories.Cart.Interface;
using DAL.Repositories.Order.Interface;
using DAL.Repositories.Product.Interface;
using DAL.Repositories.User.Interface;
using DAL.Repositories.Wallet.Interface;
using DAL.Utils.AutoMapper;
using Microsoft.EntityFrameworkCore;
using OrderItemModel = DAL.Models.OrderItem;
using OrderModel = DAL.Models.Order;
using UserModel = DAL.Models.User;

namespace BLL.Service.Order
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ICartService _cartService;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPointsService _pointsService;
        private readonly IWalletRepository _walletRepository;
        private readonly AppDbContext _context;

        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            ICartService cartService,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IPointsService pointsService,
            IWalletRepository walletRepository,
            AppDbContext context
        )
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _cartService = cartService;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _pointsService = pointsService;
            _walletRepository = walletRepository;
            _context = context;
        }

        public async Task<OrderResponseDTO> CreateOrderAsync(CreateOrderDTO createOrderDTO)
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(createOrderDTO.UserId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại");
            }

            // Get cart
            var cart = await _cartRepository.GetCartByUserIdAsync(createOrderDTO.UserId);
            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                throw new InvalidOperationException("Giỏ hàng trống, không thể tạo đơn hàng");
            }

            // Validate all cart items are still available
            foreach (var cartItem in cart.CartItems)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product == null || !product.IsActive)
                {
                    throw new InvalidOperationException(
                        $"Sản phẩm {cartItem.ProductName} không còn khả dụng"
                    );
                }
                if (product.Quantity < cartItem.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Sản phẩm {cartItem.ProductName} không đủ số lượng"
                    );
                }
            }

            // Calculate totals
            var subtotal = cart.Subtotal;
            var discountAmount = cart.DiscountAmount;
            var total = subtotal - discountAmount + createOrderDTO.ShippingFee;

            // Validate wallet balance
            if (createOrderDTO.WalletAmountUsed > user.WalletBalance)
            {
                throw new InvalidOperationException("Số dư ví không đủ");
            }

            if (createOrderDTO.WalletAmountUsed > total)
            {
                throw new InvalidOperationException(
                    "Số tiền dùng từ ví không được vượt quá tổng tiền"
                );
            }

            // Generate order number
            var orderNumber = GenerateOrderNumber(createOrderDTO.UserId);

            // Create order
            var order = new OrderModel
            {
                OrderNumber = orderNumber,
                UserId = createOrderDTO.UserId,
                CouponId = cart.CouponId,
                Status = OrderStatus.PENDING,
                PaymentStatus = PaymentStatus.PENDING,
                Subtotal = subtotal,
                DiscountAmount = discountAmount,
                ShippingFee = createOrderDTO.ShippingFee,
                Total = total,
                WalletAmountUsed = createOrderDTO.WalletAmountUsed,
                GatewayTransactionId = "", // Set to empty string for new orders (database doesn't allow NULL)
                ShippingAddress = createOrderDTO.ShippingAddress,
                CustomerName = createOrderDTO.CustomerName,
                CustomerPhone = createOrderDTO.CustomerPhone,
                Note = string.IsNullOrWhiteSpace(createOrderDTO.Note) ? null : createOrderDTO.Note,
                CancelledReason = "", // Set to empty string for new orders (database doesn't allow NULL)
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            // Save order first to get the OrderId
            order = await _orderRepository.CreateOrderAsync(order);

            // Create order items with the correct OrderId and deduct product quantities
            foreach (var cartItem in cart.CartItems)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);

                // Create order item
                var orderItem = new OrderItemModel
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    ProductSku = cartItem.ProductSku,
                    ProductName = cartItem.ProductName,
                    Quantity = cartItem.Quantity,
                    UnitCostPrice = product.CostPrice,
                    UnitSellPrice = product.SellPrice,
                    Total = cartItem.Subtotal,
                    PointsPerItem = product.PointsEarned * cartItem.Quantity,
                };
                await _orderRepository.CreateOrderItemAsync(orderItem);

                // Deduct product quantity
                product.Quantity -= cartItem.Quantity;
                await _productRepository.UpdateAsync(product);
            }

            // Create wallet hold transaction (PENDING - holds money but doesn't deduct yet)
            if (createOrderDTO.WalletAmountUsed > 0)
            {
                await _walletRepository.CreateHoldTransactionAsync(
                    createOrderDTO.UserId,
                    order.Id,
                    createOrderDTO.WalletAmountUsed,
                    $"Tạm giữ tiền cho đơn hàng #{orderNumber}"
                );
            }

            // Clear cart
            await _cartService.ClearCartAsync(createOrderDTO.UserId);

            // Load order with all relations for response
            order = await _orderRepository.GetOrderByIdAsync(order.Id);
            var orderDTO = AutoMapper.ToOrderResponseDTO(order);

            return orderDTO;
        }

        public async Task<IEnumerable<OrderResponseDTO>> GetMyOrdersAsync(int userId)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            var orderDTOs = new List<OrderResponseDTO>();

            foreach (var order in orders)
            {
                var orderDTO = AutoMapper.ToOrderResponseDTO(order);
                orderDTOs.Add(orderDTO);
            }

            return orderDTOs;
        }

        public async Task<IEnumerable<OrderResponseDTO>> GetMyOrdersAsync(
            int userId,
            OrderStatus? status
        )
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId, status);
            var orderDTOs = new List<OrderResponseDTO>();

            foreach (var order in orders)
            {
                var orderDTO = AutoMapper.ToOrderResponseDTO(order);
                orderDTOs.Add(orderDTO);
            }

            return orderDTOs;
        }

        public async Task<OrderResponseDTO> GetOrderDetailsAsync(int orderId, int userId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null || order.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xem đơn hàng này");
            }

            var orderDTO = AutoMapper.ToOrderResponseDTO(order);
            return orderDTO;
        }

        public async Task<bool> CancelOrderAsync(CancelOrderDTO cancelOrderDTO)
        {
            var order = await _orderRepository.GetOrderByIdAsync(cancelOrderDTO.OrderId);
            if (order == null)
            {
                throw new ArgumentException("Đơn hàng không tồn tại");
            }

            // Validate ownership
            if (cancelOrderDTO.UserId.HasValue && order.UserId != cancelOrderDTO.UserId.Value)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền hủy đơn hàng này");
            }

            // Only allow cancellation request if order is PENDING or CONFIRMED
            if (order.Status != OrderStatus.PENDING && order.Status != OrderStatus.CONFIRMED)
            {
                throw new InvalidOperationException(
                    $"Không thể gửi yêu cầu hủy đơn hàng ở trạng thái {order.Status}"
                );
            }

            // Check if there's already a cancellation request
            if (!string.IsNullOrWhiteSpace(order.CancelledReason))
            {
                throw new InvalidOperationException(
                    "Đơn hàng này đã có yêu cầu hủy đang chờ admin xử lý"
                );
            }

            // Only save cancellation request (do NOT cancel directly)
            // Status remains PENDING or CONFIRMED, only CancelledReason is set
            order.CancelledReason = string.IsNullOrWhiteSpace(cancelOrderDTO.Reason)
                ? "Khách hàng yêu cầu hủy đơn hàng"
                : cancelOrderDTO.Reason;
            // Do NOT set CancelledAt or change Status - wait for admin approval
            order.UpdatedAt = DateTime.Now;

            await _orderRepository.UpdateOrderAsync(order);

            return true;
        }

        public async Task<IEnumerable<OrderResponseDTO>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            return AutoMapper.ToOrderResponseDTOs(orders);
        }

        public async Task<IEnumerable<OrderResponseDTO>> GetAllOrdersAsync(OrderStatus? status)
        {
            var orders = await _orderRepository.GetAllOrdersAsync(status);
            return AutoMapper.ToOrderResponseDTOs(orders);
        }

        public async Task<IEnumerable<OrderResponseDTO>> GetAllOrdersAsync(
            DateTime? startDate,
            DateTime? endDate
        )
        {
            var orders = await _orderRepository.GetAllOrdersAsync(startDate, endDate);
            return AutoMapper.ToOrderResponseDTOs(orders);
        }

        public async Task<OrderResponseDTO> GetOrderDetailsAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new ArgumentException("Đơn hàng không tồn tại");
            }

            var orderDTO = AutoMapper.ToOrderResponseDTO(order);
            return orderDTO;
        }

        public async Task<OrderResponseDTO> UpdateOrderStatusAsync(
            UpdateOrderStatusDTO updateOrderStatusDTO
        )
        {
            var order = await _orderRepository.GetOrderByIdAsync(updateOrderStatusDTO.OrderId);
            if (order == null)
            {
                throw new ArgumentException("Đơn hàng không tồn tại");
            }

            // Update order status
            order.Status = updateOrderStatusDTO.Status;
            order.UpdatedAt = DateTime.Now;

            // Set specific timestamps based on status
            if (updateOrderStatusDTO.Status == OrderStatus.SHIPPED && order.ShippedAt == null)
            {
                order.ShippedAt = DateTime.Now;
            }

            if (updateOrderStatusDTO.Status == OrderStatus.DELIVERED && order.DeliveredAt == null)
            {
                order.DeliveredAt = DateTime.Now;
                // Award points if not already awarded
                if (order.PointsEarned > 0 && order.PointsAwardedAt == null)
                {
                    // Award points through points service
                    // This would need to be implemented in PointsService
                    order.PointsAwardedAt = DateTime.Now;
                }

                // Confirm wallet hold transaction (PENDING → SUCCESS) - actually deduct money
                if (order.WalletAmountUsed > 0)
                {
                    await _walletRepository.ConfirmHoldTransactionAsync(
                        order.Id,
                        TransactionStatus.SUCCESS
                    );
                }
            }

            if (updateOrderStatusDTO.Status == OrderStatus.CANCELLED)
            {
                order.CancelledReason = string.IsNullOrWhiteSpace(updateOrderStatusDTO.Reason)
                    ? ""
                    : updateOrderStatusDTO.Reason;
                order.CancelledAt = DateTime.Now;

                // Restore product quantities when order is cancelled
                foreach (var orderItem in order.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(orderItem.ProductId);
                    if (product != null)
                    {
                        product.Quantity += orderItem.Quantity;
                        await _productRepository.UpdateAsync(product);
                    }
                }

                // Refund wallet money if order was cancelled
                if (order.WalletAmountUsed > 0)
                {
                    // Check if there's a pending hold transaction
                    var holdTransaction = await _walletRepository.GetHoldTransactionByOrderIdAsync(
                        order.Id
                    );
                    if (holdTransaction != null)
                    {
                        // Cancel the hold transaction (set to FAILED, no deduction)
                        // Since it's PENDING, balance was never deducted, so no refund needed
                        await _walletRepository.ConfirmHoldTransactionAsync(
                            order.Id,
                            TransactionStatus.FAILED
                        );
                    }
                    else
                    {
                        // Hold transaction doesn't exist or already processed (was SUCCESS)
                        // This means balance was already deducted when order was DELIVERED
                        // So we need to refund, but only the subtotal portion (not shipping fee)
                        var refundAmount = Math.Min(order.WalletAmountUsed, order.Subtotal);
                        if (refundAmount > 0)
                        {
                            await _walletRepository.CreateRefundTransactionAsync(
                                order.UserId,
                                order.Id,
                                refundAmount,
                                $"Hoàn tiền cho đơn hàng #{order.OrderNumber} đã bị hủy (chỉ hoàn lại tiền sản phẩm, không bao gồm phí vận chuyển)"
                            );
                        }
                    }
                }
            }

            await _orderRepository.UpdateOrderAsync(order);

            var orderDTO = AutoMapper.ToOrderResponseDTO(order);
            return orderDTO;
        }

        public async Task<bool> RejectCancellationRequestAsync(int orderId, string? reason = null)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new ArgumentException("Đơn hàng không tồn tại");
            }

            if (string.IsNullOrWhiteSpace(order.CancelledReason))
            {
                throw new InvalidOperationException("Đơn hàng này chưa có yêu cầu hủy");
            }

            // Clear cancellation request
            order.CancelledReason = null;
            order.UpdatedAt = DateTime.Now;

            await _orderRepository.UpdateOrderAsync(order);
            return true;
        }

        // ProcessShippingAsync and GetShippingTrackingAsync have been removed
        // Shipping tracking functionality has been completely removed from the system

        public async Task<bool> ValidateOrderOwnershipAsync(int orderId, int userId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            return order != null && order.UserId == userId;
        }

        // Helper methods
        private string GenerateOrderNumber(int userId)
        {
            // Format: GT-YYYYMMDD-HHmmss-XXXX (GT = GreenTech, XXXX = last 4 digits of userId + random)
            // Unique constraint in DB will handle any collisions (extremely rare with timestamp + random)
            var prefix = "GT";
            var datePart = DateTime.Now.ToString("yyyyMMdd");
            var timePart = DateTime.Now.ToString("HHmmss");
            var userPart = userId
                .ToString()
                .PadLeft(4, '0')
                .Substring(Math.Max(0, userId.ToString().Length - 4));
            var randomPart = new Random().Next(1000, 9999).ToString();

            // Generate unique order number - DB unique constraint will handle collisions if any
            return $"{prefix}-{datePart}-{timePart}-{userPart}{randomPart}";
        }

        private string GetStatusDescription(string status)
        {
            return status.ToUpper() switch
            {
                "PENDING" => "Đơn hàng đang chờ xử lý",
                "CONFIRMED" => "Đơn hàng đã được xác nhận",
                "PROCESSING" => "Đơn hàng đang được chuẩn bị",
                "SHIPPED" => "Đơn hàng đã được giao cho đơn vị vận chuyển",
                "IN_TRANSIT" => "Đơn hàng đang trên đường vận chuyển",
                "OUT_FOR_DELIVERY" => "Đơn hàng đang được giao",
                "DELIVERED" => "Đơn hàng đã được giao thành công",
                "CANCELLED" => "Đơn hàng đã bị hủy",
                _ => $"Trạng thái: {status}",
            };
        }

        private string GetStatusDescription(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.PENDING => "Đơn hàng đang chờ xử lý",
                OrderStatus.CONFIRMED => "Đơn hàng đã được xác nhận",
                OrderStatus.PROCESSING => "Đơn hàng đang được chuẩn bị",
                OrderStatus.SHIPPED => "Đơn hàng đã được giao cho đơn vị vận chuyển",
                OrderStatus.DELIVERED => "Đơn hàng đã được giao thành công",
                OrderStatus.CANCELLED => "Đơn hàng đã bị hủy",
                _ => $"Trạng thái: {status}",
            };
        }

        private OrderStatus? MapShippingStatusToOrderStatus(string shippingStatus)
        {
            return shippingStatus.ToUpper() switch
            {
                "CONFIRMED" => OrderStatus.CONFIRMED,
                "PROCESSING" => OrderStatus.PROCESSING,
                "SHIPPED" => OrderStatus.SHIPPED,
                "DELIVERED" => OrderStatus.DELIVERED,
                "CANCELLED" => OrderStatus.CANCELLED,
                _ => null, // Don't update if status doesn't map
            };
        }
    }
}
