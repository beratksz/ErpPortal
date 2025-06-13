using System.ComponentModel.DataAnnotations;

namespace ErpPortal.Web.Models
{
    public class ReportCompletionViewModel
    {
        [Required]
        public string OrderNo { get; set; } = default!;

        [Required]
        public int OperationNo { get; set; }

        [Required(ErrorMessage = "Tamamlanan miktar girilmelidir.")]
        [Range(0, int.MaxValue, ErrorMessage = "Tamamlanan miktar negatif olamaz.")]
        [Display(Name = "Tamamlanan Miktar")]
        public int QuantityCompleted { get; set; }

        [Required(ErrorMessage = "Hurda miktarı girilmelidir (0 olabilir).")]
        [Range(0, int.MaxValue, ErrorMessage = "Hurda miktarı negatif olamaz.")]
        [Display(Name = "Hurda Miktarı")]
        public int QuantityScrapped { get; set; }
    }
}
