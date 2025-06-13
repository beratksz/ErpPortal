using System.Collections.Generic;

namespace ErpPortal.Domain.Entities
{
    public class WorkCenter
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        
        public ICollection<UserWorkCenter> UserWorkCenters { get; set; } = new List<UserWorkCenter>();
        public virtual ICollection<ShopOrderOperation> Operations { get; set; } = new List<ShopOrderOperation>();
    }
}
