using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ErpPortal.Web.Models
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class UserViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public List<WorkCenterViewModel> WorkCenters { get; set; } = new();
    }

    public class WorkCenterViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class CreateUserRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
        
        public bool IsAdmin { get; set; }
    }

    public class UpdateUserRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        public bool IsAdmin { get; set; }
        
        // Password is optional during update
        public string? Password { get; set; }
    }
} 