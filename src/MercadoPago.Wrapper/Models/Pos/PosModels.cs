using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MercadoPago.Wrapper.Models.Pos
{
    // ─── Request ───

    /// <summary>Solicitud para crear un punto de venta (caja).</summary>
    public class PosCreateRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("fixed_amount")]
        public bool? FixedAmount { get; set; }

        [JsonProperty("store_id")]
        public string StoreId { get; set; }

        [JsonProperty("external_store_id")]
        public string ExternalStoreId { get; set; }

        [JsonProperty("external_id")]
        public string ExternalId { get; set; }

        [JsonProperty("category")]
        public long? Category { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    // ─── Response ───

    public class PosResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("external_id")]
        public string ExternalId { get; set; }

        [JsonProperty("store_id")]
        public string StoreId { get; set; }

        [JsonProperty("external_store_id")]
        public string ExternalStoreId { get; set; }

        [JsonProperty("fixed_amount")]
        public bool? FixedAmount { get; set; }

        [JsonProperty("category")]
        public long? Category { get; set; }

        [JsonProperty("qr")]
        public PosQrResponse Qr { get; set; }

        [JsonProperty("date_created")]
        public DateTime? DateCreated { get; set; }

        [JsonProperty("date_last_updated")]
        public DateTime? DateLastUpdated { get; set; }
    }

    public class PosQrResponse
    {
        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("template_document")]
        public string TemplateDocument { get; set; }

        [JsonProperty("template_image")]
        public string TemplateImage { get; set; }
    }

    public class PosSearchResponse
    {
        [JsonProperty("paging")]
        public Http.MpPaging Paging { get; set; }

        [JsonProperty("results")]
        public List<PosResponse> Results { get; set; }
    }
}

namespace MercadoPago.Wrapper.Models.QrCode
{
    /// <summary>Solicitud para crear una orden QR asociada a una caja.</summary>
    public class QrOrderRequest
    {
        [JsonProperty("external_reference")]
        public string ExternalReference { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("notification_url")]
        public string NotificationUrl { get; set; }

        [JsonProperty("total_amount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("items")]
        public List<QrItemRequest> Items { get; set; } = new List<QrItemRequest>();

        [JsonProperty("expiration_date")]
        public DateTime? ExpirationDate { get; set; }
    }

    public class QrItemRequest
    {
        [JsonProperty("sku_number")]
        public string SkuNumber { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("unit_measure")]
        public string UnitMeasure { get; set; } = "unit";

        [JsonProperty("unit_price")]
        public decimal UnitPrice { get; set; }

        [JsonProperty("total_amount")]
        public decimal TotalAmount { get; set; }
    }

    public class QrOrderResponse
    {
        [JsonProperty("qr_data")]
        public string QrData { get; set; }

        [JsonProperty("in_store_order_id")]
        public string InStoreOrderId { get; set; }
    }
}
