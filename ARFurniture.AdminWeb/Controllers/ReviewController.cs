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

        // --- HIỂN THỊ DANH SÁCH & TÌM KIẾM & PHÂN TRANG PHẢN HỒI ---
        public async Task<IActionResult> Index(string searchQuery, int page = 1)
        {
            int pageSize = 10; // Số lượng phản hồi mỗi trang
            var model = new List<ReviewViewModel>();
            var response = await _httpClient.GetAsync("Reviews/admin-list");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<List<ReviewViewModel>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // 1. Lọc theo Tên khách hàng (FullName) hoặc Tên sản phẩm (ProductName)
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    model = model.Where(r =>
                        (r.FullName != null && r.FullName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (r.ProductName != null && r.ProductName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                // 2. Logic phân trang
                int totalItems = model.Count;
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                // Lấy dữ liệu của trang hiện tại
                model = model.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                // Lưu thông số vào ViewBag để View sử dụng
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
            }

            ViewBag.SearchQuery = searchQuery;
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