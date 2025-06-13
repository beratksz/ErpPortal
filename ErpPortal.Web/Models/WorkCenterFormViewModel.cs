using System.ComponentModel.DataAnnotations;

namespace ErpPortal.Web.Models;

public class WorkCenterFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }

    [Display(Name="Aktif mi?")]
    public bool IsActive { get; set; } = true;
} 