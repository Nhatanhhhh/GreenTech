using BLL.Service.Order.Interface;
using DAL.Context;
using DAL.Models.Enum;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GreenTech.Pages.Reports
{
    [AdminAuthorize]
    public class SalesModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly AppDbContext _context;

        public SalesModel(IOrderService orderService, AppDbContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public Dictionary<string, decimal> RevenueByStatus { get; set; } = new();
        public List<SalesData> DailySales { get; set; } = new();

        public async Task OnGetAsync()
        {
            var start = StartDate ?? DateTime.Now.AddMonths(-1);
            var end = EndDate ?? DateTime.Now;

            var orders = await _context
                .Orders.Where(o => o.CreatedAt >= start && o.CreatedAt <= end)
                .ToListAsync();

            TotalOrders = orders.Count;
            TotalRevenue = orders.Where(o => o.Status == OrderStatus.DELIVERED).Sum(o => o.Total);
            CompletedOrders = orders.Count(o => o.Status == OrderStatus.DELIVERED);
            CancelledOrders = orders.Count(o => o.Status == OrderStatus.CANCELLED);
            AverageOrderValue = CompletedOrders > 0 ? TotalRevenue / CompletedOrders : 0;

            RevenueByStatus = orders
                .Where(o => o.Status == OrderStatus.DELIVERED)
                .GroupBy(o => o.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Sum(o => o.Total));

            DailySales = orders
                .Where(o => o.Status == OrderStatus.DELIVERED)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new SalesData
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.Total),
                    Orders = g.Count(),
                })
                .OrderBy(s => s.Date)
                .ToList();
        }

        public async Task<IActionResult> OnPostExportAsync()
        {
            var start = StartDate ?? DateTime.Now.AddMonths(-1);
            var end = EndDate ?? DateTime.Now;

            var orders = await _context
                .Orders.Include(o => o.User)
                .Where(o => o.CreatedAt >= start && o.CreatedAt <= end)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var csv =
                "Mã đơn hàng, Khách hàng, Ngày đặt, Tổng tiền, Trạng thái, Phương thức thanh toán\n";
            foreach (var order in orders)
            {
                csv +=
                    $"{order.OrderNumber},{order.CustomerName},{order.CreatedAt:yyyy-MM-dd},{order.Total},{order.Status},{order.PaymentGateway}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"sales_report_{start:yyyyMMdd}_{end:yyyyMMdd}.csv");
        }
    }

    public class SalesData
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }
}
