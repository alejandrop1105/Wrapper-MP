using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Interfaces;
using MercadoPago.Wrapper.Models.Site;

namespace MercadoPago.Wrapper.Services
{
    /// <summary>Servicio de recursos de sitio (catálogos).</summary>
    public class SiteService : ISiteService
    {
        private readonly MpHttpClient _http;

        public SiteService(MpHttpClient http)
        {
            _http = http;
        }

        public async Task<MpApiResponse<List<IdentificationType>>> GetIdentificationTypesAsync(
            CancellationToken ct = default)
        {
            return await _http.GetAsync<List<IdentificationType>>(
                "/v1/identification_types", ct);
        }

        public async Task<MpApiResponse<List<PaymentMethodInfo>>> GetPaymentMethodsAsync(
            CancellationToken ct = default)
        {
            return await _http.GetAsync<List<PaymentMethodInfo>>(
                "/v1/payment_methods", ct);
        }

        public async Task<MpApiResponse<PaymentMethodInfo>> GetPaymentMethodAsync(
            string paymentMethodId, CancellationToken ct = default)
        {
            return await _http.GetAsync<PaymentMethodInfo>(
                $"/v1/payment_methods/{paymentMethodId}", ct);
        }

        public async Task<MpApiResponse<List<InstallmentInfo>>> GetInstallmentsAsync(
            decimal amount, string paymentMethodId = null,
            string issuerId = null, CancellationToken ct = default)
        {
            var query = $"?amount={amount}";
            if (!string.IsNullOrEmpty(paymentMethodId))
                query += $"&payment_method_id={paymentMethodId}";
            if (!string.IsNullOrEmpty(issuerId))
                query += $"&issuer.id={issuerId}";

            return await _http.GetAsync<List<InstallmentInfo>>(
                $"/v1/payment_methods/installments{query}", ct);
        }

        public async Task<MpApiResponse<List<CardIssuer>>> GetCardIssuersAsync(
            string paymentMethodId, CancellationToken ct = default)
        {
            return await _http.GetAsync<List<CardIssuer>>(
                $"/v1/payment_methods/card_issuers?payment_method_id={paymentMethodId}", ct);
        }
    }
}
