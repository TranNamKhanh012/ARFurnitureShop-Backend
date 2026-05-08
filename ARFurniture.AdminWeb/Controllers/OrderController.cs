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
    public class OrderController : Controller
    {
        private readonly HttpClient _httpClient;

        public OrderController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // --- HIỂN THỊ DANH SÁCH & TÌM KIẾM & PHÂN TRANG ĐƠN HÀNG ---
        public async Task<IActionResult> Index(string searchQuery, int page = 1)
        {
            int pageSize = 10; // Số lượng đơn hàng mỗi trang
            var model = new List<OrderViewModel>();
            var response = await _httpClient.GetAsync("Orders/admin-list");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<List<OrderViewModel>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // 1. Lọc theo Tên người nhận hoặc Số điện thoại
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    model = model.Where(o =>
                        (o.ReceiverName != null && o.ReceiverName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                        (o.PhoneNumber != null && o.PhoneNumber.Contains(searchQuery))
                    ).ToList();
                }

                // 2. Logic phân trang
                int totalItems = model.Count;
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                model = model.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                // Lưu thông số vào ViewBag
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
            }

            ViewBag.SearchQuery = searchQuery;
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
            Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
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
        // Action Duyệt trả hàng
        [HttpPost]
        public async Task<IActionResult> ApproveReturn(int id)
        {
            using (var client = new HttpClient())
            {
                // ĐÃ SỬA: Thêm chữ Orders vào đường dẫn
                var response = await client.PutAsJsonAsync($"http://localhost:5103/api/Orders/admin-process-return/{id}", new { isApproved = true });

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Đã phê duyệt hoàn trả và phục hồi kho hàng thành công!";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi xử lý yêu cầu.";
                }
            }
            return RedirectToAction("Details", new { id = id });
        }

        // Action Từ chối trả hàng
        [HttpPost]
        public async Task<IActionResult> RejectReturn(int id)
        {
            using (var client = new HttpClient())
            {
                // ĐÃ SỬA: Thêm chữ Orders vào đường dẫn
                var response = await client.PutAsJsonAsync($"http://localhost:5103/api/Orders/admin-process-return/{id}", new { isApproved = false });

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Đã từ chối yêu cầu hoàn trả của khách hàng.";
                }
                else
                {
                    // BỔ SUNG: Báo lỗi nếu API gọi thất bại
                    TempData["Error"] = "Có lỗi xảy ra khi xử lý từ chối.";
                }
            }
            return RedirectToAction("Details", new { id = id });
        }
    }
}