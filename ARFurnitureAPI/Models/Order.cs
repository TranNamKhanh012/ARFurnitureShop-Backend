using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARFurnitureAPI.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public int? UserId { get; set; }
        public DateTime? OrderDate { get; set; } // ĐÃ THÊM ?
        public double? TotalAmount { get; set; } // ĐÃ THÊM ?

        public string OrderStatus { get; set; } = "Pending";
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; } = "Unpaid";

        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string ReceiverName { get; set; }
        public int? VoucherId { get; set; }
        public string? ReturnReason { get; set; }

        [ForeignKey("VoucherId")]
        public virtual Voucher? Voucher { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }
    }
}