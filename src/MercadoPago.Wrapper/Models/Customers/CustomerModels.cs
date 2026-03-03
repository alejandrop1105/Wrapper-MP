using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MercadoPago.Wrapper.Models.Customers
{
    // ─── Request ───

    public class CustomerCreateRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("phone")]
        public Common.PhoneRequest Phone { get; set; }

        [JsonProperty("identification")]
        public Common.IdentificationRequest Identification { get; set; }

        [JsonProperty("default_address")]
        public string DefaultAddress { get; set; }

        [JsonProperty("address")]
        public Common.AddressRequest Address { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, object> Metadata { get; set; }
    }

    public class CustomerSearchRequest
    {
        public string Email { get; set; }
        public int? Offset { get; set; }
        public int? Limit { get; set; } = 30;

        public string ToQueryString()
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(Email))
                parts.Add($"email={Uri.EscapeDataString(Email)}");
            if (Offset.HasValue) parts.Add($"offset={Offset}");
            if (Limit.HasValue) parts.Add($"limit={Limit}");
            return string.Join("&", parts);
        }
    }

    public class CardCreateRequest
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("issuer_id")]
        public string IssuerId { get; set; }

        [JsonProperty("payment_method_id")]
        public string PaymentMethodId { get; set; }
    }

    // ─── Response ───

    public class CustomerResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("phone")]
        public Common.PhoneRequest Phone { get; set; }

        [JsonProperty("identification")]
        public Common.IdentificationRequest Identification { get; set; }

        [JsonProperty("address")]
        public Common.AddressRequest Address { get; set; }

        [JsonProperty("date_created")]
        public DateTime? DateCreated { get; set; }

        [JsonProperty("date_last_updated")]
        public DateTime? DateLastUpdated { get; set; }

        [JsonProperty("cards")]
        public List<CardResponse> Cards { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, object> Metadata { get; set; }
    }

    public class CardResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("customer_id")]
        public string CustomerId { get; set; }

        [JsonProperty("expiration_month")]
        public int? ExpirationMonth { get; set; }

        [JsonProperty("expiration_year")]
        public int? ExpirationYear { get; set; }

        [JsonProperty("first_six_digits")]
        public string FirstSixDigits { get; set; }

        [JsonProperty("last_four_digits")]
        public string LastFourDigits { get; set; }

        [JsonProperty("payment_method")]
        public CardPaymentMethod PaymentMethod { get; set; }

        [JsonProperty("issuer")]
        public CardIssuer Issuer { get; set; }

        [JsonProperty("date_created")]
        public DateTime? DateCreated { get; set; }
    }

    public class CardPaymentMethod
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("payment_type_id")]
        public string PaymentTypeId { get; set; }
    }

    public class CardIssuer
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
