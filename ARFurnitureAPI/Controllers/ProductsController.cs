using ARFurnitureAPI.Data;
using ARFurnitureAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARFurnitureAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products.ToListAsync();

            if (products == null || !products.Any())
            {
                return NotFound("Không có sản phẩm nào.");
            }

            return Ok(products);
        }
        // Lấy sản phẩm theo CategoryId
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory(int categoryId)
        {
            // Lọc ra các sản phẩm có CategoryId trùng khớp
            var products = await _context.Products
                                         .Where(p => p.CategoryId == categoryId)
                                         .ToListAsync();

            if (products == null || !products.Any())
            {
                return NotFound("Không có sản phẩm nào trong danh mục này.");
            }

            return Ok(products);
        }
    }
}