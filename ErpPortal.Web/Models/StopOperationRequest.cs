using System.ComponentModel.DataAnnotations;

namespace ErpPortal.Web.Models
{
    public class StopOperationRequest
    {
        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
} 