using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using ARFurnitureAPI.Data;
using ARFurnitureAPI.Models;

namespace ARFurnitureAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VouchersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VouchersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("apply")]
        public IActionResult ApplyVoucher([FromBody] ApplyVoucherRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest(new { message = "Vui lòng nhập mã giảm giá." });
            }

            // Tìm mã trong DB (Không phân biệt chữ hoa/thường)
            var voucher = _context.Vouchers.FirstOrDefault(v => v.Code.ToUpper() == request.Code.ToUpper());

            // 1. Kiểm tra mã có tồn tại không?
            if (voucher == null)
            {
                return NotFound(new { message = "Mã giảm giá không tồn tại hoặc viết sai." });
            }

            // 2. Kiểm tra trạng thái Kích hoạt
            if (!voucher.IsActive)
            {
                return BadRequest(new { message = "Mã giảm giá này đã bị vô hiệu hóa." });
            }

            // 3. Kiểm tra thời hạn sử dụng
            var now = DateTime.Now;
            if (now < voucher.StartDate)
            {
                return BadRequest(new { message = $"Mã giảm giá chỉ bắt đầu có hiệu lực từ ngày {voucher.StartDate:dd/MM/yyyy}." });
            }
            if (now > voucher.EndDate)
            {
                return BadRequest(new { message = "Mã giảm giá đã hết hạn sử dụng." });
            }

            // 4. Kiểm tra số lượng
            if (voucher.UsageLimit <= 0)
            {
                return BadRequest(new { message = "Rất tiếc, mã giảm giá này đã hết lượt sử dụng." });
            }

            // 5. Kiểm tra giá trị đơn hàng tối thiểu
            if (request.OrderTotal < voucher.MinOrderValue)
            {
                return BadRequest(new { message = $"Đơn hàng phải đạt tối thiểu {voucher.MinOrderValue:N0}đ để sử dụng mã này." });
            }

            // ==========================================
            // THUẬT TOÁN TÍNH TOÁN SỐ TIỀN ĐƯỢC GIẢM
            // ==========================================
            double discountAmount = 0;

            if (voucher.DiscountType == "FixedAmount")
            {
                // Kiểu 1: Giảm thẳng tiền mặt (VD: Trừ 20.000đ)
                discountAmount = voucher.DiscountValue;
            }
            else if (voucher.DiscountType == "Percentage")
            {
                // Kiểu 2: Giảm theo % (VD: Giảm 10%)
                discountAmount = request.OrderTotal * (voucher.DiscountValue / 100.0);

                // Áp dụng trần giảm tối đa nếu có (Ví dụ: Giảm 10% nhưng tối đa chỉ trừ 50.000đ)
                if (voucher.MaxDiscountAmount.HasValue && discountAmount > voucher.MaxDiscountAmount.Value)
                {
                    discountAmount = voucher.MaxDiscountAmount.Value;
                }
            }

            // Chốt chặn an toàn: Tiền giảm không được vượt quá tổng tiền đơn hàng (tránh số âm)
            if (discountAmount > request.OrderTotal)
            {
                discountAmount = request.OrderTotal;
            }

            // Trả kết quả thành công về cho Mobile
            return Ok(new
            {
                message = "Áp dụng mã giảm giá thành công!",
                discountAmount = discountAmount, // App Mobile sẽ lấy số này để trừ tiền
                voucherId = voucher.Id,
                code = voucher.Code
            });
        }
    }

    // Class hứng dữ liệu từ Mobile gửi lên
    public class ApplyVoucherRequest
    {
        public string Code { get; set; }
        public double OrderTotal { get; set; }
    }
}