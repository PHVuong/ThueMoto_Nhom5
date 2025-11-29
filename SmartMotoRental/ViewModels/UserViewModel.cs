using System.ComponentModel.DataAnnotations;

namespace SmartMotoRental.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty; // String để dùng trong URL và form
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

