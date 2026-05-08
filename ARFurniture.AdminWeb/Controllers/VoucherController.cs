using ARFurniture.AdminWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ARFurniture.AdminWeb.Controllers
{
    public class VoucherController : Controller
    {
        private readonly string _apiUrl = "http://localhost:5103/api/Vouchers"; // CHÚ Ý: Đổi link localhost này cho khớp với Port API của bạn đang chạy

        public async Task<IActionResult> Index()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(_apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var vouchers = JsonConvert.DeserializeObject<List<VoucherViewModel>>(data);
                    return View(vouchers);
                }
            }
            return View(new List<VoucherViewModel>());
        }
        // Màn hình Thêm mới
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(VoucherViewModel model)
        {
            using (var client = new HttpClient())
            {
                // Gửi dữ liệu sang API
                var response = await client.PostAsJsonAsync(_apiUrl, model);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index"); // Thành công thì về trang danh sách
                }
                else
                {
                    // Nếu thất bại, đọc lỗi từ API và ném ra màn hình
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, "Lỗi từ API: " + errorMsg);
                }
            }
            return View(model);
        }

        // Màn hình Chỉnh sửa
        public async Task<IActionResult> Edit(int id)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{_apiUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var voucher = JsonConvert.DeserializeObject<VoucherViewModel>(data);
                    return View(voucher);
                }
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, VoucherViewModel model)
        {
            using (var client = new HttpClient())
            {
                var response = await client.PutAsJsonAsync($"{_apiUrl}/{id}", model);
                if (response.IsSuccessStatusCode) return RedirectToAction("Index");
            }
            return View(model);
        }

        // Xử lý Xóa
        public async Task<IActionResult> Delete(int id)
        {
            using (var client = new HttpClient())
            {
                // Gửi yêu cầu DELETE lên API
                var response = await client.DeleteAsync($"{_apiUrl}/{id}");

                if (response.IsSuccessStatusCode)
                {
                    // Xóa xong thì quay về trang danh sách
                    return RedirectToAction("Index");
                }
            }

            // Nếu có lỗi thì có thể thông báo hoặc trả về trang lỗi
            return BadRequest("Không thể xóa Voucher này.");
        }
    }
}