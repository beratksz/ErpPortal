namespace ErpPortal.Web.Models
{
    public class SelectWorkCenterViewModel
    {
        public string Username { get; set; } = string.Empty;
        public List<ErpPortal.Domain.Entities.WorkCenter> WorkCenters { get; set; } = new();
    }
} 