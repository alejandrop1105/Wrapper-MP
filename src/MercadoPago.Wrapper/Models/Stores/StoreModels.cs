using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MercadoPago.Wrapper.Models.Stores
{
    // ─── Request ───

    public class StoreCreateRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("external_id")]
        public string ExternalId { get; set; }

        [JsonProperty("location")]
        public StoreLocationRequest Location { get; set; }

        [JsonProperty("business_hours")]
        public BusinessHoursRequest BusinessHours { get; set; }
    }

    public class StoreLocationRequest
    {
        [JsonProperty("street_number")]
        public string StreetNumber { get; set; }

        [JsonProperty("street_name")]
        public string StreetName { get; set; }

        [JsonProperty("city_name")]
        public string CityName { get; set; }

        [JsonProperty("state_name")]
        public string StateName { get; set; }

        [JsonProperty("latitude")]
        public double? Latitude { get; set; }

        [JsonProperty("longitude")]
        public double? Longitude { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }
    }

    public class BusinessHoursRequest
    {
        [JsonProperty("monday")]
        public List<HourBlock> Monday { get; set; }
        [JsonProperty("tuesday")]
        public List<HourBlock> Tuesday { get; set; }
        [JsonProperty("wednesday")]
        public List<HourBlock> Wednesday { get; set; }
        [JsonProperty("thursday")]
        public List<HourBlock> Thursday { get; set; }
        [JsonProperty("friday")]
        public List<HourBlock> Friday { get; set; }
        [JsonProperty("saturday")]
        public List<HourBlock> Saturday { get; set; }
        [JsonProperty("sunday")]
        public List<HourBlock> Sunday { get; set; }
    }

    public class HourBlock
    {
        [JsonProperty("open")]
        public string Open { get; set; }

        [JsonProperty("close")]
        public string Close { get; set; }
    }

    // ─── Response ───

    public class StoreResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("external_id")]
        public string ExternalId { get; set; }

        [JsonProperty("date_creation")]
        public DateTime? DateCreation { get; set; }

        [JsonProperty("location")]
        public StoreLocationRequest Location { get; set; }

        [JsonProperty("business_hours")]
        public BusinessHoursRequest BusinessHours { get; set; }
    }

    public class StoreSearchResponse
    {
        [JsonProperty("paging")]
        public Http.MpPaging Paging { get; set; }

        [JsonProperty("results")]
        public List<StoreResponse> Results { get; set; }
    }
}
