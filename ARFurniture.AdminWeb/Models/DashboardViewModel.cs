namespace ARFurniture.AdminWeb.Models
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; } = 15;
        public int TotalOrders { get; set; } = 4;
        public int TotalUsers { get; set; } = 6;
        public decimal RevenueToday { get; set; } = 0;
        // ... (Bạn có thể thêm các trường khác theo ý muốn)
    }
}