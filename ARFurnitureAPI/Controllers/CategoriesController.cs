using ARFurnitureAPI.Data;
using ARFurnitureAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARFurnitureAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Dependency Injection: Tiêm DbContext vào để sử dụng
        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            // Lấy toàn bộ danh sách danh mục từ Database MySQL
            var categories = await _context.Categories.ToListAsync();

            if (categories == null || !categories.Any())
            {
                return NotFound("Không tìm thấy danh mục nào.");
            }

            return Ok(categories); // Trả về dữ liệu định dạng JSON
        }
        // 1. LẤY CHI TIẾT 1 DANH MỤC (Dùng cho giao diện Edit)
        [HttpGet("admin-get/{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound("Không tìm thấy danh mục.");
            return Ok(category);
        }

        // 2. THÊM MỚI DANH MỤC
        [HttpPost("admin-create")]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Thêm danh mục thành công!" });
        }

        // 3. CẬP NHẬT DANH MỤC
        [HttpPut("admin-update/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            if (id != category.Id) return BadRequest("ID không hợp lệ.");

            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null) return NotFound("Không tìm thấy danh mục.");

            // Cập nhật thông tin
            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.ImageUrl = category.ImageUrl;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công!" });
        }

        // 4. XÓA DANH MỤC
        [HttpDelete("admin-delete/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound("Không tìm thấy danh mục.");

            // Có thể thêm logic kiểm tra xem danh mục này có sản phẩm nào không trước khi xóa
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                return BadRequest("Không thể xóa danh mục đang chứa sản phẩm. Vui lòng xóa sản phẩm trước.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa thành công!" });
        }
    }
}