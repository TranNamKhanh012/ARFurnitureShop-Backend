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
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;

        public UserController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // 1. Hiển thị danh sách User
        public async Task<IActionResult> Index()
        {
            var model = new List<UserViewModel>();
            var response = await _httpClient.GetAsync("Auth/admin-list");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<List<UserViewModel>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return View(model);
        }

        // 2. Đổi quyền (Role)
        [HttpPost]
        public async Task<IActionResult> ChangeRole(int id, string role)
        {
            var payload = new { Role = role };
            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            await _httpClient.PutAsync($"Auth/admin-change-role/{id}", content);

            return RedirectToAction("Index");
        }

        // 3. Xóa tài khoản
        public async Task<IActionResult> Delete(int id)
        {
            // Gọi API DeleteAccount đã có sẵn trong AuthController của bạn
            await _httpClient.DeleteAsync($"Auth/{id}");
            return RedirectToAction("Index");
        }
    }
}