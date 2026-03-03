namespace ARFurnitureAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } = string.Empty;
        public string? ModelUrl { get; set; } = string.Empty;
        public double Price { get; set; }
    }
}