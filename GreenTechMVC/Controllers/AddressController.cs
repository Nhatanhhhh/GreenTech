using Microsoft.AspNetCore.Mvc;

namespace GreenTechMVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AddressController> _logger;

        private const string ApiBaseUrl = "http://provinces.open-api.vn/api/";

        public AddressController(
            IHttpClientFactory httpClientFactory,
            ILogger<AddressController> logger
        )
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Proxy endpoint để lấy danh sách tỉnh/thành phố
        /// </summary>
        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                var response = await client.GetAsync($"{ApiBaseUrl}p/");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Failed to fetch provinces. Status: {StatusCode}",
                        response.StatusCode
                    );
                    return StatusCode(
                        (int)response.StatusCode,
                        new { error = "Không thể tải danh sách tỉnh/thành phố" }
                    );
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching provinces from external API");
                return StatusCode(
                    500,
                    new { error = "Đã có lỗi xảy ra khi tải danh sách tỉnh/thành phố" }
                );
            }
        }

        /// <summary>
        /// Proxy endpoint để lấy thông tin tỉnh và districts
        /// </summary>
        [HttpGet("provinces/{code}")]
        public async Task<IActionResult> GetProvinceWithDistricts(
            string code,
            [FromQuery] int depth = 2
        )
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                var response = await client.GetAsync($"{ApiBaseUrl}p/{code}?depth={depth}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Failed to fetch province with districts. Code: {Code}, Status: {StatusCode}",
                        code,
                        response.StatusCode
                    );
                    return StatusCode(
                        (int)response.StatusCode,
                        new { error = "Không thể tải thông tin quận/huyện" }
                    );
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching province with districts. Code: {Code}", code);
                return StatusCode(
                    500,
                    new { error = "Đã có lỗi xảy ra khi tải thông tin quận/huyện" }
                );
            }
        }

        /// <summary>
        /// Proxy endpoint để lấy thông tin quận/huyện và wards
        /// </summary>
        [HttpGet("districts/{code}")]
        public async Task<IActionResult> GetDistrictWithWards(
            string code,
            [FromQuery] int depth = 2
        )
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                var response = await client.GetAsync($"{ApiBaseUrl}d/{code}?depth={depth}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Failed to fetch district with wards. Code: {Code}, Status: {StatusCode}",
                        code,
                        response.StatusCode
                    );
                    return StatusCode(
                        (int)response.StatusCode,
                        new { error = "Không thể tải thông tin phường/xã" }
                    );
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching district with wards. Code: {Code}", code);
                return StatusCode(
                    500,
                    new { error = "Đã có lỗi xảy ra khi tải thông tin phường/xã" }
                );
            }
        }
    }
}
