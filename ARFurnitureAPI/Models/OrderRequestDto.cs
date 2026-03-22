using System.Collections.Generic;

namespace ARFurnitureAPI.Models
{
    public class OrderRequestDto
    {
        public int UserId { get; set; }
        public double TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string ReceiverName { get; set; }


        // Danh sách các mặt hàng trong Giỏ
        public List<OrderItemDto> Items { get; set; }
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public string? SelectedSize { get; set; }
    }
}