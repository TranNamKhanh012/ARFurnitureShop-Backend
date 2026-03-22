using System;
using System.Collections.Generic;

namespace ARFurniture.AdminWeb.Models
{
    public class DashboardViewModel
    {
        // 7 CHỈ SỐ THỐNG KÊ (Đã thêm đầy đủ)
        public int TotalCategories { get; set; }
        public int TotalProductsSold { get; set; }
        public int TotalStock { get; set; }
        public int TotalUsers { get; set; }

        public double TodayRevenue { get; set; }
        public double WeeklyRevenueTotal { get; set; }
        public double MonthlyRevenue { get; set; }

        public List<RecentOrderViewModel> RecentOrders { get; set; } = new List<RecentOrderViewModel>();
        public List<DailyRevenueViewModel> WeeklyRevenue { get; set; } = new List<DailyRevenueViewModel>();
    }

    public class RecentOrderViewModel
    {
        public int Id { get; set; }
        public string ReceiverName { get; set; }
        public double TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
    }

    public class DailyRevenueViewModel
    {
        public string Date { get; set; }
        public double Revenue { get; set; }
    }
}