using DAL.DTOs.Order;
using DAL.Models.Enum;

namespace BLL.Service.Order.Interface
{
    public interface IOrderService
    {
        // Customer operations
        Task<OrderResponseDTO> CreateOrderAsync(CreateOrderDTO createOrderDTO);
        Task<IEnumerable<OrderResponseDTO>> GetMyOrdersAsync(int userId);
        Task<IEnumerable<OrderResponseDTO>> GetMyOrdersAsync(int userId, OrderStatus? status);
        Task<OrderResponseDTO> GetOrderDetailsAsync(int orderId, int userId);
        Task<bool> CancelOrderAsync(CancelOrderDTO cancelOrderDTO);

        // Admin operations
        Task<IEnumerable<OrderResponseDTO>> GetAllOrdersAsync();
        Task<IEnumerable<OrderResponseDTO>> GetAllOrdersAsync(OrderStatus? status);
        Task<IEnumerable<OrderResponseDTO>> GetAllOrdersAsync(
            DateTime? startDate,
            DateTime? endDate
        );
        Task<OrderResponseDTO> GetOrderDetailsAsync(int orderId);
        Task<OrderResponseDTO> UpdateOrderStatusAsync(UpdateOrderStatusDTO updateOrderStatusDTO);
        Task<bool> RejectCancellationRequestAsync(int orderId, string? reason = null);

        // Validation
        Task<bool> ValidateOrderOwnershipAsync(int orderId, int userId);
    }
}
