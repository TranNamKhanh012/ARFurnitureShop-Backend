using ARFurniture.AdminWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ARFurniture.AdminWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        // Tiêm IHttpClientFactory vào để tạo Client
        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient"); // Sử dụng cấu hình đã tạo ở Program.cs
        }

        public async Task<IActionResult> Index()
        {
            // Tạo một model trống với dữ liệu mặc định (để nếu API lỗi thì Web vẫn chạy)
            var model = new DashboardViewModel
            {
                TotalProducts = 0,
                TotalOrders = 0,
                TotalUsers = 0,
                RevenueToday = 0
            };

            try
            {
                // Dùng HttpClient gọi sang đường dẫn API thật
                var response = await _httpClient.GetAsync("api/Dashboard/summary");

                if (response.IsSuccessStatusCode)
                {
                    // Ép kiểu JSON trả về từ API thành Model của project WebAdmin
                    // (Lưu ý: Tên các thuộc tính trong JSON và Model phải trùng nhau)
                    model = await response.Content.ReadFromJsonAsync<DashboardViewModel>();
                }
                else
                {
                    // Ghi log lỗi nếu cần
                    ViewBag.Error = "Không thể lấy dữ liệu từ API. Mã lỗi: " + response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                // Nếu API chưa bật hoặc lỗi kết nối, tạm thời vẫn trả về view với dữ liệu 0
                ViewBag.Error = "Lỗi kết nối Server API: " + ex.Message;
            }

            return View(model);
        }
    }
}