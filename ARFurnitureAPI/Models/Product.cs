using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARFurnitureAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } = string.Empty;
        public string? ModelUrl { get; set; } = string.Empty;
        public double? Price { get; set; } // ĐÃ THÊM ?

        public int? Discount { get; set; } // ĐÃ THÊM ?
        public double? Rating { get; set; } // ĐÃ THÊM ?
        public int? CategoryId { get; set; } // ĐÃ THÊM ?

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public string Description { get; set; } = "";
        public int? StockQuantity { get; set; } // ĐÃ THÊM ?

        [NotMapped]
        public string? Sizes { get; set; }

        public int? ReviewCount { get; set; } // ĐÃ THÊM ?
        public DateTime? DateAdded { get; set; } // ĐÃ THÊM ?
    }
}