namespace ARFurnitureAPI.Models
{
    public class DashboardDto
    {
        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }

        // Các trường này tạm thời bằng 0 chờ làm chức năng Thanh toán
        public int SoldProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal RevenueToday { get; set; }
    }
}