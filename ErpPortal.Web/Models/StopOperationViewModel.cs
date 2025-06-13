using System.ComponentModel.DataAnnotations;

namespace ErpPortal.Web.Models
{
    public class StopOperationViewModel
    {
        [Required]
        public string OrderNo { get; set; } = default!;

        [Required]
        public int OperationNo { get; set; }

        [Display(Name = "Durdurma Sebebi")]
        [MaxLength(500)]
        public string? Reason { get; set; } // Sebep opsiyonel olabilir
    }
}
