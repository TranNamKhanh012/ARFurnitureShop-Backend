using Microsoft.AspNetCore.Mvc;
using ARFurniture.AdminWeb.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace ARFurniture.AdminWeb.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly HttpClient _httpClient;

        public OrderController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Hiển thị danh sách đơn hàng
        public async Task<IActionResult> Index()
        {
            var model = new List<OrderViewModel>();
            var response = await _httpClient.GetAsync("Orders/admin-list");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<List<OrderViewModel>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return View(model);
        }

        // Cập nhật trạng thái
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var payload = new { Status = status };
            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            await _httpClient.PutAsync($"Orders/admin-update-status/{id}", content);

            return RedirectToAction("Index");
        }
        // Hiển thị Chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            var response = await _httpClient.GetAsync($"Orders/admin-get/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var model = JsonSerializer.Deserialize<OrderDetailViewModel>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(model);
            }

            TempData["Error"] = "Không thể lấy thông tin chi tiết đơn hàng.";
            return RedirectToAction("Index");
        }
    }
}