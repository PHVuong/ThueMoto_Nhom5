using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartMotoRental.Models
{
    public class RentalOrder
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int BikeId { get; set; }
        
        [ForeignKey(nameof(BikeId))]
        public Motorbike? Motorbike { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa điểm nhận xe")]
        public string PickupLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa điểm trả xe")]
        public string ReturnLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngày nhận xe")]
        public DateTime PickupDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày trả xe")]
        public DateTime ReturnDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên người thuê")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        public string? Note { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? EstimatedPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
