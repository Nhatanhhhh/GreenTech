using BLL.Service.Product.Interface;
using DAL.Context;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GreenTech.Pages.Reports
{
    [AdminAuthorize]
    public class ProductsModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly AppDbContext _context;

        public ProductsModel(IProductService productService, AppDbContext context)
        {
            _productService = productService;
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public List<ProductReportData> TopSellingProducts { get; set; } = new();
        public List<ProductReportData> LowStockProductsList { get; set; } = new();

        public async Task OnGetAsync()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();

            TotalProducts = products.Count;
            ActiveProducts = products.Count(p => p.IsActive);
            LowStockProducts = products.Count(p => p.Quantity > 0 && p.Quantity <= 10);
            OutOfStockProducts = products.Count(p => p.Quantity == 0);

            var start = StartDate ?? DateTime.Now.AddMonths(-1);
            var end = EndDate ?? DateTime.Now;

            TopSellingProducts = await _context
                .OrderItems.Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .Where(oi =>
                    oi.Order.CreatedAt >= start
                    && oi.Order.CreatedAt <= end
                    && oi.Order.Status == DAL.Models.Enum.OrderStatus.DELIVERED
                )
                .GroupBy(oi => new
                {
                    oi.ProductId,
                    oi.Product.Name,
                    oi.Product.Sku,
                })
                .Select(g => new ProductReportData
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    Sku = g.Key.Sku,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.UnitSellPrice * oi.Quantity),
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(10)
                .ToListAsync();

            LowStockProductsList = products
                .Where(p => p.Quantity > 0 && p.Quantity <= 10)
                .OrderBy(p => p.Quantity)
                .Take(10)
                .Select(p => new ProductReportData
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    Sku = p.Sku,
                    QuantitySold = p.Quantity,
                    Revenue = 0,
                })
                .ToList();
        }

        public async Task<IActionResult> OnPostExportAsync()
        {
            var products = await _context
                .Products.Include(p => p.Category)
                .Include(p => p.Supplier)
                .OrderBy(p => p.Name)
                .ToListAsync();

            var csv = "SKU, Tên sản phẩm, Danh mục, Nhà cung cấp, Giá bán, Số lượng, Trạng thái\n";
            foreach (var product in products)
            {
                csv +=
                    $"{product.Sku},{product.Name},{product.Category?.Name ?? ""},{product.Supplier?.Name ?? ""},{product.SellPrice},{product.Quantity},{(product.IsActive ? "Hoạt động" : "Ngừng bán")}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"products_report_{DateTime.Now:yyyyMMdd}.csv");
        }
    }

    public class ProductReportData
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string Sku { get; set; } = "";
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }
}
