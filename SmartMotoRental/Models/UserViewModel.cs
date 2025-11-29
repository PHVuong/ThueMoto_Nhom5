using System.ComponentModel.DataAnnotations;

namespace SmartMotoRental.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string UserName { get; set; } = "";

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = "";

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public string Role { get; set; } = "User";
    }
}
