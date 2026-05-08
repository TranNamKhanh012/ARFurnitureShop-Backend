using System;

namespace ARFurniture.AdminWeb.Models
{
    public class VoucherViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string DiscountType { get; set; }
        public double DiscountValue { get; set; }
        public double MinOrderValue { get; set; }
        public double? MaxDiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UsageLimit { get; set; }
        public bool IsActive { get; set; }
    }
}