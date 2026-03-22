namespace ARFurnitureAPI.Models
{
    public class AdminProductDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public double OriginalPrice { get; set; } // Giá gốc
        public double SellingPrice { get; set; }  // Giá bán (Đã trừ discount)
        public int StockQuantity { get; set; }    // Tồn kho
        public int SoldQuantity { get; set; }     // Đã bán
    }
}