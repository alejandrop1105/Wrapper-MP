using System.Collections.Generic;
using Newtonsoft.Json;

namespace MercadoPago.Wrapper.Models.Disbursements
{
    /// <summary>Desembolso de un pago avanzado.</summary>
    public class DisbursementResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("external_reference")]
        public string ExternalReference { get; set; }

        [JsonProperty("collector_id")]
        public long CollectorId { get; set; }

        [JsonProperty("application_fee")]
        public decimal? ApplicationFee { get; set; }

        [JsonProperty("money_release_days")]
        public int? MoneyReleaseDays { get; set; }

        [JsonProperty("money_release_status")]
        public string MoneyReleaseStatus { get; set; }

        [JsonProperty("money_release_date")]
        public string MoneyReleaseDate { get; set; }
    }

    /// <summary>Reembolso de un desembolso.</summary>
    public class DisbursementRefundResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("payment_id")]
        public long PaymentId { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("date_created")]
        public string DateCreated { get; set; }
    }

    /// <summary>Request para reembolsar un desembolso.</summary>
    public class DisbursementRefundRequest
    {
        [JsonProperty("amount")]
        public decimal? Amount { get; set; }
    }
}
