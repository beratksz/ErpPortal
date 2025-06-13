using System;
using System.Collections.Generic;

namespace ErpPortal.Domain.Entities
{
    public class ShopOrderOperation
    {
        // Primary Key for Entity Framework
        public int Id { get; set; }

        // Composite Key (Veritabanı için)
        public string OrderNo { get; set; } = string.Empty;
        public int OperationNo { get; set; }

        // Required fields for PATCH
        public string ReleaseNo { get; set; } = default!;
        public string SequenceNo { get; set; } = default!;
        public int OpSequenceNo { get; set; }
        public decimal EfficiencyFactor { get; set; }
        public decimal MachRunFactor { get; set; }
        public decimal MachSetupTime { get; set; }
        public decimal MoveTime { get; set; }
        public decimal QueueTime { get; set; }
        public decimal LaborRunFactor { get; set; }
        public decimal LaborSetupTime { get; set; }
        public string OperationDescription { get; set; } = default!;
        public string WorkCenterNo { get; set; } = default!;
        public string PartNo { get; set; } = default!;
        public string PartDescription { get; set; } = default!;
        public string OperStatusCode { get; set; } = default!;
        public decimal RevisedQtyDue { get; set; }
        public decimal QtyComplete { get; set; }
        public decimal QtyScrapped { get; set; }
        public DateTime OpStartDate { get; set; }
        public DateTime OpFinishDate { get; set; }
        public string SchedDirection { get; set; } = "BackwardsScheduling";
        public string RunTimeCode { get; set; } = "HoursUnit";
        public string Contract { get; set; } = default!;
        public string ParallelOperation { get; set; } = "NotParallel";
        public string OperationSchedStatus { get; set; } = "InfiniteScheduled";
        public bool OutsideOpComplete { get; set; }
        public string OutsideOpBackflush { get; set; } = "Disallowed";

        // Additional fields for local tracking
        public string Code { get; set; } = string.Empty; // For WorkCenter Code reference
        public string WorkCenterCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? AssignedTo { get; set; }
        public string? ReportedBy { get; set; } // İşlemi yapan kullanıcı
        public string Description { get; set; } = string.Empty;
        public decimal QuantityCompleted { get; set; }
        public decimal QuantityScrapped { get; set; }
        public string? Notes { get; set; }
        public bool IsSyncPending { get; set; }
        public bool IsAwaitingQuality { get; set; } // Kalite onayı bekliyor mu?
        public DateTime? LastSyncTime { get; set; }
        public string? LastSyncError { get; set; } // Senkronizasyon hata mesajı
        public string? ETag { get; set; }
        public DateTime? LastInterruptionTime { get; set; }
        public string? InterruptionReason { get; set; }
        public TimeSpan TotalInterruptionDuration { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualFinishDate { get; set; }

        // Navigation Properties
        public virtual ShopOrder? ShopOrder { get; set; }
        public virtual WorkCenter? WorkCenter { get; set; }
        // WorkLogs navigation removed to avoid composite FK issues

        public string GetEtag() => string.IsNullOrEmpty(ETag) ? "*" : ETag;
    }
}
