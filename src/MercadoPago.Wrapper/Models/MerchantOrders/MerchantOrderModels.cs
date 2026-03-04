using System.Collections.Generic;
using Newtonsoft.Json;

namespace MercadoPago.Wrapper.Models.MerchantOrders
{
    /// <summary>Request para crear una merchant order.</summary>
    public class MerchantOrderCreateRequest
    {
        [JsonProperty("preference_id")]
        public string PreferenceId { get; set; }

        [JsonProperty("application_id")]
        public string ApplicationId { get; set; }

        [JsonProperty("site_id")]
        public string SiteId { get; set; }

        [JsonProperty("notification_url")]
        public string NotificationUrl { get; set; }

        [JsonProperty("additional_info")]
        public string AdditionalInfo { get; set; }

        [JsonProperty("external_reference")]
        public string ExternalReference { get; set; }

        [JsonProperty("marketplace")]
        public string Marketplace { get; set; }

        [JsonProperty("items")]
        public List<MerchantOrderItem> Items { get; set; }

        [JsonProperty("collector")]
        public MerchantOrderCollector Collector { get; set; }
    }

    public class MerchantOrderItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("category_id")]
        public string CategoryId { get; set; }

        [JsonProperty("currency_id")]
        public string CurrencyId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("picture_url")]
        public string PictureUrl { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("unit_price")]
        public decimal UnitPrice { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public class MerchantOrderCollector
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }
    }

    /// <summary>Respuesta de merchant order.</summary>
    public class MerchantOrderResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("external_reference")]
        public string ExternalReference { get; set; }

        [JsonProperty("preference_id")]
        public string PreferenceId { get; set; }

        [JsonProperty("payments")]
        public List<MerchantOrderPayment> Payments { get; set; }

        [JsonProperty("shipments")]
        public List<object> Shipments { get; set; }

        [JsonProperty("collector")]
        public MerchantOrderCollector Collector { get; set; }

        [JsonProperty("marketplace")]
        public string Marketplace { get; set; }

        [JsonProperty("notification_url")]
        public string NotificationUrl { get; set; }

        [JsonProperty("date_created")]
        public string DateCreated { get; set; }

        [JsonProperty("last_updated")]
        public string LastUpdated { get; set; }

        [JsonProperty("sponsor_id")]
        public long? SponsorId { get; set; }

        [JsonProperty("shipping_cost")]
        public decimal? ShippingCost { get; set; }

        [JsonProperty("total_amount")]
        public decimal? TotalAmount { get; set; }

        [JsonProperty("site_id")]
        public string SiteId { get; set; }

        [JsonProperty("paid_amount")]
        public decimal? PaidAmount { get; set; }

        [JsonProperty("refunded_amount")]
        public decimal? RefundedAmount { get; set; }

        [JsonProperty("items")]
        public List<MerchantOrderItem> Items { get; set; }

        [JsonProperty("cancelled")]
        public bool Cancelled { get; set; }

        [JsonProperty("additional_info")]
        public string AdditionalInfo { get; set; }

        [JsonProperty("application_id")]
        public string ApplicationId { get; set; }

        [JsonProperty("order_status")]
        public string OrderStatus { get; set; }
    }

    public class MerchantOrderPayment
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("transaction_amount")]
        public decimal TransactionAmount { get; set; }

        [JsonProperty("total_paid_amount")]
        public decimal TotalPaidAmount { get; set; }

        [JsonProperty("shipping_cost")]
        public decimal ShippingCost { get; set; }

        [JsonProperty("currency_id")]
        public string CurrencyId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_detail")]
        public string StatusDetail { get; set; }

        [JsonProperty("operation_type")]
        public string OperationType { get; set; }

        [JsonProperty("date_approved")]
        public string DateApproved { get; set; }

        [JsonProperty("date_created")]
        public string DateCreated { get; set; }

        [JsonProperty("last_modified")]
        public string LastModified { get; set; }

        [JsonProperty("amount_refunded")]
        public decimal AmountRefunded { get; set; }
    }

    /// <summary>Respuesta de búsqueda de merchant orders.</summary>
    public class MerchantOrderSearchResponse
    {
        [JsonProperty("elements")]
        public List<MerchantOrderResponse> Elements { get; set; }

        [JsonProperty("next_offset")]
        public int? NextOffset { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
