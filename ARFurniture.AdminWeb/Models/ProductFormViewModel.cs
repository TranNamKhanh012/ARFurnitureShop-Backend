using System.Collections.Generic;

namespace ARFurniture.AdminWeb.Models
{
    public class ProductFormViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int Discount { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string? Sizes { get; set; } // Phải giống hệt tên biến bên DTO của API

        // Danh sách danh mục để đổ vào thẻ <select> (Dropdown)
        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}