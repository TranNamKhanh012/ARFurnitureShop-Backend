using System;
using System.Collections.Generic;

namespace ARFurniture.AdminWeb.Models
{
    // Class chứa thông tin từng món đồ
    public class OrderDetailItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public string SelectedSize { get; set; }
    }

    // Class chứa toàn bộ thông tin đơn hàng
    public class OrderDetailViewModel
    {
        public int Id { get; set; }
        public string ReceiverName { get; set; }
        public string PhoneNumber { get; set; }
        public string ShippingAddress { get; set; }
        public double TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public List<OrderDetailItemViewModel> Items { get; set; }
    }
}