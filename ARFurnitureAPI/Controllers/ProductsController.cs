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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            var allSizes = await _context.ProductSizes.ToListAsync();

            foreach (var p in products)
            {
                // MA THUẬT Ở ĐÂY: Chỉ lấy những Size có StockQuantity > 0
                var availableSizes = allSizes
                    .Where(s => s.ProductId == p.Id && s.StockQuantity > 0)
                    .Select(s => s.SizeName);

                p.Sizes = string.Join(",", availableSizes); // Ghép lại thành "39,41"
            }

            return Ok(products);
        }

        // Lấy sản phẩm theo CategoryId
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory(int categoryId)
        {
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
            if (string.IsNullOrWhiteSpace(keyword))
            {
                var allProducts = await _context.Products.ToListAsync();
                return Ok(allProducts);
            }

            keyword = keyword.ToLower();
            var products = await _context.Products
                .Where(p => p.Name.ToLower().Contains(keyword) || p.Description.ToLower().Contains(keyword))
                .ToListAsync();

            return Ok(products);
        }

        // ==========================================
        // API TÌM KIẾM, LỌC VÀ SẮP XẾP NÂNG CAO (ĐÃ THÊM TÌM KIẾM KHÔNG DẤU)
        // ==========================================
        [HttpGet("filter-sort")]
        public async Task<IActionResult> FilterSortProducts(
            [FromQuery] string? query = null,
            [FromQuery] double? minPrice = null,
            [FromQuery] double? maxPrice = null,
            [FromQuery] string? sortBy = "date_desc"
        )
        {
            // 1. Kéo danh sách về trước để xử lý chuỗi tiếng Việt dễ dàng hơn
            var products = await _context.Products.ToListAsync();

            // 2. TÌM KIẾM TƯƠNG ĐỐI (KHÔNG DẤU, KHÔNG PHÂN BIỆT HOA THƯỜNG)
            if (!string.IsNullOrWhiteSpace(query))
            {
                // Gọt sạch dấu từ khóa khách hàng nhập
                string searchKeyword = RemoveVietnameseAccents(query).ToLower();

                products = products.Where(p =>
                    // Gọt dấu Tên sản phẩm trong Database để so sánh
                    RemoveVietnameseAccents(p.Name).ToLower().Contains(searchKeyword) ||
                    // Gọt dấu cả trong Mô tả sản phẩm
                    (p.Description != null && RemoveVietnameseAccents(p.Description).ToLower().Contains(searchKeyword))
                ).ToList();
            }

            // 3. Lọc theo giá
            if (minPrice != null) products = products.Where(p => p.Price >= minPrice.Value).ToList();
            if (maxPrice != null) products = products.Where(p => p.Price <= maxPrice.Value).ToList();

            // 4. Sắp xếp (Dựa trên giá sau khi đã tính chiết khấu)
            switch (sortBy)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price - (p.Price * p.Discount / 100.0)).ToList();
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price - (p.Price * p.Discount / 100.0)).ToList();
                    break;
                case "date_asc":
                    products = products.OrderBy(p => p.DateAdded).ToList();
                    break;
                case "date_desc":
                    products = products.OrderByDescending(p => p.DateAdded).ToList();
                    break;
                case "rating_desc":
                    products = products.OrderByDescending(p => p.Rating).ToList();
                    break;
                default:
                    products = products.OrderByDescending(p => p.DateAdded).ToList();
                    break;
            }

            return Ok(products);
        }

        [HttpGet("admin-list")]
        public async Task<IActionResult> GetAdminProducts()
        {
            var products = await _context.Products
                .Select(p => new AdminProductDto
                {
                    Id = p.Id,
                    ImageUrl = p.ImageUrl,
                    Name = p.Name,
                    OriginalPrice = p.Price,
                    SellingPrice = p.Discount > 0 ? p.Price - (p.Price * p.Discount / 100.0) : p.Price,
                    StockQuantity = p.StockQuantity,
                    SoldQuantity = _context.OrderDetails
                        .Where(od => _context.Orders.Any(o => o.Id == od.OrderId && o.OrderStatus != "Cancelled") && od.ProductId == p.Id)
                        .Sum(od => (int?)od.Quantity) ?? 0
                })
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return Ok(products);
        }

        // ==========================================
        // CÁC HÀM XỬ LÝ FORM THÊM/SỬA/XÓA
        // ==========================================

        // Lấy 1 sản phẩm để hiện lên form Sửa (Hiển thị format 39:10)
        [HttpGet("admin-get/{id}")]
        public async Task<IActionResult> AdminGetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var sizes = await _context.ProductSizes
                .Where(s => s.ProductId == id)
                .Select(s => $"{s.SizeName}:{s.StockQuantity}") // Nối lại thành "39:10"
                .ToListAsync();

            var dto = new AdminProductFormDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Discount = product.Discount,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                Description = product.Description,
                CategoryId = product.CategoryId,
                Sizes = sizes.Any() ? string.Join(", ", sizes) : ""
            };
            return Ok(dto);
        }

        // HÀM XỬ LÝ LƯU SIZE VÀO DB (Dùng chung logic cho cả Create và Update)
        private async Task ProcessProductSizes(int productId, string? sizesInput)
        {
            if (string.IsNullOrWhiteSpace(sizesInput)) return;

            var sizeList = sizesInput.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in sizeList)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    string sizeName = item.Trim();
                    int qty = 50; // Mặc định nếu Admin lười chỉ nhập "39" thay vì "39:10"

                    if (item.Contains(":"))
                    {
                        var parts = item.Split(':', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                        {
                            sizeName = parts[0].Trim();
                            int.TryParse(parts[1].Trim(), out qty);
                        }
                    }

                    _context.ProductSizes.Add(new ProductSize
                    {
                        ProductId = productId,
                        SizeName = sizeName,
                        StockQuantity = qty
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        [HttpPost("admin-create")]
        public async Task<IActionResult> AdminCreateProduct([FromBody] AdminProductFormDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Discount = dto.Discount,
                StockQuantity = dto.StockQuantity,
                ImageUrl = dto.ImageUrl,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                DateAdded = System.DateTime.Now
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await ProcessProductSizes(product.Id, dto.Sizes); // Gọi hàm cắt size
            return Ok(new { Message = "Thêm thành công!" });
        }

        [HttpPut("admin-update/{id}")]
        public async Task<IActionResult> AdminUpdateProduct(int id, [FromBody] AdminProductFormDto dto)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null) return NotFound();

            existingProduct.Name = dto.Name; existingProduct.Price = dto.Price; existingProduct.Discount = dto.Discount;
            existingProduct.StockQuantity = dto.StockQuantity; existingProduct.ImageUrl = dto.ImageUrl;
            existingProduct.Description = dto.Description; existingProduct.CategoryId = dto.CategoryId;

            // Xóa size cũ đi
            var oldSizes = _context.ProductSizes.Where(s => s.ProductId == id);
            _context.ProductSizes.RemoveRange(oldSizes);

            // Thêm lại size mới
            await ProcessProductSizes(existingProduct.Id, dto.Sizes);

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Cập nhật thành công!" });
        }

        // 4. Xóa sản phẩm (ĐÃ SỬA: Xóa Size trước để không bị lỗi)
        [HttpDelete("admin-delete/{id}")]
        public async Task<IActionResult> AdminDeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            // 1. Xóa liên kết Giỏ hàng
            var cartItems = _context.CartItems.Where(c => c.ProductId == id);
            _context.CartItems.RemoveRange(cartItems);

            // 2. Xóa các Size liên quan
            var productSizes = _context.ProductSizes.Where(s => s.ProductId == id);
            _context.ProductSizes.RemoveRange(productSizes);

            // 3. Xóa sản phẩm chính
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Xóa thành công!" });
        }

        // ===================================================
        // HÀM TIỆN ÍCH: XÓA DẤU TIẾNG VIỆT
        // ===================================================
        private string RemoveVietnameseAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            string[] vietnameseSigns = new string[] {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ", "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ", "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ", "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ", "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ", "ÍÌỊỈĨ",
                "đ", "Đ",
                "ýỳỵỷỹ", "ÝỲỴỶỸ"
            };

            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                    text = text.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
            }
            return text;
        }

    }
}