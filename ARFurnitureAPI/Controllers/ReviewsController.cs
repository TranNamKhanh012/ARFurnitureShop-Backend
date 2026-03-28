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

        // ==========================================
        // 4. LẤY TOÀN BỘ ĐÁNH GIÁ (Cho Admin Web)
        // ==========================================
        [HttpGet("admin-list")]
        public async Task<IActionResult> GetAllReviewsAdmin()
        {
            // Dùng LINQ Join trực tiếp bảng Reviews và Products thông qua ProductId
            var reviews = await (from r in _context.Reviews
                                 join p in _context.Products on r.ProductId equals p.Id
                                 orderby r.CreatedAt descending
                                 select new
                                 {
                                     Id = r.Id,
                                     ProductName = p.Name,       // Lấy tên sản phẩm từ bảng p
                                     FullName = r.FullName,
                                     Rating = r.Rating,
                                     Comment = r.Comment,
                                     CreatedAt = r.CreatedAt
                                 }).ToListAsync();

            return Ok(reviews);
        }

        // ==========================================
        // 5. ADMIN XÓA BÌNH LUẬN RÁC
        // ==========================================
        [HttpDelete("admin-delete/{id}")]
        public async Task<IActionResult> DeleteReviewAdmin(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound("Không tìm thấy đánh giá.");

            var productId = review.ProductId;

            // Xóa đánh giá
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            // QUAN TRỌNG: Cập nhật lại số Sao và Lượt đánh giá của Sản phẩm sau khi xóa
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                var remainingReviews = await _context.Reviews.Where(r => r.ProductId == productId).ToListAsync();
                product.ReviewCount = remainingReviews.Count;
                // Nếu xóa hết sạch review rồi thì set rating về 0
                product.Rating = remainingReviews.Any() ? Math.Round(remainingReviews.Average(r => r.Rating), 1) : 0;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Xóa đánh giá thành công!" });
        }
    }
}