using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MercadoPago.Wrapper.Models.Payments
{
    // ─── Request ───

    /// <summary>Solicitud para crear un pago directo.</summary>
    public class PaymentCreateRequest
    {
        [JsonProperty("transaction_amount")]
        public decimal TransactionAmount { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("installments")]
        public int Installments { get; set; } = 1;

        [JsonProperty("payment_method_id")]
        public string PaymentMethodId { get; set; }

        [JsonProperty("issuer_id")]
        public long? IssuerId { get; set; }

        [JsonProperty("payer")]
        public PayerRequest Payer { get; set; }

        [JsonProperty("external_reference")]
        public string ExternalReference { get; set; }

        [JsonProperty("notification_url")]
        public string NotificationUrl { get; set; }

        [JsonProperty("binary_mode")]
        public bool? BinaryMode { get; set; }

        [JsonProperty("additional_info")]
        public AdditionalInfoRequest AdditionalInfo { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, object> Metadata { get; set; }

        [JsonProperty("statement_descriptor")]
        public string StatementDescriptor { get; set; }

        [JsonProperty("application_fee")]
        public decimal? ApplicationFee { get; set; }
    }

    public class PayerRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("identification")]
        public Common.IdentificationRequest Identification { get; set; }

        [JsonProperty("phone")]
        public Common.PhoneRequest Phone { get; set; }

        [JsonProperty("address")]
        public Common.AddressRequest Address { get; set; }

        [JsonProperty("entity_type")]
        public string EntityType { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class AdditionalInfoRequest
    {
        [JsonProperty("items")]
        public List<ItemRequest> Items { get; set; }

        [JsonProperty("payer")]
        public AdditionalPayerRequest Payer { get; set; }

        [JsonProperty("ip_address")]
        public string IpAddress { get; set; }
    }

    public class ItemRequest
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

        [JsonProperty("unit_price")]
        public decimal UnitPrice { get; set; }

        [JsonProperty("picture_url")]
        public string PictureUrl { get; set; }
    }

    public class AdditionalPayerRequest
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("registration_date")]
        public DateTime? RegistrationDate { get; set; }
    }

    /// <summary>Criterios de búsqueda de pagos.</summary>
    public class PaymentSearchRequest
    {
        public string ExternalReference { get; set; }
        public string Status { get; set; }
        public DateTime? DateCreatedFrom { get; set; }
        public DateTime? DateCreatedTo { get; set; }
        public string Sort { get; set; } = "date_created";
        public string Criteria { get; set; } = "desc";
        public int? Offset { get; set; }
        public int? Limit { get; set; } = 30;

        /// <summary>Construye el query string para la API.</summary>
        public string ToQueryString()
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(ExternalReference))
                parts.Add($"external_reference={Uri.EscapeDataString(ExternalReference)}");
            if (!string.IsNullOrEmpty(Status))
                parts.Add($"status={Status}");
            if (DateCreatedFrom.HasValue)
                parts.Add($"begin_date={DateCreatedFrom:yyyy-MM-ddTHH:mm:ssZ}");
            if (DateCreatedTo.HasValue)
                parts.Add($"end_date={DateCreatedTo:yyyy-MM-ddTHH:mm:ssZ}");
            if (!string.IsNullOrEmpty(Sort))
                parts.Add($"sort={Sort}");
            if (!string.IsNullOrEmpty(Criteria))
                parts.Add($"criteria={Criteria}");
            if (Offset.HasValue)
                parts.Add($"offset={Offset}");
            if (Limit.HasValue)
                parts.Add($"limit={Limit}");
            return string.Join("&", parts);
        }
    }

    // ─── Response ───

    /// <summary>Pago devuelto por la API.</summary>
    public class PaymentResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("date_created")]
        public DateTime? DateCreated { get; set; }

        [JsonProperty("date_approved")]
        public DateTime? DateApproved { get; set; }

        [JsonProperty("date_last_updated")]
        public DateTime? DateLastUpdated { get; set; }

        [JsonProperty("money_release_date")]
        public DateTime? MoneyReleaseDate { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_detail")]
        public string StatusDetail { get; set; }

        [JsonProperty("operation_type")]
        public string OperationType { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("external_reference")]
        public string ExternalReference { get; set; }

        [JsonProperty("transaction_amount")]
        public decimal TransactionAmount { get; set; }

        [JsonProperty("transaction_amount_refunded")]
        public decimal? TransactionAmountRefunded { get; set; }

        [JsonProperty("net_received_amount")]
        public decimal? NetReceivedAmount { get; set; }

        [JsonProperty("currency_id")]
        public string CurrencyId { get; set; }

        [JsonProperty("payment_method_id")]
        public string PaymentMethodId { get; set; }

        [JsonProperty("payment_type_id")]
        public string PaymentTypeId { get; set; }

        [JsonProperty("installments")]
        public int? Installments { get; set; }

        [JsonProperty("issuer_id")]
        public string IssuerId { get; set; }

        [JsonProperty("payer")]
        public PayerResponse Payer { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, object> Metadata { get; set; }

        [JsonProperty("notification_url")]
        public string NotificationUrl { get; set; }

        [JsonProperty("point_of_interaction")]
        public PointOfInteraction PointOfInteraction { get; set; }
    }

    public class PayerResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("identification")]
        public Common.IdentificationRequest Identification { get; set; }
    }

    public class PointOfInteraction
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("transaction_data")]
        public TransactionData TransactionData { get; set; }
    }

    public class TransactionData
    {
        [JsonProperty("qr_code")]
        public string QrCode { get; set; }

        [JsonProperty("qr_code_base64")]
        public string QrCodeBase64 { get; set; }

        [JsonProperty("ticket_url")]
        public string TicketUrl { get; set; }
    }

    /// <summary>Reembolso devuelto por la API.</summary>
    public class RefundResponse
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
        public DateTime? DateCreated { get; set; }

        [JsonProperty("source")]
        public RefundSource Source { get; set; }
    }

    public class RefundSource
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
