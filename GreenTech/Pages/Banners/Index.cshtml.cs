using BLL.Service.Interface;
using DAL.DTOs.Banner;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Banners
{
    [AdminAuthorize]
    public class IndexModel : PageModel
    {
        private readonly IBannerService _bannerService;

        public IndexModel(IBannerService bannerService)
        {
            _bannerService = bannerService;
        }

        public IEnumerable<BannerDTO> Banners { get; set; } = new List<BannerDTO>();
        public BannerQueryParams QueryParams { get; set; } = new BannerQueryParams();

        public async Task OnGetAsync(BannerQueryParams queryParams)
        {
            QueryParams = queryParams;
            if (queryParams.Page <= 0) QueryParams.Page = 1;
            if (queryParams.PageSize <= 0) QueryParams.PageSize = 10;
            
            Banners = await _bannerService.GetWithQueryAsync(QueryParams);
        }
    }
}

