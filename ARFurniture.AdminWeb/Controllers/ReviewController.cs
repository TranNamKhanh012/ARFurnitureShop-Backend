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
    public class ReviewController : Controller
    {
        private readonly HttpClient _httpClient;

        public ReviewController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Hiển thị danh sách phản hồi
        public async Task<IActionResult> Index()
        {
            var model = new List<ReviewViewModel>();
            var response = await _httpClient.GetAsync("Reviews/admin-list");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<List<ReviewViewModel>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return View(model);
        }

        // Xóa phản hồi
        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync($"Reviews/admin-delete/{id}");
            return RedirectToAction("Index");
        }
    }
}