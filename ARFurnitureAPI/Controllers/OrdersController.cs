using Microsoft.AspNetCore.Mvc;
using ARFurnitureAPI.Data;
using ARFurnitureAPI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
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
            await _context.SaveChangesAsync();

            foreach (var item in request.Items)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = newOrder.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    SelectedSize = item.SelectedSize
                };
                _context.OrderDetails.Add(orderDetail);

                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    // SỬA LỖI ĐỎ: Bọc ?? 0 an toàn
                    product.StockQuantity = (product.StockQuantity ?? 0) - item.Quantity;
                    if (product.StockQuantity < 0) product.StockQuantity = 0;
                }

                if (!string.IsNullOrEmpty(item.SelectedSize))
                {
                    var sizeStock = await _context.ProductSizes
                        .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.SizeName == item.SelectedSize);

                    if (sizeStock != null)
                    {
                        sizeStock.StockQuantity -= item.Quantity;
                        if (sizeStock.StockQuantity < 0) sizeStock.StockQuantity = 0;
                    }
                }
            }

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
                    voucher.UsageLimit -= 1;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Đặt hàng thành công!", OrderId = newOrder.Id });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new {
                    o.Id,
                    OrderDate = o.OrderDate ?? DateTime.Now,
                    TotalAmount = o.TotalAmount ?? 0,
                    OrderStatus = o.OrderStatus ?? "Pending",
                    PaymentMethod = o.PaymentMethod ?? "COD",
                    PaymentStatus = o.PaymentStatus ?? "Unpaid"
                })
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                return NotFound(new { message = "Bạn chưa có đơn hàng nào." });
            }

            return Ok(orders);
        }

        public class UpdateStatusDto { public string Status { get; set; } }

        [HttpGet("admin-list")]
        public async Task<IActionResult> GetAdminOrders()
        {
            try
            {
                var orders = await _context.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new {
                        Id = o.Id,
                        ReceiverName = o.ReceiverName ?? "Khách hàng",
                        PhoneNumber = o.PhoneNumber ?? "N/A",
                        ShippingAddress = o.ShippingAddress ?? "N/A",
                        TotalAmount = o.TotalAmount ?? 0,
                        OrderDate = o.OrderDate ?? DateTime.Now,
                        OrderStatus = o.OrderStatus ?? "Pending",
                        PaymentMethod = o.PaymentMethod ?? "COD"
                    }).ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "LỖI TẠI API ADMIN-LIST: " + ex.Message);
            }
        }

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

        [HttpGet("admin-get/{id}")]
        public async Task<IActionResult> GetAdminOrderDetail(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Where(o => o.Id == id)
                    .Select(o => new {
                        Id = o.Id,
                        ReceiverName = o.ReceiverName ?? "",
                        PhoneNumber = o.PhoneNumber ?? "",
                        ShippingAddress = o.ShippingAddress ?? "",
                        TotalAmount = o.TotalAmount ?? 0,
                        OrderDate = o.OrderDate ?? DateTime.Now,
                        OrderStatus = o.OrderStatus ?? "Pending",
                        PaymentMethod = o.PaymentMethod ?? "",
                        PaymentStatus = o.PaymentStatus ?? "",
                        ReturnReason = o.ReturnReason ?? "",
                        VoucherId = (int?)o.VoucherId
                    })
                    .FirstOrDefaultAsync();

                if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng!" });

                string voucherCode = null;
                string discountInfo = null;

                if (order.VoucherId != null)
                {
                    var voucher = await _context.Vouchers
                        .Where(v => v.Id == order.VoucherId)
                        .Select(v => new {
                            v.Code,
                            v.DiscountType,
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

                var itemsList = await _context.OrderDetails
                    .Where(od => od.OrderId == id)
                    .Select(od => new {
                        ProductId = (int?)od.ProductId,
                        Quantity = od.Quantity ?? 0,
                        UnitPrice = od.UnitPrice ?? 0,
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
                            pName = product.Name ?? "Chưa có tên";
                            pImg = product.ImageUrl ?? "";
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
                return StatusCode(500, "LỖI API CHI TIẾT: " + ex.Message);
            }
        }

        [HttpPut("user-confirm/{id}")]
        public async Task<IActionResult> UserConfirmOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

            if (!order.OrderStatus.Equals("Shipping", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Chỉ có thể xác nhận khi đơn hàng đang được giao." });
            }

            order.OrderStatus = "Completed";
            if (order.PaymentMethod == "COD") order.PaymentStatus = "Paid";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xác nhận nhận hàng thành công!" });
        }

        public class ReturnRequestDto { public string Reason { get; set; } }
        public class ProcessReturnDto { public bool IsApproved { get; set; } }

        [HttpPut("user-request-return/{id}")]
        public async Task<IActionResult> UserRequestReturn(int id, [FromBody] ReturnRequestDto request)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

            if (order.OrderStatus != "Completed")
                return BadRequest(new { message = "Chỉ đơn hàng đã giao thành công mới được yêu cầu hoàn trả." });

            order.OrderStatus = "ReturnRequested";
            order.ReturnReason = request.Reason;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã gửi yêu cầu hoàn trả, vui lòng chờ Admin duyệt!" });
        }

        [HttpPut("admin-process-return/{id}")]
        public async Task<IActionResult> AdminProcessReturn(int id, [FromBody] ProcessReturnDto request)
        {
            var order = await _context.Orders
                .Include(o => o.Voucher)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });
            if (order.OrderStatus != "ReturnRequested") return BadRequest(new { message = "Đơn hàng này không có yêu cầu hoàn trả." });

            if (request.IsApproved)
            {
                order.OrderStatus = "Returned";
                order.PaymentStatus = "Refunded";

                var orderItems = await _context.OrderDetails.Where(od => od.OrderId == id).ToListAsync();
                foreach (var item in orderItems)
                {
                    if (item.ProductId.HasValue)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId.Value);
                        // SỬA LỖI ĐỎ: Bọc ?? 0 an toàn
                        if (product != null) product.StockQuantity = (product.StockQuantity ?? 0) + (item.Quantity ?? 0);

                        if (!string.IsNullOrEmpty(item.SelectedSize))
                        {
                            var sizeStock = await _context.ProductSizes
                                .FirstOrDefaultAsync(s => s.ProductId == item.ProductId.Value && s.SizeName == item.SelectedSize);
                            if (sizeStock != null) sizeStock.StockQuantity += (item.Quantity ?? 0);
                        }
                    }
                }

                if (order.VoucherId.HasValue && order.Voucher != null)
                {
                    order.Voucher.UsageLimit += 1;
                }
            }
            else
            {
                order.OrderStatus = "ReturnRejected";
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = request.IsApproved ? "Đã duyệt trả hàng & Phục hồi tồn kho thành công!" : "Đã từ chối yêu cầu trả hàng." });
        }
    }
}