using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MercadoPago.Wrapper.Models.PointDevice
{
    // ─── Response ───

    /// <summary>Respuesta de listado de terminales Point.</summary>
    public class PointDeviceResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("pos_id")]
        public long? PosId { get; set; }

        [JsonProperty("store_id")]
        public string StoreId { get; set; }

        [JsonProperty("external_pos_id")]
        public string ExternalPosId { get; set; }

        [JsonProperty("operating_mode")]
        public string OperatingMode { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("brand")]
        public string Brand { get; set; }
    }

    public class PointDeviceListResponse
    {
        [JsonProperty("devices")]
        public List<PointDeviceResponse> Devices { get; set; }

        [JsonProperty("paging")]
        public Http.MpPaging Paging { get; set; }
    }

    /// <summary>Solicitud de intent de pago para terminal Point.</summary>
    public class PointPaymentIntentRequest
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("external_reference")]
        public string ExternalReference { get; set; }

        [JsonProperty("installments")]
        public int Installments { get; set; } = 1;

        [JsonProperty("installments_cost")]
        public string InstallmentsCost { get; set; } = "seller";

        [JsonProperty("payment_type")]
        public string PaymentType { get; set; } = "credit_card";

        [JsonProperty("print_on_terminal")]
        public bool PrintOnTerminal { get; set; } = true;

        [JsonProperty("ticket_number")]
        public string TicketNumber { get; set; }
    }

    public class PointPaymentIntentResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("amount")]
        public decimal? Amount { get; set; }

        [JsonProperty("device_id")]
        public string DeviceId { get; set; }

        [JsonProperty("payment")]
        public PointPaymentInfo Payment { get; set; }
    }

    public class PointPaymentInfo
    {
        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_detail")]
        public string StatusDetail { get; set; }
    }
}
