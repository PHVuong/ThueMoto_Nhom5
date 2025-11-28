using System;
using System.ComponentModel.DataAnnotations;

namespace SmartMotoRental.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Nếu sau này có đăng nhập người dùng:
        public string? UserId { get; set; }
    }
}
