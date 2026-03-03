using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MercadoPago.Wrapper.Models.Preferences
{
    // ─── Request ───

    /// <summary>Solicitud para crear una preferencia de Checkout Pro.</summary>
    public class PreferenceCreateRequest
    {
        [JsonProperty("items")]
        public List<PreferenceItemRequest> Items { get; set; } = new List<PreferenceItemRequest>();

        [JsonProperty("payer")]
        public PreferencePayerRequest Payer { get; set; }

        [JsonProperty("back_urls")]
        public BackUrlsRequest BackUrls { get; set; }

        [JsonProperty("auto_return")]
        public string AutoReturn { get; set; }

        [JsonProperty("external_reference")]
        public string ExternalReference { get; set; }

        [JsonProperty("notification_url")]
        public string NotificationUrl { get; set; }

        [JsonProperty("expires")]
        public bool? Expires { get; set; }

        [JsonProperty("expiration_date_from")]
        public DateTime? ExpirationDateFrom { get; set; }

        [JsonProperty("expiration_date_to")]
        public DateTime? ExpirationDateTo { get; set; }

        [JsonProperty("payment_methods")]
        public PreferencePaymentMethodsRequest PaymentMethods { get; set; }

        [JsonProperty("binary_mode")]
        public bool? BinaryMode { get; set; }

        [JsonProperty("statement_descriptor")]
        public string StatementDescriptor { get; set; }

        [JsonProperty("marketplace_fee")]
        public decimal? MarketplaceFee { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, object> Metadata { get; set; }
    }

    public class PreferenceItemRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("category_id")]
        public string CategoryId { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("currency_id")]
        public string CurrencyId { get; set; }

        [JsonProperty("unit_price")]
        public decimal UnitPrice { get; set; }

        [JsonProperty("picture_url")]
        public string PictureUrl { get; set; }
    }

    public class PreferencePayerRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("surname")]
        public string Surname { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public Common.PhoneRequest Phone { get; set; }

        [JsonProperty("identification")]
        public Common.IdentificationRequest Identification { get; set; }

        [JsonProperty("address")]
        public Common.AddressRequest Address { get; set; }
    }

    public class BackUrlsRequest
    {
        [JsonProperty("success")]
        public string Success { get; set; }

        [JsonProperty("pending")]
        public string Pending { get; set; }

        [JsonProperty("failure")]
        public string Failure { get; set; }
    }

    public class PreferencePaymentMethodsRequest
    {
        [JsonProperty("excluded_payment_methods")]
        public List<PaymentMethodIdRequest> ExcludedPaymentMethods { get; set; }

        [JsonProperty("excluded_payment_types")]
        public List<PaymentMethodIdRequest> ExcludedPaymentTypes { get; set; }

        [JsonProperty("installments")]
        public int? Installments { get; set; }

        [JsonProperty("default_installments")]
        public int? DefaultInstallments { get; set; }

        [JsonProperty("default_payment_method_id")]
        public string DefaultPaymentMethodId { get; set; }
    }

    public class PaymentMethodIdRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    // ─── Response ───

    public class PreferenceResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("init_point")]
        public string InitPoint { get; set; }

        [JsonProperty("sandbox_init_point")]
        public string SandboxInitPoint { get; set; }

        [JsonProperty("external_reference")]
        public string ExternalReference { get; set; }

        [JsonProperty("items")]
        public List<PreferenceItemRequest> Items { get; set; }

        [JsonProperty("date_created")]
        public DateTime? DateCreated { get; set; }

        [JsonProperty("collector_id")]
        public long? CollectorId { get; set; }
    }
}
