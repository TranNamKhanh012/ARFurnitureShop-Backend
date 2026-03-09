using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ARFurnitureAPI.Models;
using ARFurnitureAPI.Data; // Đổi tên namespace cho khớp project của bạn
using System.Linq;

namespace ARFurnitureAPI.Controllers // Nhớ bọc trong namespace của bạn nhé
{
    [Route("api/[controller]")]
    [ApiController]
    public class WishlistController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WishlistController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. LẤY DANH SÁCH SẢN PHẨM ĐÃ THẢ TIM (CỦA RIÊNG USER ĐÓ)
        // ==========================================
        [HttpGet("{userId}")]
        public IActionResult GetWishlist(int userId)
        {
            // Lọc đúng tim của user này, sau đó trích xuất lấy cái Product ra
            var products = _context.Wishlists
                .Include(w => w.Product)
                .Where(w => w.UserId == userId) // <-- BẮT BUỘC CÓ DÒNG NÀY
                .Select(w => w.Product)
                .ToList();
            return Ok(products);
        }

        // ==========================================
        // 2. THÊM TIM (KÈM USER ID)
        // ==========================================
        [HttpPost("{userId}/{productId}")]
        public IActionResult AddToWishlist(int userId, int productId)
        {
            // Kiểm tra xem USER NÀY đã thả tim sản phẩm này chưa
            if (!_context.Wishlists.Any(w => w.UserId == userId && w.ProductId == productId))
            {
                // Chưa thả thì thêm mới, nhớ gán biển tên UserId vào
                _context.Wishlists.Add(new Wishlist { UserId = userId, ProductId = productId });
                _context.SaveChanges();
            }
            return Ok();
        }

        // ==========================================
        // 3. BỎ TIM (KÈM USER ID)
        // ==========================================
        [HttpDelete("{userId}/{productId}")]
        public IActionResult RemoveFromWishlist(int userId, int productId)
        {
            // Tìm đúng trái tim của user này gắn trên sản phẩm này để xóa
            var item = _context.Wishlists.FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);
            if (item != null)
            {
                _context.Wishlists.Remove(item);
                _context.SaveChanges();
            }
            return Ok();
        }
    }
}