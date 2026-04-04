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
    public IActionResult Login([FromBody] LoginDto request)
    {
        var user = _context.Users.SingleOrDefault(u => u.Username == request.Username && u.Password == request.Password);

        if (user == null)
        {
            return BadRequest("Sai tài khoản hoặc mật khẩu");
        }

        // PHẢI TRẢ VỀ ĐỐI TƯỢNG 'user' (Lấy từ Database, có chứa Id, FullName, Role)
        return Ok(user);
    }

    // ==========================================
    // 3. API LẤY THÔNG TIN HỒ SƠ (Dựa vào ID)
    // ==========================================
    [HttpGet("profile/{id}")]
    public IActionResult GetProfile(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy người dùng" });
        }

        // Trả về cục dữ liệu khớp với class UserProfile bên Android
        return Ok(new
        {
            username = user.Username,
            fullName = user.FullName,
            email = user.Email
        });
    }

    // ==========================================
    // 4. API CẬP NHẬT HỒ SƠ
    // ==========================================
    [HttpPut("update/{id}")]
    public IActionResult UpdateProfile(int id, [FromBody] UserProfile updateData)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound();

        // Chỉ cho phép cập nhật FullName và Username
        user.FullName = updateData.FullName;
        user.Username = updateData.Username;

        _context.SaveChanges();
        return Ok(new { message = "Cập nhật thành công!" });
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        try
        {
            // 1. Tìm người dùng trong Database
            var user = await _context.Users.FindAsync(id); // Nhớ đổi _context thành tên biến db của bạn (ví dụ: _dbContext)
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy tài khoản này!" });
            }

            // 2. DỌN DẸP DỮ LIỆU LIÊN QUAN (QUAN TRỌNG)
            // Nếu Database của bạn đã cài đặt "Cascade Delete" (Xóa dây chuyền) thì không cần đoạn này.
            // Còn nếu chưa, bạn phải tự tay xóa Giỏ hàng và Wishlist của họ trước khi xóa tài khoản.

            var userCart = _context.CartItems.Where(c => c.UserId == id);
            if (userCart.Any())
            {
                _context.CartItems.RemoveRange(userCart);
            }

            var userWishlist = _context.Wishlists.Where(w => w.UserId == id);
            if (userWishlist.Any())
            {
                _context.Wishlists.RemoveRange(userWishlist);
            }

            // 3. XÓA TÀI KHOẢN CHÍNH
            _context.Users.Remove(user);

            // 4. Lưu thay đổi xuống Database
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tài khoản đã được xóa vĩnh viễn khỏi Database." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi Server: " + ex.Message });
        }
    }
    // ==========================================
    // 5. API ĐỔI MẬT KHẨU
    // ==========================================
    [HttpPut("change-password/{id}")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy người dùng!" });
        }

        // Kiểm tra mật khẩu cũ có khớp không
        if (user.Password != request.OldPassword)
        {
            return BadRequest(new { message = "Mật khẩu cũ không chính xác!" });
        }

        // Cập nhật mật khẩu mới
        user.Password = request.NewPassword;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đổi mật khẩu thành công!" });
    }

    // Class phụ trợ để nhận dữ liệu từ Android gửi lên
    public class ChangePasswordDto
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
    // ==========================================
    // 6. API Lấy danh sách tất cả người dùng (Cho Admin)
    // ==========================================
    [HttpGet("admin-list")]
    public async Task<IActionResult> GetAllUsers()
    {
        // Lấy toàn bộ user nhưng không trả về cột Password để bảo mật
        var users = await _context.Users.Select(u => new {
            u.Id,
            u.Username,
            u.FullName,
            u.Email,
            u.Role
        }).ToListAsync();

        return Ok(users);
    }

    // DTO để hứng dữ liệu đổi quyền
    public class ChangeRoleDto
    {
        public string Role { get; set; }
    }

    // ==========================================
    // 7. API Thay đổi quyền (User <-> Admin)
    // ==========================================
    [HttpPut("admin-change-role/{id}")]
    public async Task<IActionResult> ChangeUserRole(int id, [FromBody] ChangeRoleDto request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "Không tìm thấy người dùng." });

        user.Role = request.Role;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Cập nhật quyền thành công!" });
    }
    // ==========================================
    // 8. API QUÊN MẬT KHẨU (Reset Password)
    // ==========================================
    [HttpPost("forgot-password")]
    public IActionResult ForgotPassword([FromBody] ForgotPasswordDto request)
    {
        // 1. Tìm xem trong hệ thống có ai dùng Email này không
        var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
        {
            return BadRequest(new { message = "Email này chưa được đăng ký trong hệ thống!" });
        }

        // 2. Demo cho đồ án: Tự động cấp lại mật khẩu mặc định
        user.Password = "123456";
        _context.SaveChanges();

        return Ok(new { message = "Mật khẩu của bạn đã được đặt lại thành: 123456. Vui lòng đăng nhập và đổi lại mật khẩu ngay nhé!" });
    }

    // Class hứng dữ liệu Email gửi lên
    public class ForgotPasswordDto
    {
        public string Email { get; set; }
    }
}