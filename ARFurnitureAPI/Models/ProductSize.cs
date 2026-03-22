using System.ComponentModel.DataAnnotations;

namespace ARFurnitureAPI.Models
{
    public class ProductSize
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }

        // Tên size: "39", "40", "XL", "FreeSize"...
        public string SizeName { get; set; }

        // Số lượng tồn kho của riêng size này
        public int StockQuantity { get; set; }
    }
}