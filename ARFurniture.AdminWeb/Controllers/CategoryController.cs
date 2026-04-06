using Microsoft.AspNetCore.Mvc;
using ARFurniture.AdminWeb.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace ARFurniture.AdminWeb.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly HttpClient _httpClient;

        public CategoryController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

     
        public async Task<IActionResult> Index(string searchQuery)
        {
            var model = new List<CategoryViewModel>();
            var response = await _httpClient.GetAsync("Categories");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<List<CategoryViewModel>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Lọc theo tên danh mục
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    model = model.Where(c => c.Name != null && c.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }

            ViewBag.SearchQuery = searchQuery;
            return View(model);
        }

        // --- THÊM MỚI ---
        public IActionResult Create()
        {
            return View(new CategoryViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            var content = new StringContent(JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("Categories/admin-create", content);

            if (response.IsSuccessStatusCode) return RedirectToAction("Index");

            // Đọc và in chi tiết mã lỗi từ API trả về
            var errorMsg = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"Lỗi từ API ({response.StatusCode}): {errorMsg}";
            return View(model);
        }

        // --- SỬA ---
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"Categories/admin-get/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var model = JsonSerializer.Deserialize<CategoryViewModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(model);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CategoryViewModel model)
        {
            var content = new StringContent(JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"Categories/admin-update/{model.Id}", content);

            if (response.IsSuccessStatusCode) return RedirectToAction("Index");

            // Đọc và in chi tiết mã lỗi từ API trả về
            var errorMsg = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"Lỗi từ API ({response.StatusCode}): {errorMsg}";
            return View(model);
        }

        // --- XÓA ---
        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync($"Categories/admin-delete/{id}");
            return RedirectToAction("Index");
        }
    }
}