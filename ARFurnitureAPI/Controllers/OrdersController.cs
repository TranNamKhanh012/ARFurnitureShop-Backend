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
                VoucherId = request.VoucherId,
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

            if (request.VoucherId.HasValue && request.VoucherId.Value > 0)
            {
                var voucher = _context.Vouchers.Find(request.VoucherId.Value);
                if (voucher != null && voucher.UsageLimit > 0)
                {
                    voucher.UsageLimit -= 1; // Trừ đi 1 lượt sử dụng
                }
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

        // 1. API Lấy danh sách đơn hàng (PHIÊN BẢN CHỐNG LỖI DATA IS NULL)
        [HttpGet("admin-list")]
        public async Task<IActionResult> GetAdminOrders()
        {
            try
            {
                var orders = await _context.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new {
                        Id = o.Id,
                        // Dùng ?? để ép giá trị rỗng thành chuỗi an toàn
                        ReceiverName = o.ReceiverName ?? "Khách hàng",
                        PhoneNumber = o.PhoneNumber ?? "N/A",
                        ShippingAddress = o.ShippingAddress ?? "N/A",
                        TotalAmount = o.TotalAmount,
                        OrderDate = o.OrderDate,
                        OrderStatus = o.OrderStatus ?? "Pending",
                        PaymentMethod = o.PaymentMethod ?? "COD"
                    }).ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                // Nếu vẫn cố tình lỗi, ném thẳng nguyên nhân ra ngoài
                return StatusCode(500, "LỖI TẠI API ADMIN-LIST: " + ex.Message);
            }
        }

        // 2. API Cập nhật trạng thái đơn hàng (Bọc try-catch an toàn)
        [HttpPut("admin-update-status/{id}")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusDto request)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

                order.OrderStatus = request.Status;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "LỖI UPDATE STATUS: " + ex.Message);
            }
        }
        // 3. API Lấy chi tiết 1 đơn hàng (PHIÊN BẢN TỐI THƯỢNG - BYPASS 100% LỖI NULL)
        [HttpGet("admin-get/{id}")]
        public async Task<IActionResult> GetAdminOrderDetail(int id)
        {
            try
            {
                // BƯỚC 1: Lấy đơn hàng bằng Select thay vì FindAsync (Ép kiểu an toàn mọi giá trị)
                var order = await _context.Orders
                    .Where(o => o.Id == id)
                    .Select(o => new {
                        Id = o.Id,
                        ReceiverName = o.ReceiverName ?? "",
                        PhoneNumber = o.PhoneNumber ?? "",
                        ShippingAddress = o.ShippingAddress ?? "",
                        TotalAmount = o.TotalAmount,
                        OrderDate = o.OrderDate,
                        OrderStatus = o.OrderStatus ?? "Pending",
                        PaymentMethod = o.PaymentMethod ?? "",
                        PaymentStatus = o.PaymentStatus ?? "",
                        ReturnReason = o.ReturnReason ?? "",
                        // Ép kiểu trực tiếp VoucherId thành int? để chống lỗi Data is Null
                        VoucherId = (int?)o.VoucherId
                    })
                    .FirstOrDefaultAsync();

                if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng!" });

                // BƯỚC 2: Xử lý Voucher an toàn
                string voucherCode = null;
                string discountInfo = null;

                if (order.VoucherId != null)
                {
                    var voucher = await _context.Vouchers
                        .Where(v => v.Id == order.VoucherId)
                        .Select(v => new {
                            v.Code,
                            v.DiscountType,
                            // Ép kiểu an toàn để phòng hờ DiscountValue bị rỗng
                            DiscountValue = (double?)v.DiscountValue
                        })
                        .FirstOrDefaultAsync();

                    if (voucher != null)
                    {
                        voucherCode = voucher.Code;
                        discountInfo = voucher.DiscountType == "FixedAmount"
                            ? $"-{voucher.DiscountValue:N0}đ"
                            : $"-{voucher.DiscountValue}%";
                    }
                }

                // BƯỚC 3: Lấy chi tiết đơn hàng (Dùng Select để chống lỗi ProductId bị xóa/null)
                var itemsList = await _context.OrderDetails
                    .Where(od => od.OrderId == id)
                    .Select(od => new {
                        ProductId = (int?)od.ProductId, // Ép kiểu an toàn ngay trong query
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        SelectedSize = od.SelectedSize ?? ""
                    })
                    .ToListAsync();

                var finalItems = new List<object>();

                foreach (var item in itemsList)
                {
                    string pName = "Sản phẩm đã bị xóa";
                    string pImg = "";

                    if (item.ProductId != null)
                    {
                        var product = await _context.Products
                            .Where(p => p.Id == item.ProductId)
                            .Select(p => new { p.Name, p.ImageUrl })
                            .FirstOrDefaultAsync();

                        if (product != null)
                        {
                            pName = product.Name;
                            pImg = product.ImageUrl;
                        }
                    }

                    finalItems.Add(new
                    {
                        ProductId = item.ProductId ?? 0,
                        ProductName = pName,
                        ProductImage = pImg,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        SelectedSize = item.SelectedSize
                    });
                }

                // BƯỚC 4: Đóng gói trả về Web Admin
                return Ok(new
                {
                    Id = order.Id,
                    ReceiverName = order.ReceiverName,
                    PhoneNumber = order.PhoneNumber,
                    ShippingAddress = order.ShippingAddress,
                    TotalAmount = order.TotalAmount,
                    OrderDate = order.OrderDate,
                    OrderStatus = order.OrderStatus,
                    PaymentMethod = order.PaymentMethod,
                    PaymentStatus = order.PaymentStatus,
                    ReturnReason = order.ReturnReason,
                    Items = finalItems,
                    VoucherCode = voucherCode,
                    DiscountInfo = discountInfo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "LỖI C# TẠI API: " + ex.Message);
            }
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
        // ==========================================
        // 1. DTO NHẬN DỮ LIỆU TỪ CLIENT
        // ==========================================
        public class ReturnRequestDto { public string Reason { get; set; } }
        public class ProcessReturnDto { public bool IsApproved { get; set; } }

        // ==========================================
        // 2. API CHO MOBILE APP: KHÁCH YÊU CẦU TRẢ HÀNG
        // ==========================================
        [HttpPut("user-request-return/{id}")]
        public async Task<IActionResult> UserRequestReturn(int id, [FromBody] ReturnRequestDto request)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

            // Chỉ cho phép trả khi trạng thái là Completed (Đã nhận hàng)
            if (order.OrderStatus != "Completed")
            {
                return BadRequest(new { message = "Chỉ đơn hàng đã giao thành công mới được yêu cầu hoàn trả." });
            }

            order.OrderStatus = "ReturnRequested"; // Đổi trạng thái: Đang yêu cầu trả
            order.ReturnReason = request.Reason;   // Ghi lại lý do khách nhập

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã gửi yêu cầu hoàn trả, vui lòng chờ Admin duyệt!" });
        }

        // ==========================================
        // 3. API CHO ADMIN WEB: DUYỆT HOẶC TỪ CHỐI
        // ==========================================
        [HttpPut("admin-process-return/{id}")]
        public async Task<IActionResult> AdminProcessReturn(int id, [FromBody] ProcessReturnDto request)
        {
            // Lấy đơn hàng kèm theo chi tiết sản phẩm và Voucher để phục hồi
            var order = await _context.Orders
                .Include(o => o.Voucher)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });
            if (order.OrderStatus != "ReturnRequested") return BadRequest(new { message = "Đơn hàng này không có yêu cầu hoàn trả." });

            if (request.IsApproved)
            {
                // NẾU ADMIN ĐỒNG Ý
                order.OrderStatus = "Returned";
                order.PaymentStatus = "Refunded"; // Đánh dấu là đã hoàn tiền

                // A. Lấy danh sách sản phẩm trong đơn để cộng lại kho
                var orderItems = await _context.OrderDetails.Where(od => od.OrderId == id).ToListAsync();
                foreach (var item in orderItems)
                {
                    // Trả lại kho tổng
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null) product.StockQuantity += item.Quantity;

                    // Trả lại kho Size (nếu có)
                    if (!string.IsNullOrEmpty(item.SelectedSize))
                    {
                        var sizeStock = await _context.ProductSizes
                            .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.SizeName == item.SelectedSize);
                        if (sizeStock != null) sizeStock.StockQuantity += item.Quantity;
                    }
                }

                // B. Trả lại lượt sử dụng Voucher (Nếu có)
                if (order.VoucherId.HasValue && order.Voucher != null)
                {
                    order.Voucher.UsageLimit += 1;
                }
            }
            else
            {
                // NẾU ADMIN TỪ CHỐI
                order.OrderStatus = "ReturnRejected";
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = request.IsApproved ? "Đã duyệt trả hàng & Phục hồi tồn kho thành công!" : "Đã từ chối yêu cầu trả hàng." });
        }
    }
}