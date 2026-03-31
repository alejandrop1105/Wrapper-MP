using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MercadoPago.Wrapper.Models.Orders
{
    // ─── Request ───

    /// <summary>Solicitud para crear una orden (unificada para QR, Point, online).</summary>
    public class OrderCreateRequest
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "online";

        [JsonProperty("total_amount")]
        public string TotalAmount { get; set; }

        [JsonProperty("external_reference")]
        public string ExternalReference { get; set; }

        /// <summary>Identificador de plataforma asignado por MercadoPago al integrador homologado.</summary>
        [JsonProperty("platform_id")]
        public string PlatformId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("notification_url")]
        public string NotificationUrl { get; set; }

        [JsonProperty("expiration_time")]
        public string ExpirationTime { get; set; }

        /// <summary>Configuración adicional de la orden (Point, impresión, etc.).</summary>
        [JsonProperty("config")]
        public OrderConfigRequest Config { get; set; }

        [JsonProperty("transactions")]
        public OrderTransactionsRequest Transactions { get; set; }

        [JsonProperty("payer")]
        public OrderPayerRequest Payer { get; set; }
    }

    /// <summary>Configuración de la orden.</summary>
    public class OrderConfigRequest
    {
        /// <summary>Configuración específica de Point Smart.</summary>
        [JsonProperty("point")]
        public OrderPointConfigRequest Point { get; set; }
    }

    /// <summary>Configuración específica de Point Smart en la orden.</summary>
    public class OrderPointConfigRequest
    {
        /// <summary>Indica si el ticket debe ser impreso en el dispositivo.</summary>
        [JsonProperty("print_on_terminal")]
        public bool? PrintOnTerminal { get; set; }

        /// <summary>Número de identificación que será impreso en el ticket.</summary>
        [JsonProperty("ticket_number")]
        public string TicketNumber { get; set; }
    }

    public class OrderTransactionsRequest
    {
        [JsonProperty("payments")]
        public List<OrderTransactionPaymentRequest> Payments { get; set; }
    }

    public class OrderTransactionPaymentRequest
    {
        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("payment_method")]
        public OrderPaymentMethodRequest PaymentMethod { get; set; }
    }

    public class OrderPaymentMethodRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("installments")]
        public int? Installments { get; set; }
    }

    public class OrderPayerRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }
    }

    /// <summary>Criterios de búsqueda de órdenes.</summary>
    public class OrderSearchRequest
    {
        public string ExternalReference { get; set; }
        public string Status { get; set; }
        public int? Offset { get; set; }
        public int? Limit { get; set; } = 30;

        public string ToQueryString()
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(ExternalReference))
                parts.Add($"external_reference={Uri.EscapeDataString(ExternalReference)}");
            if (!string.IsNullOrEmpty(Status))
                parts.Add($"status={Status}");
            if (Offset.HasValue)
                parts.Add($"offset={Offset}");
            if (Limit.HasValue)
                parts.Add($"limit={Limit}");
            return string.Join("&", parts);
        }
    }

    // ─── Response ───

    public class OrderResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_detail")]
        public string StatusDetail { get; set; }

        [JsonProperty("external_reference")]
        public string ExternalReference { get; set; }

        [JsonProperty("total_amount")]
        public string TotalAmount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("created_date")]
        public DateTime? CreatedDate { get; set; }

        [JsonProperty("last_updated_date")]
        public DateTime? LastUpdatedDate { get; set; }

        [JsonProperty("transactions")]
        public OrderTransactionsResponse Transactions { get; set; }

        [JsonProperty("payer")]
        public OrderPayerResponse Payer { get; set; }
    }

    public class OrderTransactionsResponse
    {
        [JsonProperty("payments")]
        public List<OrderPaymentResponse> Payments { get; set; }
    }

    public class OrderPaymentResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_detail")]
        public string StatusDetail { get; set; }
    }

    public class OrderPayerResponse
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
