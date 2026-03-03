using Newtonsoft.Json;

namespace MercadoPago.Wrapper.Models.Common
{
    /// <summary>Dirección postal.</summary>
    public class AddressRequest
    {
        [JsonProperty("street_name")]
        public string StreetName { get; set; }

        [JsonProperty("street_number")]
        public int? StreetNumber { get; set; }

        [JsonProperty("zip_code")]
        public string ZipCode { get; set; }

        [JsonProperty("city_name")]
        public string CityName { get; set; }

        [JsonProperty("state_name")]
        public string StateName { get; set; }
    }

    /// <summary>Teléfono.</summary>
    public class PhoneRequest
    {
        [JsonProperty("area_code")]
        public string AreaCode { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }
    }

    /// <summary>Identificación (DNI, CUIT, etc.).</summary>
    public class IdentificationRequest
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }
    }
}
