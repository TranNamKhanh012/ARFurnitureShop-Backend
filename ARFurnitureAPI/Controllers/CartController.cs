using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ARFurnitureAPI.Models;
using ARFurnitureAPI.Data;
using System.Linq;

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
        // Chỉ lấy những sản phẩm nằm trong giỏ của đúng người dùng này
        var cart = _context.CartItems
                           .Include(c => c.Product)
                           .Where(c => c.UserId == userId)
                           .ToList();
        return Ok(cart);
    }

    // ==========================================
    // 2. THÊM VÀO GIỎ HÀNG (Kèm UserId)
    // ==========================================
    [HttpPost("{userId}/{productId}")]
    public IActionResult AddToCart(int userId, int productId)
    {
        // Tìm xem TRONG GIỎ CỦA USER NÀY đã có sản phẩm này chưa
        var item = _context.CartItems.FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);

        if (item == null)
        {
            // Chưa có thì thêm mới (nhớ gán UserId vào)
            _context.CartItems.Add(new CartItem { UserId = userId, ProductId = productId, Quantity = 1 });
        }
        else
        {
            // Có rồi thì cộng dồn số lượng
            item.Quantity += 1;
        }
        _context.SaveChanges();
        return Ok();
    }

    // ==========================================
    // 3. XÓA KHỎI GIỎ HÀNG (Kèm UserId)
    // ==========================================
    [HttpDelete("{userId}/{productId}")]
    public IActionResult RemoveFromCart(int userId, int productId)
    {
        // Tìm đúng sản phẩm trong giỏ của đúng user để xóa
        var item = _context.CartItems.FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);

        if (item != null)
        {
            _context.CartItems.Remove(item);
            _context.SaveChanges();
        }
        return Ok();
    }
}