namespace ARFurnitureAPI.Models
{
    public class AdminProductFormDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int Discount { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }

        // [MỚI] Ô này để hứng chuỗi các size người quản trị nhập (vd: "39, 40, 41, XL")
        public string? Sizes { get; set; }
    }
}