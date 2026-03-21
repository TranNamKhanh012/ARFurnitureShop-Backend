using ARFurnitureAPI.Data;
using ARFurnitureAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
        public async Task<ActionResult<DashboardDto>> GetDashboardSummary()
        {
            try
            {
                var dto = new DashboardDto
                {
                    // Lấy dữ liệu THẬT 100% từ bảng Products và Users
                    TotalProducts = await _context.Products.CountAsync(),
                    TotalUsers = await _context.Users.CountAsync(),

                    // Chưa có bảng Đơn hàng nên để mặc định là 0
                    SoldProducts = 0,
                    TotalOrders = 0,
                    RevenueToday = 0
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi khi tính toán thống kê: " + ex.Message);
            }
        }
    }
}