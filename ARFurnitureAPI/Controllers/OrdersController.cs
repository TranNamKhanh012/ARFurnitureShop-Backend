using Microsoft.AspNetCore.Mvc;
using ARFurnitureAPI.Data;
using ARFurnitureAPI.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ARFurnitureAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDto request)
        {
            if (request == null || request.Items == null || request.Items.Count == 0)
            {
                return BadRequest("Dữ liệu đơn hàng không hợp lệ!");
            }

            // 1. Lưu thông tin Đơn Hàng (Bảng Orders)
            var newOrder = new Order
            {
                UserId = request.UserId,
                TotalAmount = request.TotalAmount,
                PaymentMethod = request.PaymentMethod,
                ShippingAddress = request.ShippingAddress,
                PhoneNumber = request.PhoneNumber,
                ReceiverName = request.ReceiverName,
                OrderDate = System.DateTime.Now,
                OrderStatus = "Pending",
                PaymentStatus = request.PaymentMethod == "COD" ? "Unpaid" : "Paid"
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync(); // Lưu để lấy được OrderId mới tạo

            // 2. Lưu chi tiết và TRỪ TỒN KHO
            foreach (var item in request.Items)
            {
                // A. Lưu vào lịch sử mua hàng (Thêm SelectedSize để biết khách mua size gì)
                var orderDetail = new OrderDetail
                {
                    OrderId = newOrder.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    SelectedSize = item.SelectedSize // Lưu lại size khách đặt
                };
                _context.OrderDetails.Add(orderDetail);

                // B. TRỪ TỒN KHO TỔNG CỦA SẢN PHẨM
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                    if (product.StockQuantity < 0) product.StockQuantity = 0; // Chống âm kho
                }

                // C. TRỪ TỒN KHO CỦA RIÊNG SIZE ĐÓ (Nếu sản phẩm có chọn Size)
                if (!string.IsNullOrEmpty(item.SelectedSize))
                {
                    var sizeStock = await _context.ProductSizes
                        .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.SizeName == item.SelectedSize);

                    if (sizeStock != null)
                    {
                        sizeStock.StockQuantity -= item.Quantity;
                        if (sizeStock.StockQuantity < 0) sizeStock.StockQuantity = 0; // Chống âm kho
                    }
                }
            }

            // 3. Xóa giỏ hàng của User sau khi đặt thành công
            var cartItems = _context.CartItems.Where(c => c.UserId == request.UserId).ToList();
            if (cartItems.Any())
            {
                _context.CartItems.RemoveRange(cartItems);
            }

            // 4. Lưu tất cả thay đổi (OrderDetails, Trừ kho, Xóa giỏ) vào Database cùng 1 lúc
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đặt hàng thành công!", OrderId = newOrder.Id });
        }
        // ==========================================
        // API LẤY LỊCH SỬ ĐƠN HÀNG CỦA USER
        // ==========================================
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate) // Đơn mới nhất xếp lên đầu
                .Select(o => new {
                    o.Id,
                    o.OrderDate,
                    o.TotalAmount,
                    o.OrderStatus,
                    o.PaymentMethod,
                    o.PaymentStatus
                })
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                return NotFound(new { message = "Bạn chưa có đơn hàng nào." });
            }

            return Ok(orders);
        }
        // Dto nhỏ để nhận dữ liệu trạng thái
        public class UpdateStatusDto
        {
            public string Status { get; set; }
        }

        // 1. API Lấy toàn bộ danh sách đơn hàng cho Admin
        [HttpGet("admin-list")]
        public async Task<IActionResult> GetAdminOrders()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new {
                    o.Id,
                    o.ReceiverName,
                    o.PhoneNumber,
                    o.ShippingAddress,
                    o.TotalAmount,
                    o.OrderDate,
                    o.OrderStatus,
                    o.PaymentMethod
                }).ToListAsync();

            return Ok(orders);
        }

        // 2. API Cập nhật trạng thái đơn hàng (Pending -> Shipping -> Completed)
        [HttpPut("admin-update-status/{id}")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusDto request)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

            order.OrderStatus = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật trạng thái thành công" });
        }
        // 3. API Lấy chi tiết 1 đơn hàng (Bao gồm danh sách sản phẩm)
        [HttpGet("admin-get/{id}")]
        public async Task<IActionResult> GetAdminOrderDetail(int id)
        {
            // Lấy thông tin hóa đơn và nối (Join) với bảng OrderDetails và Products
            var order = await _context.Orders
                .Where(o => o.Id == id)
                .Select(o => new {
                    o.Id,
                    o.ReceiverName,
                    o.PhoneNumber,
                    o.ShippingAddress,
                    o.TotalAmount,
                    o.OrderDate,
                    o.OrderStatus,
                    o.PaymentMethod,
                    o.PaymentStatus,
                    // Lấy danh sách các món đồ trong đơn hàng này
                    Items = _context.OrderDetails
                        .Where(od => od.OrderId == o.Id)
                        .Select(od => new {
                            od.ProductId,
                            ProductName = _context.Products.FirstOrDefault(p => p.Id == od.ProductId).Name,
                            ProductImage = _context.Products.FirstOrDefault(p => p.Id == od.ProductId).ImageUrl,
                            od.Quantity,
                            od.UnitPrice,
                            od.SelectedSize
                        }).ToList()
                }).FirstOrDefaultAsync();

            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng!" });

            return Ok(order);
        }
        // ==========================================
        // KHÁCH HÀNG XÁC NHẬN ĐÃ NHẬN HÀNG
        // ==========================================
        [HttpPut("user-confirm/{id}")]
        public async Task<IActionResult> UserConfirmOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

            if (!order.OrderStatus.Equals("Shipping", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Chỉ có thể xác nhận khi đơn hàng đang được giao." });
            }

            // Chuyển trạng thái thành Hoàn thành
            order.OrderStatus = "Completed";

            // Nếu khách chọn COD (Thanh toán khi nhận hàng), thì nhận hàng xong coi như đã trả tiền
            if (order.PaymentMethod == "COD")
            {
                order.PaymentStatus = "Paid";
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Xác nhận nhận hàng thành công!" });
        }
    }
}