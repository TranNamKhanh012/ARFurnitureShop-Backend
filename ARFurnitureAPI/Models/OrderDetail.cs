using System.ComponentModel.DataAnnotations;

namespace ARFurnitureAPI.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? ProductId { get; set; }
        public int? Quantity { get; set; } // ĐÃ THÊM ?
        public double? UnitPrice { get; set; } // ĐÃ THÊM ?
        public string? SelectedSize { get; set; }

        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}