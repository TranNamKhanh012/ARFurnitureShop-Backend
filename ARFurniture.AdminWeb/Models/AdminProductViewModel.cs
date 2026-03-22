namespace ARFurniture.AdminWeb.Models
{
    public class AdminProductViewModel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public double OriginalPrice { get; set; }
        public double SellingPrice { get; set; }
        public int StockQuantity { get; set; }
        public int SoldQuantity { get; set; }
    }
}