using System.ComponentModel.DataAnnotations.Schema;

namespace ARFurnitureAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } = string.Empty;
        public string? ModelUrl { get; set; } = string.Empty;
        public double Price { get; set; }

        // BÍ QUYẾT Ở ĐÂY: Thêm cột CategoryId làm Khóa ngoại (Foreign Key)
        public int Discount { get; set; } // Phần trăm giảm giá (VD: 20 nghĩa là giảm 20%)
        public double Rating { get; set; } // Số sao đánh giá (VD: 4.5, 5.0)
        public int CategoryId { get; set; }

        // Móc nối trực tiếp sản phẩm này với đối tượng Category ở trên
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        public string Description { get; set; } = "";

        // ==========================================
        // THÊM 2 CỘT NÀY VÀO ĐỂ HẾT LỖI ĐỎ VÀ HIỂN THỊ LÊN APP:
        // ==========================================
        public int ReviewCount { get; set; } = 0; // Số lượt người đã đánh giá (VD: 4)
        public DateTime DateAdded { get; set; } = DateTime.Now; // Ngày thêm sp (Để xếp Mới/Cũ)
    }
}