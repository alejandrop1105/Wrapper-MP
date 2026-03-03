using System;
using Newtonsoft.Json;

namespace MercadoPago.Wrapper.Models.Webhooks
{
    /// <summary>Notificación de webhook de MercadoPago.</summary>
    public class WebhookNotification
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("live_mode")]
        public bool LiveMode { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("date_created")]
        public DateTime? DateCreated { get; set; }

        [JsonProperty("user_id")]
        public long? UserId { get; set; }

        [JsonProperty("api_version")]
        public string ApiVersion { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("data")]
        public WebhookData Data { get; set; }
    }

    public class WebhookData
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    /// <summary>Evento tipado para suscripción de notificaciones.</summary>
    public class WebhookEventArgs : EventArgs
    {
        public WebhookNotification Notification { get; set; }
        public string RawJson { get; set; }
        public bool IsValid { get; set; }
    }
}

namespace MercadoPago.Wrapper.Models.Marketplace
{
    /// <summary>Solicitud de pago con split para marketplace.</summary>
    public class MarketplacePaymentRequest : Payments.PaymentCreateRequest
    {
        // Hereda todo de PaymentCreateRequest, `application_fee` ya está definido allí
    }

    /// <summary>Preferencia con fee de marketplace.</summary>
    public class MarketplacePreferenceRequest : Preferences.PreferenceCreateRequest
    {
        // Hereda todo de PreferenceCreateRequest, `marketplace_fee` ya está definido allí
    }
}
