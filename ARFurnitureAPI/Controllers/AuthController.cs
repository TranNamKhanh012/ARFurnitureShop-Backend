using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ARFurnitureAPI.Models; // Sửa lại cho khớp namespace của bạn
using System.Linq;
using ARFurnitureAPI.Data;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // 1. API ĐĂNG KÝ (REGISTER)
    // ==========================================
    [HttpPost("register")]
    public IActionResult Register([FromBody] User user)
    {
        // Kiểm tra xem Username đã có ai dùng chưa
        if (_context.Users.Any(u => u.Username == user.Username))
        {
            return BadRequest(new { message = "Tên đăng nhập đã tồn tại!" });
        }

        // Mặc định khách hàng tự đăng ký sẽ có quyền 'User'
        user.Role = "User";

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok(new { message = "Đăng ký thành công!" });
    }

    // ==========================================
    // 2. API ĐĂNG NHẬP (LOGIN)
    // ==========================================
    [HttpPost("login")]
    // Đổi [FromBody] User thành [FromBody] LoginDto
    public IActionResult Login([FromBody] LoginDto request)
    {
        // Sửa lại các biến bên trong cho khớp với request mới
        var user = _context.Users.SingleOrDefault(u => u.Username == request.Username && u.Password == request.Password);

        if (user == null)
        {
            return BadRequest("Sai tài khoản hoặc mật khẩu");
        }

        return Ok(user);
    }
}