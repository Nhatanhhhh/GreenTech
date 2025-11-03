using BLL.Service.Order.Interface;
using DAL.DTOs.Order;
using DAL.Models.Enum;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Orders
{
    [AdminAuthorize]
    public class IndexModel : PageModel
    {
        private readonly IOrderService _orderService;

        public IndexModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IEnumerable<OrderResponseDTO> Orders { get; set; } = new List<OrderResponseDTO>();

        [BindProperty(SupportsGet = true)]
        public OrderStatus? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public async Task OnGetAsync()
        {
            if (StartDate.HasValue || EndDate.HasValue)
            {
                Orders = await _orderService.GetAllOrdersAsync(StartDate, EndDate);
            }
            else if (Status.HasValue)
            {
                Orders = await _orderService.GetAllOrdersAsync(Status);
            }
            else
            {
                Orders = await _orderService.GetAllOrdersAsync();
            }
        }
    }
}
