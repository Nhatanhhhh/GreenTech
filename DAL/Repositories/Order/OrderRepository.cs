using DAL.Context;
using DAL.Models.Enum;
using DAL.Repositories.Order.Interface;
using Microsoft.EntityFrameworkCore;
using OrderItemModel = DAL.Models.OrderItem;
using OrderModel = DAL.Models.Order;

namespace DAL.Repositories.Order
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrderModel> GetOrderByIdAsync(int orderId)
        {
            return await _context
                .Orders.Include(o => o.User)
                .Include(o => o.Coupon)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.WalletTransactions)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<OrderModel> GetOrderByOrderNumberAsync(string orderNumber)
        {
            return await _context
                .Orders.Include(o => o.User)
                .Include(o => o.Coupon)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<OrderModel> CreateOrderAsync(OrderModel order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<OrderModel> UpdateOrderAsync(OrderModel order)
        {
            order.UpdatedAt = DateTime.Now;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<OrderModel>> GetOrdersByUserIdAsync(int userId)
        {
            return await _context
                .Orders.Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Coupon)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderModel>> GetOrdersByUserIdAsync(
            int userId,
            OrderStatus? status
        )
        {
            var query = _context
                .Orders.Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Coupon)
                .Where(o => o.UserId == userId);

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<OrderModel>> GetAllOrdersAsync()
        {
            return await _context
                .Orders.Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Coupon)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderModel>> GetAllOrdersAsync(OrderStatus? status)
        {
            IQueryable<OrderModel> query = _context
                .Orders.Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Coupon);

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        }

        public async Task<IEnumerable<OrderModel>> GetAllOrdersAsync(
            DateTime? startDate,
            DateTime? endDate
        )
        {
            IQueryable<OrderModel> query = _context
                .Orders.Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Coupon);

            if (startDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= endDate.Value);
            }

            return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        }

        public async Task<OrderItemModel> CreateOrderItemAsync(OrderItemModel orderItem)
        {
            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();
            return orderItem;
        }

        public async Task<IEnumerable<OrderItemModel>> GetOrderItemsByOrderIdAsync(int orderId)
        {
            return await _context
                .OrderItems.Include(oi => oi.Product)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<OrderItemModel> GetOrderItemByIdAsync(int orderItemId)
        {
            return await _context
                .OrderItems.Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .FirstOrDefaultAsync(oi => oi.Id == orderItemId);
        }
    }
}
