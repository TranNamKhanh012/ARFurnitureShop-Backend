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
        try
        {
            var vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);

            long vnpAmount = (long)(amount * 100);
            vnpay.AddRequestData("vnp_Amount", vnpAmount.ToString());

            DateTime vietnamNow = DateTime.UtcNow.AddHours(7);

            vnpay.AddRequestData("vnp_CreateDate", vietnamNow.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh_toan_don_hang_Accessories");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", "http://trannamkhanh-001-site1.jtempurl.com/api/Payment/PaymentCallback");
            vnpay.AddRequestData("vnp_TxnRef", vietnamNow.Ticks.ToString());

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

            return Content(paymentUrl, "text/plain");
        }
        catch (Exception ex)
        {
            // Nếu có lỗi, nó sẽ không báo 500 nữa mà sẽ in rõ dòng chữ này ra!
            return StatusCode(500, "Lỗi chi tiết: " + ex.Message + " | Dòng lỗi: " + ex.StackTrace);
        }
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