using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ARFurnitureAPI.Models;
using ARFurnitureAPI.Data;
using System.Linq;

namespace ARFurnitureAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. LẤY GIỎ HÀNG THEO USER ID
        // ==========================================
        [HttpGet("{userId}")]
        public IActionResult GetCart(int userId)
        {
            var cart = _context.CartItems
                               .Include(c => c.Product)
                               .Where(c => c.UserId == userId)
                               .ToList();
            return Ok(cart);
        }

        // ==========================================
        // 2. THÊM VÀO GIỎ HÀNG (Nhận chuẩn Số lượng & Size)
        // ==========================================
        [HttpPost("add")]
        public IActionResult AddToCart([FromQuery] int userId, [FromQuery] int productId, [FromQuery] int quantity, [FromQuery] string? selectedSize = "")
        {
            // Kiểm tra xem giỏ đã có đúng SẢN PHẨM này và đúng SIZE này chưa
            var item = _context.CartItems.FirstOrDefault(c =>
                c.UserId == userId &&
                c.ProductId == productId &&
                c.SelectedSize == selectedSize);

            if (item == null)
            {
                _context.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    SelectedSize = selectedSize
                });
            }
            else
            {
                // Có rồi thì cộng dồn số lượng khách vừa chọn thêm
                item.Quantity += quantity;
            }

            _context.SaveChanges();
            return Ok();
        }

        // ==========================================
        // 3. XÓA KHỎI GIỎ HÀNG (Chỉ xóa đúng Size được chọn)
        // ==========================================
        [HttpDelete("remove")]
        public IActionResult RemoveFromCart([FromQuery] int userId, [FromQuery] int productId, [FromQuery] string? selectedSize = "")
        {
            var item = _context.CartItems.FirstOrDefault(c =>
                c.UserId == userId &&
                c.ProductId == productId &&
                c.SelectedSize == selectedSize);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                _context.SaveChanges();
            }
            return Ok();
        }
    }
}