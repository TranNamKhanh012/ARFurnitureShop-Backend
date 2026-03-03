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
    }
}