using System;
using Newtonsoft.Json;

namespace MercadoPago.Wrapper.Models.Subscriptions
{
    // ─── Plan ───

    public class SubscriptionPlanCreateRequest
    {
        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("auto_recurring")]
        public AutoRecurringRequest AutoRecurring { get; set; }

        [JsonProperty("back_url")]
        public string BackUrl { get; set; }
    }

    public class AutoRecurringRequest
    {
        [JsonProperty("frequency")]
        public int Frequency { get; set; }

        [JsonProperty("frequency_type")]
        public string FrequencyType { get; set; } = "months";

        [JsonProperty("transaction_amount")]
        public decimal TransactionAmount { get; set; }

        [JsonProperty("currency_id")]
        public string CurrencyId { get; set; } = "ARS";

        [JsonProperty("repetitions")]
        public int? Repetitions { get; set; }

        [JsonProperty("free_trial")]
        public FreeTrialRequest FreeTrial { get; set; }

        [JsonProperty("billing_day")]
        public int? BillingDay { get; set; }

        [JsonProperty("billing_day_proportional")]
        public bool? BillingDayProportional { get; set; }
    }

    public class FreeTrialRequest
    {
        [JsonProperty("frequency")]
        public int Frequency { get; set; }

        [JsonProperty("frequency_type")]
        public string FrequencyType { get; set; }
    }

    public class SubscriptionPlanResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("auto_recurring")]
        public AutoRecurringRequest AutoRecurring { get; set; }

        [JsonProperty("init_point")]
        public string InitPoint { get; set; }

        [JsonProperty("back_url")]
        public string BackUrl { get; set; }

        [JsonProperty("date_created")]
        public DateTime? DateCreated { get; set; }
    }

    // ─── Suscripción ───

    public class SubscriptionCreateRequest
    {
        [JsonProperty("preapproval_plan_id")]
        public string PlanId { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("external_reference")]
        public string ExternalReference { get; set; }

        [JsonProperty("payer_email")]
        public string PayerEmail { get; set; }

        [JsonProperty("card_token_id")]
        public string CardTokenId { get; set; }

        [JsonProperty("auto_recurring")]
        public AutoRecurringRequest AutoRecurring { get; set; }

        [JsonProperty("back_url")]
        public string BackUrl { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class SubscriptionUpdateRequest
    {
        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("auto_recurring")]
        public AutoRecurringRequest AutoRecurring { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("card_token_id")]
        public string CardTokenId { get; set; }

        [JsonProperty("back_url")]
        public string BackUrl { get; set; }
    }

    public class SubscriptionResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("preapproval_plan_id")]
        public string PlanId { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("external_reference")]
        public string ExternalReference { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("payer_id")]
        public long? PayerId { get; set; }

        [JsonProperty("auto_recurring")]
        public AutoRecurringRequest AutoRecurring { get; set; }

        [JsonProperty("init_point")]
        public string InitPoint { get; set; }

        [JsonProperty("back_url")]
        public string BackUrl { get; set; }

        [JsonProperty("date_created")]
        public DateTime? DateCreated { get; set; }

        [JsonProperty("last_modified")]
        public DateTime? LastModified { get; set; }
    }
}
