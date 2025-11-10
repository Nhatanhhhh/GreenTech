using BLL.Service.User.Interface;
using DAL.Context;
using DAL.Models.Enum;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RoleName = DAL.Models.Enum.RoleName;

namespace GreenTech.Pages.Reports
{
    [AdminAuthorize]
    public class CustomersModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly AppDbContext _context;

        public CustomersModel(IUserService userService, AppDbContext context)
        {
            _userService = userService;
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int BannedCustomers { get; set; }
        public List<CustomerReportData> TopCustomers { get; set; } = new();

        public async Task OnGetAsync()
        {
            var customers = await _context
                .Users.Where(u =>
                    !u.UserRoles.Any(ur =>
                        ur.Role.RoleName == RoleName.ROLE_ADMIN || ur.Role.RoleName == RoleName.ROLE_STAFF
                    )
                )
                .ToListAsync();

            TotalCustomers = customers.Count;
            ActiveCustomers = customers.Count(c => c.Status == UserStatus.ACTIVE);
            BannedCustomers = customers.Count(c => c.Status == UserStatus.BLOCKED);

            var start = StartDate ?? DateTime.Now.AddMonths(-1);
            var end = EndDate ?? DateTime.Now;

            NewCustomers = customers.Count(c => c.CreatedAt >= start && c.CreatedAt <= end);

            TopCustomers = await _context
                .Orders.Include(o => o.User)
                .Where(o =>
                    o.CreatedAt >= start && o.CreatedAt <= end && o.Status == OrderStatus.DELIVERED
                )
                .GroupBy(o => new
                {
                    o.UserId,
                    o.User.FullName,
                    o.User.Email,
                })
                .Select(g => new CustomerReportData
                {
                    UserId = g.Key.UserId,
                    FullName = g.Key.FullName,
                    Email = g.Key.Email,
                    TotalOrders = g.Count(),
                    TotalSpent = g.Sum(o => o.Total),
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(10)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostExportAsync()
        {
            var customers = await _context
                .Users.Where(u =>
                    !u.UserRoles.Any(ur =>
                        ur.Role.RoleName == RoleName.ROLE_ADMIN || ur.Role.RoleName == RoleName.ROLE_STAFF
                    )
                )
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var csv =
                "Họ tên, Email, Số điện thoại, Địa chỉ, Điểm tích lũy, Số dư ví, Trạng thái, Ngày đăng ký\n";
            foreach (var customer in customers)
            {
                var address =
                    $"{customer.SpecificAddress}, {customer.Ward}, {customer.District}, {customer.Province}";
                csv +=
                    $"{customer.FullName},{customer.Email},{customer.Phone},{address},{customer.Points},{customer.WalletBalance},{(customer.Status == UserStatus.ACTIVE ? "Hoạt động" : "Bị chặn")},{customer.CreatedAt:yyyy-MM-dd}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"customers_report_{DateTime.Now:yyyyMMdd}.csv");
        }
    }

    public class CustomerReportData
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
