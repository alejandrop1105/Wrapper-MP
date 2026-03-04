using System.Collections.Generic;
using Newtonsoft.Json;

namespace MercadoPago.Wrapper.Models.Site
{
    // ─── Tipos de identificación ───

    /// <summary>Tipo de documento de identificación (DNI, CUIT, etc.).</summary>
    public class IdentificationType
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("min_length")]
        public int MinLength { get; set; }

        [JsonProperty("max_length")]
        public int MaxLength { get; set; }
    }

    // ─── Métodos de pago ───

    /// <summary>Método de pago disponible.</summary>
    public class PaymentMethodInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("payment_type_id")]
        public string PaymentTypeId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("secure_thumbnail")]
        public string SecureThumbnail { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("deferred_capture")]
        public string DeferredCapture { get; set; }

        [JsonProperty("settings")]
        public List<PaymentMethodSetting> Settings { get; set; }

        [JsonProperty("additional_info_needed")]
        public List<string> AdditionalInfoNeeded { get; set; }

        [JsonProperty("min_allowed_amount")]
        public decimal? MinAllowedAmount { get; set; }

        [JsonProperty("max_allowed_amount")]
        public decimal? MaxAllowedAmount { get; set; }

        [JsonProperty("accreditation_time")]
        public int? AccreditationTime { get; set; }

        [JsonProperty("financial_institutions")]
        public List<FinancialInstitution> FinancialInstitutions { get; set; }

        [JsonProperty("processing_modes")]
        public List<string> ProcessingModes { get; set; }
    }

    public class PaymentMethodSetting
    {
        [JsonProperty("card_number")]
        public CardNumberSetting CardNumber { get; set; }

        [JsonProperty("bin")]
        public BinSetting Bin { get; set; }

        [JsonProperty("security_code")]
        public SecurityCodeSetting SecurityCode { get; set; }
    }

    public class CardNumberSetting
    {
        [JsonProperty("validation")]
        public string Validation { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }
    }

    public class BinSetting
    {
        [JsonProperty("pattern")]
        public string Pattern { get; set; }

        [JsonProperty("installments_pattern")]
        public string InstallmentsPattern { get; set; }

        [JsonProperty("exclusion_pattern")]
        public string ExclusionPattern { get; set; }
    }

    public class SecurityCodeSetting
    {
        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("card_location")]
        public string CardLocation { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }
    }

    public class FinancialInstitution
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    // ─── Cuotas ───

    /// <summary>Información de cuotas para un método de pago.</summary>
    public class InstallmentInfo
    {
        [JsonProperty("payment_method_id")]
        public string PaymentMethodId { get; set; }

        [JsonProperty("payment_type_id")]
        public string PaymentTypeId { get; set; }

        [JsonProperty("issuer")]
        public CardIssuer Issuer { get; set; }

        [JsonProperty("processing_mode")]
        public string ProcessingMode { get; set; }

        [JsonProperty("merchant_account_id")]
        public string MerchantAccountId { get; set; }

        [JsonProperty("payer_costs")]
        public List<PayerCost> PayerCosts { get; set; }
    }

    public class PayerCost
    {
        [JsonProperty("installments")]
        public int Installments { get; set; }

        [JsonProperty("installment_rate")]
        public decimal InstallmentRate { get; set; }

        [JsonProperty("discount_rate")]
        public decimal DiscountRate { get; set; }

        [JsonProperty("reimbursement_rate")]
        public decimal? ReimbursementRate { get; set; }

        [JsonProperty("labels")]
        public List<string> Labels { get; set; }

        [JsonProperty("installment_rate_collector")]
        public List<string> InstallmentRateCollector { get; set; }

        [JsonProperty("min_allowed_amount")]
        public decimal MinAllowedAmount { get; set; }

        [JsonProperty("max_allowed_amount")]
        public decimal MaxAllowedAmount { get; set; }

        [JsonProperty("recommended_message")]
        public string RecommendedMessage { get; set; }

        [JsonProperty("installment_amount")]
        public decimal InstallmentAmount { get; set; }

        [JsonProperty("total_amount")]
        public decimal TotalAmount { get; set; }
    }

    /// <summary>Emisor de tarjeta.</summary>
    public class CardIssuer
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("secure_thumbnail")]
        public string SecureThumbnail { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }
    }
}
