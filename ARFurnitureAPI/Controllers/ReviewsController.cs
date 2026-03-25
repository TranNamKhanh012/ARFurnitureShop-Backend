using Microsoft.AspNetCore.Mvc;
using ARFurnitureAPI.Data;
using ARFurnitureAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ARFurnitureAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ReviewsController(AppDbContext context) { _context = context; }

        // 1. TÌM CÁC MÓN HÀNG CHƯA ĐÁNH GIÁ (DÙNG CHO CHUÔNG THÔNG BÁO)
        [HttpGet("pending/{userId}")]
        public async Task<IActionResult> GetPendingReviews(int userId)
        {
            // Tìm các mặt hàng khách đã mua ở đơn hàng "Completed"
            var completedProductIds = await _context.OrderDetails
                .Include(od => od.Order)
                .Where(od => od.Order.UserId == userId && od.Order.OrderStatus == "Completed")
                .Select(od => od.ProductId)
                .Distinct()
                .ToListAsync();

            // Tìm các mặt hàng khách đã đánh giá rồi
            var reviewedProductIds = await _context.Reviews
                .Where(r => r.UserId == userId)
                .Select(r => r.ProductId)
                .ToListAsync();

            // Phép trừ: Đã mua trừ đi Đã đánh giá = Cần đánh giá
            var pendingProductIds = completedProductIds.Except(reviewedProductIds).ToList();

            var pendingProducts = await _context.Products
                .Where(p => pendingProductIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Name, p.ImageUrl, p.Price })
                .ToListAsync();

            return Ok(pendingProducts);
        }

        // 2. GỬI ĐÁNH GIÁ MỚI LÊN
        [HttpPost("create")]
        public async Task<IActionResult> CreateReview([FromBody] Review review)
        {
            review.CreatedAt = DateTime.Now;
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Tự động tính lại Điểm trung bình (Rating) và Số lượt đánh giá (ReviewCount)
            var product = await _context.Products.FindAsync(review.ProductId);
            if (product != null)
            {
                var allReviews = await _context.Reviews.Where(r => r.ProductId == review.ProductId).ToListAsync();
                product.ReviewCount = allReviews.Count;
                product.Rating = Math.Round(allReviews.Average(r => r.Rating), 1);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Đánh giá thành công!" });
        }

        // 3. LẤY ĐÁNH GIÁ CỦA SẢN PHẨM ĐỂ HIỆN Ở TRANG CHI TIẾT
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return Ok(reviews);
        }
    }
}