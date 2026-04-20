using System;
using System.ComponentModel.DataAnnotations;

namespace ARFurnitureAPI.Models
{
    public class Voucher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } // Ví dụ: FREESHIP, GIAM10K, HSSV2026

        public string Description { get; set; } // Ví dụ: "Giảm 10k cho đơn từ 100k"

        // Loại giảm giá: "FixedAmount" (Giảm tiền mặt) hoặc "Percentage" (Giảm theo %)
        public string DiscountType { get; set; }

        // Giá trị giảm (Ví dụ: 10000 hoặc 10)
        public double DiscountValue { get; set; }

        // Giá trị đơn hàng tối thiểu mới được áp mã
        public double MinOrderValue { get; set; }

        // Số tiền giảm tối đa (Dành cho loại Percentage - Giảm 10% nhưng tối đa 50k)
        public double? MaxDiscountAmount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Số lượng mã có thể sử dụng (Ai nhanh tay thì được)
        public int UsageLimit { get; set; }

        // Trạng thái bật/tắt mã
        public bool IsActive { get; set; }
    }
}