using System.Collections.Generic;

namespace SmartMotoRental.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }        // Tổng thành viên
        public int TotalBikes { get; set; }        // Tổng xe
        public int TotalOrders { get; set; }       // Tổng đơn hàng
        public decimal TotalRevenue { get; set; }  // Tổng doanh thu

        // Dữ liệu vẽ biểu đồ
        public List<string> ChartLabels { get; set; }
        public List<decimal> ChartData { get; set; }
    }
}