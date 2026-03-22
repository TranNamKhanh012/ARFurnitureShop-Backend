using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ARFurnitureAPI.Data;
using ARFurnitureAPI.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ARFurnitureAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var today = DateTime.Now.Date;
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var sevenDaysAgo = today.AddDays(-6);

            // 1. TỔNG DANH MỤC & NGƯỜI DÙNG
            var totalCategories = await _context.Categories.CountAsync();
            var totalUsers = await _context.Users.CountAsync(u => u.Role == "User");

            // 2. HÀNG TRONG KHO (Tính tổng cột StockQuantity bảng Products)
            var totalStock = await _context.Products.SumAsync(p => (int?)p.StockQuantity) ?? 0;

            // 3. SẢN PHẨM ĐÃ BÁN (Tính tổng số lượng trong chi tiết đơn hàng thành công)
            // XÓA ĐOẠN LỖI NÀY ĐI
            var totalProductsSold = await _context.OrderDetails
             .Where(od => _context.Orders.Any(o => o.Id == od.OrderId && o.OrderStatus != "Cancelled"))
             .SumAsync(od => (int?)od.Quantity) ?? 0;

            // 4. DOANH THU (Hôm nay, 7 ngày, Tháng)
            var todayRevenue = await _context.Orders
                .Where(o => o.OrderStatus != "Cancelled" && o.OrderDate.Date == today)
                .SumAsync(o => (double?)o.TotalAmount) ?? 0;

            var weeklyRevenueTotal = await _context.Orders
                .Where(o => o.OrderStatus != "Cancelled" && o.OrderDate.Date >= sevenDaysAgo)
                .SumAsync(o => (double?)o.TotalAmount) ?? 0;

            var monthlyRevenue = await _context.Orders
                .Where(o => o.OrderStatus != "Cancelled" && o.OrderDate.Month == currentMonth && o.OrderDate.Year == currentYear)
                .SumAsync(o => (double?)o.TotalAmount) ?? 0;

            // 5. BIỂU ĐỒ & ĐƠN HÀNG GẦN ĐÂY (Giữ nguyên logic cũ)
            var recentOrders = await _context.Orders.OrderByDescending(o => o.OrderDate).Take(5)
                .Select(o => new RecentOrderDto { Id = o.Id, ReceiverName = o.ReceiverName, TotalAmount = o.TotalAmount, OrderStatus = o.OrderStatus, OrderDate = o.OrderDate }).ToListAsync();

            var ordersLast7Days = await _context.Orders.Where(o => o.OrderDate >= sevenDaysAgo && o.OrderStatus != "Cancelled").Select(o => new { o.OrderDate, o.TotalAmount }).ToListAsync();
            var weeklyRevenueList = new List<DailyRevenueDto>();
            for (int i = 0; i < 7; i++)
            {
                var currentDate = sevenDaysAgo.AddDays(i);
                weeklyRevenueList.Add(new DailyRevenueDto { Date = currentDate.ToString("dd/MM"), Revenue = ordersLast7Days.Where(o => o.OrderDate.Date == currentDate.Date).Sum(o => o.TotalAmount) });
            }

            return Ok(new DashboardDto
            {
                TotalCategories = totalCategories,
                TotalProductsSold = totalProductsSold,
                TotalStock = totalStock,
                TotalUsers = totalUsers,
                TodayRevenue = todayRevenue,
                WeeklyRevenueTotal = weeklyRevenueTotal,
                MonthlyRevenue = monthlyRevenue,
                RecentOrders = recentOrders,
                WeeklyRevenue = weeklyRevenueList
            });
        }
    }
}