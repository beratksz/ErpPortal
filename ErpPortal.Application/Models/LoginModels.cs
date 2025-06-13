using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ErpPortal.Application.Models
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? WorkCenterCode { get; set; }
    }

    public class LoginResponse
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string WorkCenterCode { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }

    public class LoginViewModel
    {
        public List<SelectListItem> WorkCenters { get; set; } = new();
        public List<SelectListItem> Users { get; set; } = new();
    }
} 