using DAL.Models.Enum;
using OrderItemModel = DAL.Models.OrderItem;
using OrderModel = DAL.Models.Order;

namespace DAL.Repositories.Order.Interface
{
    public interface IOrderRepository
    {
        // Order CRUD
        Task<OrderModel> GetOrderByIdAsync(int orderId);
        Task<OrderModel> GetOrderByOrderNumberAsync(string orderNumber);
        Task<OrderModel> CreateOrderAsync(OrderModel order);
        Task<OrderModel> UpdateOrderAsync(OrderModel order);
        Task<bool> DeleteOrderAsync(int orderId);

        // Get orders by user
        Task<IEnumerable<OrderModel>> GetOrdersByUserIdAsync(int userId);
        Task<IEnumerable<OrderModel>> GetOrdersByUserIdAsync(int userId, OrderStatus? status);

        // Get all orders (Admin)
        Task<IEnumerable<OrderModel>> GetAllOrdersAsync();
        Task<IEnumerable<OrderModel>> GetAllOrdersAsync(OrderStatus? status);
        Task<IEnumerable<OrderModel>> GetAllOrdersAsync(DateTime? startDate, DateTime? endDate);

        // Order Items
        Task<OrderItemModel> CreateOrderItemAsync(OrderItemModel orderItem);
        Task<IEnumerable<OrderItemModel>> GetOrderItemsByOrderIdAsync(int orderId);
        Task<OrderItemModel> GetOrderItemByIdAsync(int orderItemId);
    }
}
