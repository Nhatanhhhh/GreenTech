using BLL.Service.Interface;
using DAL.DTOs.Category;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Categories
{
    [AdminAuthorize]
    public class IndexModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public IndexModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public IEnumerable<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();
        public CategoryQueryParams QueryParams { get; set; } = new CategoryQueryParams();

        public async Task OnGetAsync(CategoryQueryParams queryParams)
        {
            QueryParams = queryParams;
            Categories = await _categoryService.GetWithQueryAsync(QueryParams);
        }
    }
}

