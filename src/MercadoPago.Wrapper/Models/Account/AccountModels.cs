using System.Collections.Generic;
using Newtonsoft.Json;

namespace MercadoPago.Wrapper.Models.Account
{
    /// <summary>Información del usuario de MercadoPago.</summary>
    public class UserInfo
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("registration_date")]
        public string RegistrationDate { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("site_id")]
        public string SiteId { get; set; }

        [JsonProperty("country_id")]
        public string CountryId { get; set; }

        [JsonProperty("phone")]
        public UserPhone Phone { get; set; }

        [JsonProperty("identification")]
        public UserIdentification Identification { get; set; }

        [JsonProperty("address")]
        public UserAddress Address { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("logo")]
        public string Logo { get; set; }

        [JsonProperty("points")]
        public int? Points { get; set; }

        [JsonProperty("site_status")]
        public string SiteStatus { get; set; }

        [JsonProperty("permalink")]
        public string Permalink { get; set; }

        [JsonProperty("seller_reputation")]
        public SellerReputation SellerReputation { get; set; }

        [JsonProperty("status")]
        public UserAccountStatus Status { get; set; }
    }

    public class UserPhone
    {
        [JsonProperty("area_code")]
        public string AreaCode { get; set; }

        [JsonProperty("extension")]
        public string Extension { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }
    }

    public class UserIdentification
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }
    }

    public class UserAddress
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("zip_code")]
        public string ZipCode { get; set; }
    }

    public class SellerReputation
    {
        [JsonProperty("level_id")]
        public string LevelId { get; set; }

        [JsonProperty("power_seller_status")]
        public string PowerSellerStatus { get; set; }

        [JsonProperty("transactions")]
        public ReputationTransactions Transactions { get; set; }
    }

    public class ReputationTransactions
    {
        [JsonProperty("canceled")]
        public int Canceled { get; set; }

        [JsonProperty("completed")]
        public int Completed { get; set; }

        [JsonProperty("period")]
        public string Period { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }

    public class UserAccountStatus
    {
        [JsonProperty("site_status")]
        public string SiteStatus { get; set; }

        [JsonProperty("list")]
        public object List { get; set; }
    }

    /// <summary>Balance de la cuenta de MercadoPago.</summary>
    public class AccountBalance
    {
        [JsonProperty("currency_id")]
        public string CurrencyId { get; set; }

        [JsonProperty("balance")]
        public decimal Balance { get; set; }

        [JsonProperty("available_balance")]
        public decimal AvailableBalance { get; set; }

        [JsonProperty("unavailable_balance")]
        public decimal UnavailableBalance { get; set; }

        [JsonProperty("total_amount")]
        public decimal TotalAmount { get; set; }
    }
}
