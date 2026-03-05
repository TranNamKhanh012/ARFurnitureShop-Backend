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
        public int CategoryId { get; set; }

        // Móc nối trực tiếp sản phẩm này với đối tượng Category ở trên
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
    }
}