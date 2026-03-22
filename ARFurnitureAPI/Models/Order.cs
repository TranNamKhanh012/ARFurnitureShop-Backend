using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ARFurnitureAPI.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public double TotalAmount { get; set; }

        // Trạng thái đơn hàng: "Pending", "Processing", "Shipped"
        public string OrderStatus { get; set; } = "Pending";

        // Phương thức thanh toán: "COD", "VNPAY", "MOCK"
        public string PaymentMethod { get; set; }

        // Trạng thái thanh toán: "Unpaid", "Paid"
        public string PaymentStatus { get; set; } = "Unpaid";

        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string ReceiverName { get; set; }

        // Liên kết 1 Đơn hàng -> Nhiều Chi tiết
        public List<OrderDetail> OrderDetails { get; set; }
    }
}