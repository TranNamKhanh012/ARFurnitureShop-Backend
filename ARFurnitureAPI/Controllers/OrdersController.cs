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
    }
}