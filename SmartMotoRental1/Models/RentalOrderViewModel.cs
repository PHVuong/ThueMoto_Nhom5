namespace SmartMotoRental.Models
{
    public class RentalOrderViewModel
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string VehicleName { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string BranchInfo { get; set; } 
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public bool CanBeRented { get; set; } // Hiển thị nút "Nhận xe"
        public bool CanBeCancelled { get; set; } // Hiển thị nút "Hủy"
    }
}