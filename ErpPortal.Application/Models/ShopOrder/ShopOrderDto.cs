using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ErpPortal.Application.Models.ShopOrder
{
    public class ShopOrderDto
    {
        [JsonPropertyName("OrderNo")]
        public string OrderNo { get; set; } = string.Empty;

        [JsonPropertyName("OperationNo")]
        public int OperationNo { get; set; }

        [JsonPropertyName("OperationDescription")]
        public string OperationDescription { get; set; } = string.Empty;

        [JsonPropertyName("WorkCenterNo")]
        public string WorkCenterNo { get; set; } = string.Empty;
        
        [JsonPropertyName("Contract")]
        public string Contract { get; set; } = string.Empty;
        
        [JsonPropertyName("ReleaseNo")]
        public string ReleaseNo { get; set; } = string.Empty;
        
        [JsonPropertyName("SequenceNo")]
        public string SequenceNo { get; set; } = string.Empty;
        
        [JsonPropertyName("OpSequenceNo")]
        public int OpSequenceNo { get; set; }
        
        [JsonPropertyName("EfficiencyFactor")]
        public decimal EfficiencyFactor { get; set; }
        
        [JsonPropertyName("MachRunFactor")]
        public decimal MachRunFactor { get; set; }
        
        [JsonPropertyName("MachSetupTime")]
        public decimal MachSetupTime { get; set; }
        
        [JsonPropertyName("MoveTime")]
        public decimal MoveTime { get; set; }
        
        [JsonPropertyName("QueueTime")]
        public decimal QueueTime { get; set; }
        
        [JsonPropertyName("LaborRunFactor")]
        public decimal LaborRunFactor { get; set; }
        
        [JsonPropertyName("LaborSetupTime")]
        public decimal LaborSetupTime { get; set; }
        
        [JsonPropertyName("RevisedQtyDue")]
        public decimal RevisedQtyDue { get; set; }
        
        [JsonPropertyName("OperStatusCode")]
        public string Status { get; set; } = string.Empty;
        
        [JsonPropertyName("PartNo")]
        public string PartNo { get; set; } = string.Empty;
        
        [JsonPropertyName("PartDescription")]
        public string PartDescription { get; set; } = string.Empty;
        
        [JsonPropertyName("OpStartDate")]
        public DateTime? OpStartDate { get; set; }

        [JsonPropertyName("OpFinishDate")]
        public DateTime? OpFinishDate { get; set; }

        [JsonPropertyName("QtyComplete")]
        public decimal QtyComplete { get; set; }

        [JsonPropertyName("QtyScrapped")]
        public decimal QtyScrapped { get; set; }

        [JsonPropertyName("NoteText")]
        public string? Notes { get; set; }

        [JsonPropertyName("@odata.etag")]
        public string? ETag { get; set; }
    }

    public class ShopOrderDetailsDto : ShopOrderDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerOrderNo { get; set; } = string.Empty;
        public new string Notes { get; set; } = string.Empty;
        public List<ShopOrderOperationDto> Operations { get; set; } = new List<ShopOrderOperationDto>();
    }

    public class ShopOrderOperationDto
    {
        public string OrderNo { get; set; } = string.Empty;
        public int OperationNo { get; set; }
        public string WorkCenterCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? AssignedTo { get; set; }
        public string OperationDescription { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal QuantityCompleted { get; set; }
        public decimal QuantityScrapped { get; set; }
        public string? Notes { get; set; }
        public string OperStatusCode { get; set; } = string.Empty;
        public decimal QtyComplete { get; set; }
        public decimal QtyScrapped { get; set; }
        public string? NoteText { get; set; }
        public string? ReportedBy { get; set; }
    }

    public class OperationStatusUpdateDto
    {
        public string Status { get; set; } = string.Empty;
        public string? AssignedTo { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Notes { get; set; }
        public decimal? QuantityCompleted { get; set; }
        public decimal? QuantityScrapped { get; set; }
    }
}