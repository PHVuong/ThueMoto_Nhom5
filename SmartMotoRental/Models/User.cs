using System;
using System.ComponentModel.DataAnnotations;

namespace SmartMotoRental.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }              // Khóa chính

        [Required, MaxLength(100)]
        public string UserName { get; set; } = "";

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = "";

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required, MaxLength(50)]
        public string Role { get; set; } = "User"; // "Admin" hoặc "User"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
