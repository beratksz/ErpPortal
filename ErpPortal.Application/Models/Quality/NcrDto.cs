namespace ErpPortal.Application.Models.Quality
{
    using System;
    using System.Text.Json.Serialization;

    public class NcrDto
    {
        [JsonPropertyName("NcrNo")]
        public string NcrNo { get; set; } = string.Empty;

        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        [JsonPropertyName("Objstate")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("Company")]
        public string Company { get; set; } = string.Empty;

        [JsonPropertyName("Cf_Is_Emri_No")]
        public string? OrderNo { get; set; }

        [JsonPropertyName("Cf_Operation_No")]
        public string? OperationNo { get; set; }

        [JsonPropertyName("Cf_Hurda_Mik")]
        public decimal? ScrapQty { get; set; }
    }
} 