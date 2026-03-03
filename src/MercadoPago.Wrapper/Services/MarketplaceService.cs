using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Interfaces;
using MercadoPago.Wrapper.Models.Payments;
using MercadoPago.Wrapper.Models.Preferences;

namespace MercadoPago.Wrapper.Services
{
    /// <summary>Servicio de marketplace con split payments.</summary>
    public class MarketplaceService : IMarketplaceService
    {
        private readonly MpHttpClient _http;

        public MarketplaceService(MpHttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Crea un pago con application_fee para split entre vendedor y marketplace.
        /// </summary>
        public async Task<MpApiResponse<PaymentResponse>> CreatePaymentAsync(
            PaymentCreateRequest request,
            string idempotencyKey = null,
            CancellationToken ct = default)
        {
            return await _http.PostAsync<PaymentResponse>(
                "/v1/payments", request, idempotencyKey, ct);
        }

        /// <summary>
        /// Crea una preferencia con marketplace_fee para Checkout Pro con split.
        /// </summary>
        public async Task<MpApiResponse<PreferenceResponse>> CreatePreferenceAsync(
            PreferenceCreateRequest request,
            CancellationToken ct = default)
        {
            return await _http.PostAsync<PreferenceResponse>(
                "/checkout/preferences", request, ct: ct);
        }
    }
}
