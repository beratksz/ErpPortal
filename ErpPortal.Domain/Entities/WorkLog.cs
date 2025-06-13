using System;

namespace ErpPortal.Domain.Entities
{
    public class WorkLog
    {
        public int Id { get; set; }
        public string OrderNo { get; set; } = string.Empty; // Shop Order reference
        public int OperationNo { get; set; } // Operation reference  
        public int UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Status { get; set; } = string.Empty; // ACTIVE, PAUSED, COMPLETED

        // Navigation Properties - sadece User ile direkt ilişki
        public virtual User? User { get; set; }
        // ShopOrderOperation navigation kaldırıldı - composite FK sorununu önlemek için
    }
} 