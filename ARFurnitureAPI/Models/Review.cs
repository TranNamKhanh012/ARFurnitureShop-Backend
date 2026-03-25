using System;
using System.ComponentModel.DataAnnotations;

namespace ARFurnitureAPI.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } // Lưu tên người đánh giá
        public int Rating { get; set; } // Số sao (1 đến 5)
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}