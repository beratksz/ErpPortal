using System.ComponentModel.DataAnnotations;

namespace ErpPortal.Web.Models;

public class UserFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Kullanıcı adı zorunlu")] 
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ad Soyad zorunlu")]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunlu")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name="Admin?")]
    public bool IsAdmin { get; set; }

    [Display(Name="Aktif mi?")]
    public bool IsActive { get; set; } = true;
} 