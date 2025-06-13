using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ErpPortal.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur")]
        [Display(Name = "Şifre")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "İş Merkezi")]
        public string? WorkCenterCode { get; set; }

        public string? ReturnUrl { get; set; }
        public List<SelectListItem> WorkCenters { get; set; } = new();
    }
} 