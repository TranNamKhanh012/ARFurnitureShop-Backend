using Microsoft.AspNetCore.Mvc;
using ARFurniture.AdminWeb.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ARFurniture.AdminWeb.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomeController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();

            try
            {
                // Nhớ đổi PORT 5186 thành cổng API Localhost của bạn nhé!
                var response = await _httpClient.GetAsync("http://localhost:5103/api/Dashboard/summary");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    model = JsonSerializer.Deserialize<DashboardViewModel>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            catch
            {
                // Nếu API tắt, nó sẽ trả về 0 để không bị lỗi trang
            }

            return View(model);
        }

        // ... các hàm khác giữ nguyên ...
    }
}