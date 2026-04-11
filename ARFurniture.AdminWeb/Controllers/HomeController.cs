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
        // ==========================================
        // HÀM XUẤT BÁO CÁO RA FILE EXCEL (CSV)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ExportReport()
        {
            var model = new DashboardViewModel();

            try
            {
                // Gọi lại API để lấy số liệu mới nhất
                var response = await _httpClient.GetAsync("http://localhost:5103/api/Dashboard/summary");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    model = JsonSerializer.Deserialize<DashboardViewModel>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            catch
            {
                // Bỏ qua lỗi nếu API chưa bật
            }

            // Dùng StringBuilder để vẽ cấu trúc file Excel
            var builder = new System.Text.StringBuilder();

            // Mẹo cực hay: Thêm mã BOM (Byte Order Mark) để Excel mở tiếng Việt có dấu không bị lỗi font
            builder.Append('\uFEFF');

            // --- VIẾT NỘI DUNG FILE ---
            builder.AppendLine("BÁO CÁO TỔNG QUAN KINH DOANH - ACCESSORIES");
            builder.AppendLine($"Ngày xuất: {System.DateTime.Now.ToString("dd/MM/yyyy HH:mm")}");
            builder.AppendLine(""); // Dòng trống

            builder.AppendLine("1. CHỈ SỐ DOANH THU & KHO");
            builder.AppendLine($"Doanh thu hôm nay:,{model.TodayRevenue} đ");
            builder.AppendLine($"Doanh thu 7 ngày:,{model.WeeklyRevenueTotal} đ");
            builder.AppendLine($"Doanh thu tháng hiện tại:,{model.MonthlyRevenue} đ");
            builder.AppendLine($"Thuế dự kiến (1.5%):,{model.MonthlyTax} đ");
            builder.AppendLine($"Tổng số lượng sản phẩm đã bán:,{model.TotalProductsSold}");
            builder.AppendLine($"Tổng hàng trong kho:,{model.TotalStock}");
            builder.AppendLine($"Tổng số khách hàng:,{model.TotalUsers}");
            builder.AppendLine("");

            builder.AppendLine("2. 5 GIAO DỊCH GẦN NHẤT");
            builder.AppendLine("Mã Đơn,Người Nhận,Thời Gian,Tổng Tiền");

            if (model.RecentOrders != null)
            {
                foreach (var order in model.RecentOrders)
                {
                    // Ngăn cách các cột bằng dấu phẩy
                    builder.AppendLine($"#{order.Id},{order.ReceiverName},{order.OrderDate.ToString("dd/MM/yyyy HH:mm")},{order.TotalAmount} đ");
                }
            }

            // --- ĐÓNG GÓI VÀ TRẢ FILE VỀ CHO TRÌNH DUYỆT TẢI XUỐNG ---
            string fileName = $"BaoCao_KinhDoanh_{System.DateTime.Now.ToString("ddMMyyyy")}.csv";
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());

            return File(fileBytes, "text/csv", fileName);
        }

        // ... các hàm khác giữ nguyên ...
    }
}