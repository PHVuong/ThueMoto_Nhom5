using System.ComponentModel.DataAnnotations;

namespace SmartMotoRental.Models
{
    public class RentalOrder
    {
        [Key]
        public int Id { get; set; }
        
        // Khóa ngoại
        public int CustomerId { get; set; } 
        public int VehicleId { get; set; }  
        public int BranchStartId { get; set; }
        public int BranchEndId { get; set; }

        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } 

        // Thuộc tính Điều hướng
        public Customer Customer { get; set; } 
        public Vehicle Vehicle { get; set; }
        public Branch BranchStart { get; set; }
        public Branch BranchEnd { get; set; }
    }
}
// Lưu ý: Các Entity Customer.cs, Vehicle.cs, Branch.cs cũng cần được tạo tương tự.