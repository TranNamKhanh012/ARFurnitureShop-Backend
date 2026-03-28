using System;

namespace ARFurniture.AdminWeb.Models
{
    public class ReviewViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string FullName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}