using System;
using System.Collections.Generic;

namespace ErpPortal.Domain.Entities
{
    public class ShopOrder
    {
        public string OrderNo { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PartNo { get; set; }
        public string? PartDescription { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedFinishDate { get; set; }
        public decimal Quantity { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }

        public ICollection<ShopOrderOperation> Operations { get; set; } = new List<ShopOrderOperation>();
    }
} 