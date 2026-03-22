using System;
using System.Collections.Generic;

namespace ARFurnitureAPI.Models
{
    public class DashboardDto
    {
        public int TotalCategories { get; set; } // Tổng danh mục
        public int TotalProductsSold { get; set; } // SP đã bán
        public int TotalStock { get; set; } // Hàng trong kho
        public int TotalUsers { get; set; } // Người dùng

        public double TodayRevenue { get; set; } // Doanh thu hôm nay
        public double WeeklyRevenueTotal { get; set; } // Doanh thu tuần
        public double MonthlyRevenue { get; set; } // Doanh thu tháng

        // (Giữ nguyên các List Biểu đồ và Đơn hàng gần đây)
        public List<RecentOrderDto> RecentOrders { get; set; } // Thay Dto bằng ViewModel nếu ở AdminWeb
        public List<DailyRevenueDto> WeeklyRevenue { get; set; }
    }

    public class RecentOrderDto
    {
        public int Id { get; set; }
        public string ReceiverName { get; set; }
        public double TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
    }

    // [THÊM MỚI] Gói dữ liệu từng ngày
    public class DailyRevenueDto
    {
        public string Date { get; set; }
        public double Revenue { get; set; }
    }
}