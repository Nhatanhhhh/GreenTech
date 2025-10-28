using BLL.Service.Interface;
using DAL.DTOs.CouponTemplate;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.CouponTemplates
{
    [AdminAuthorize]
    public class IndexModel : PageModel
    {
        private readonly ICouponTemplateService _couponTemplateService;

        public IndexModel(ICouponTemplateService couponTemplateService)
        {
            _couponTemplateService = couponTemplateService;
        }

        public IEnumerable<CouponTemplateDTO> Templates { get; set; } = new List<CouponTemplateDTO>();
        public CouponTemplateQueryParams QueryParams { get; set; } = new CouponTemplateQueryParams();

        public async Task OnGetAsync(CouponTemplateQueryParams queryParams)
        {
            QueryParams = queryParams;
            if (queryParams.Page <= 0)
            {
                QueryParams.Page = 1;
            }

            if (queryParams.PageSize <= 0)
            {
                QueryParams.PageSize = 10;
            }

            Templates = await _couponTemplateService.GetWithQueryAsync(QueryParams);
        }
    }
}
