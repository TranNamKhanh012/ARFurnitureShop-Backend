using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ARFurnitureAPI.Data;
using ARFurnitureAPI.Models;
using System;
using System.Collections.Generic;
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
            try
            {
                var today = DateTime.Now.Date;
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var sevenDaysAgo = today.AddDays(-6);

                // 1. TỔNG DANH MỤC & NGƯỜI DÙNG
                var totalCategories = await _context.Categories.CountAsync();
                var totalUsers = await _context.Users.CountAsync(u => u.Role == "User");

                // 2. HÀNG TRONG KHO (Dùng ?? 0 an toàn)
                var totalStock = await _context.Products.SumAsync(p => (int?)p.StockQuantity) ?? 0;

                // 3. SẢN PHẨM ĐÃ BÁN
                var totalProductsSold = await _context.OrderDetails
                    .Where(od => _context.Orders.Any(o => o.Id == od.OrderId && o.OrderStatus != "Cancelled"))
                    .SumAsync(od => (int?)od.Quantity) ?? 0;

                // 4. DOANH THU (Bắt buộc phải check o.OrderDate != null và dùng .Value)
                var todayRevenue = await _context.Orders
                    .Where(o => o.OrderStatus != "Cancelled" && o.OrderDate != null && o.OrderDate.Value.Date == today)
                    .SumAsync(o => (double?)o.TotalAmount) ?? 0;

                var weeklyRevenueTotal = await _context.Orders
                    .Where(o => o.OrderStatus != "Cancelled" && o.OrderDate != null && o.OrderDate.Value.Date >= sevenDaysAgo)
                    .SumAsync(o => (double?)o.TotalAmount) ?? 0;

                var monthlyRevenue = await _context.Orders
                    .Where(o => o.OrderStatus != "Cancelled" && o.OrderDate != null && o.OrderDate.Value.Month == currentMonth && o.OrderDate.Value.Year == currentYear)
                    .SumAsync(o => (double?)o.TotalAmount) ?? 0;

                var monthlyTax = monthlyRevenue * 0.015;

                // 5. ĐƠN HÀNG GẦN ĐÂY (Dùng ?? để ép kiểu rỗng thành giá trị mặc định)
                var recentOrders = await _context.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .Select(o => new RecentOrderDto
                    {
                        Id = o.Id,
                        ReceiverName = o.ReceiverName ?? "Khách hàng",
                        TotalAmount = o.TotalAmount ?? 0,
                        OrderStatus = o.OrderStatus ?? "Pending",
                        OrderDate = o.OrderDate ?? DateTime.Now
                    }).ToListAsync();

                // 6. BIỂU ĐỒ DOANH THU 7 NGÀY
                var ordersLast7Days = await _context.Orders
                    .Where(o => o.OrderDate != null && o.OrderDate.Value.Date >= sevenDaysAgo && o.OrderStatus != "Cancelled")
                    .Select(o => new { OrderDate = o.OrderDate.Value, TotalAmount = o.TotalAmount ?? 0 })
                    .ToListAsync();

                var weeklyRevenueList = new List<DailyRevenueDto>();
                for (int i = 0; i < 7; i++)
                {
                    var currentDate = sevenDaysAgo.AddDays(i);
                    weeklyRevenueList.Add(new DailyRevenueDto
                    {
                        Date = currentDate.ToString("dd/MM"),
                        Revenue = ordersLast7Days.Where(o => o.OrderDate.Date == currentDate.Date).Sum(o => o.TotalAmount)
                    });
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
                    MonthlyTax = monthlyTax,
                    RecentOrders = recentOrders,
                    WeeklyRevenue = weeklyRevenueList
                });
            }
            catch (Exception)
            {
                // Trả về JSON rỗng thay vì làm Crash Server
                return Ok(new DashboardDto());
            }
        }
    }
}