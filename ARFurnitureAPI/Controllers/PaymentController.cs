using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using ARFurnitureAPI.Utils;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    // CÁC THÔNG SỐ TEST CỦA VNPAY SANDBOX
    private string vnp_TmnCode = "FV1ISYBO";
    private string vnp_HashSecret = "VGXDWXWGQ9Y5BKJF35HPMJEJSXLTSZKN";
    private string vnp_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

    [HttpGet("get-vnpay-url")]
    public IActionResult GetVnpayUrl([FromQuery] double amount)
    {
        var vnpay = new VnPayLibrary(); // Sử dụng lớp tiện ích trong Utils

        vnpay.AddRequestData("vnp_Version", "2.1.0");
        vnpay.AddRequestData("vnp_Command", "pay");
        vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode); // ĐÃ SỬA: Dùng biến ở trên
        vnpay.AddRequestData("vnp_Amount", (amount * 100).ToString()); // VNPAY nhân 100
                                                                       // Lấy giờ Việt Nam chuẩn bất kể Server đặt ở đâu
        TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        DateTime vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

        vnpay.AddRequestData("vnp_CreateDate", vietnamNow.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", "VND");
        vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
        vnpay.AddRequestData("vnp_Locale", "vn");
        vnpay.AddRequestData("vnp_OrderInfo", "Thanh_toan_don_hang_Accessories");
        vnpay.AddRequestData("vnp_OrderType", "other");
        vnpay.AddRequestData("vnp_ReturnUrl", "http://trannamkhanh-001-site1.jtempurl.com/api/Payment/PaymentCallback");
        vnpay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString());

        // ĐÃ SỬA: Dùng biến vnp_HashSecret ở trên để băm dữ liệu
        string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

        return Content(paymentUrl, "text/plain");
    }

    [HttpGet("PaymentCallback")]
    public IActionResult PaymentCallback()
    {
        // Khi thanh toán xong, VNPAY sẽ trả kết quả về đây qua URL
        // Bạn có thể lấy các tham số như vnp_ResponseCode để kiểm tra thành công hay thất bại
        var query = Request.Query;
        if (query["vnp_ResponseCode"] == "00")
        {
            return Ok("Thanh toán thành công! Bạn có thể đóng trình duyệt.");
        }
        return Ok("Giao dịch không thành công.");
    }
}