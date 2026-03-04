using System.Collections.Generic;
using Newtonsoft.Json;

namespace MercadoPago.Wrapper.Models.Chargebacks
{
    /// <summary>Contracargo (chargeback).</summary>
    public class ChargebackResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("payments")]
        public List<long> Payments { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("coverage_applied")]
        public bool? CoverageApplied { get; set; }

        [JsonProperty("coverage_eligible")]
        public bool? CoverageEligible { get; set; }

        [JsonProperty("documentation_required")]
        public bool? DocumentationRequired { get; set; }

        [JsonProperty("documentation_status")]
        public string DocumentationStatus { get; set; }

        [JsonProperty("documentation")]
        public List<ChargebackDocument> Documentation { get; set; }

        [JsonProperty("date_documentation_deadline")]
        public string DateDocumentationDeadline { get; set; }

        [JsonProperty("date_created")]
        public string DateCreated { get; set; }

        [JsonProperty("date_last_updated")]
        public string DateLastUpdated { get; set; }

        [JsonProperty("live_mode")]
        public bool LiveMode { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_detail")]
        public string StatusDetail { get; set; }
    }

    public class ChargebackDocument
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
