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
        // ==========================================
        // API TÌM KIẾM SẢN PHẨM
        // ==========================================
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string keyword)
        {
            // Nếu không nhập gì, trả về tất cả sản phẩm
            if (string.IsNullOrWhiteSpace(keyword))
            {
                var allProducts = await _context.Products.ToListAsync();
                return Ok(allProducts);
            }

            // Chuyển từ khóa về chữ thường để tìm kiếm không phân biệt hoa/thường
            keyword = keyword.ToLower();

            // Tìm sản phẩm có tên hoặc mô tả chứa từ khóa
            var products = await _context.Products
                .Where(p => p.Name.ToLower().Contains(keyword) || p.Description.ToLower().Contains(keyword))
                .ToListAsync();

            return Ok(products);
        }

        // ==========================================
        // API TÌM KIẾM, LỌC VÀ SẮP XẾP NÂNG CAO
        // ==========================================
        [HttpGet("filter-sort")]
        public async Task<IActionResult> FilterSortProducts(
            [FromQuery] string? query = null,
            [FromQuery] double? minPrice = null,
            [FromQuery] double? maxPrice = null,
            [FromQuery] string? sortBy = "date_desc" // Mặc định là Mới nhất
        )
        {
            var products = _context.Products.AsQueryable();

            // 1. LỌC THEO TỪ KHÓA
            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();
                products = products.Where(p => p.Name.ToLower().Contains(query) || p.Description.ToLower().Contains(query));
            }

            // 2. LỌC THEO KHOẢNG GIÁ (Dựa trên giá gốc)
            if (minPrice != null)
            {
                products = products.Where(p => p.Price >= minPrice);
            }
            if (maxPrice != null)
            {
                products = products.Where(p => p.Price <= maxPrice);
            }

            // 3. SẮP XẾP 
            switch (sortBy)
            {
                case "price_asc":
                    // Tính giá thực tế (Giá gốc - Phần trăm giảm) để xếp từ Thấp -> Cao
                    products = products.OrderBy(p => p.Price - (p.Price * p.Discount / 100.0));
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price - (p.Price * p.Discount / 100.0));
                    break;
                case "date_asc":
                    products = products.OrderBy(p => p.DateAdded); // Cũ nhất
                    break;
                case "date_desc":
                    products = products.OrderByDescending(p => p.DateAdded); // Mới nhất
                    break;
                case "rating_desc":
                    products = products.OrderByDescending(p => p.Rating); // Đánh giá cao nhất
                    break;
                default:
                    products = products.OrderByDescending(p => p.DateAdded);
                    break;
            }

            var resultList = await products.ToListAsync();
            return Ok(resultList);
        }
    }
}