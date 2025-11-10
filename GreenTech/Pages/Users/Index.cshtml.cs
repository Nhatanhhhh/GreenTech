using BLL.Service.User.Interface;
using DAL.DTOs.User;
using GreenTech.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Pages.Users
{
    [AdminAuthorize]
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;

        public IndexModel(IUserService userService)
        {
            _userService = userService;
        }

        public IEnumerable<UserResponseDTO> Users { get; set; } = new List<UserResponseDTO>();
        public UserQueryParamsDTO QueryParams { get; set; } = new UserQueryParamsDTO();
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / QueryParams.PageSize);

        public async Task OnGetAsync(UserQueryParamsDTO queryParams)
        {
            QueryParams = queryParams;
            if (queryParams.PageNumber <= 0)
            {
                QueryParams.PageNumber = 1;
            }

            if (queryParams.PageSize <= 0)
            {
                QueryParams.PageSize = 10;
            }

            var result = await _userService.GetAllUsersAsync(QueryParams);
            Users = result.Users;
            TotalCount = result.TotalCount;
        }
    }
}
