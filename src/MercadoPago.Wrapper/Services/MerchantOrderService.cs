using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Interfaces;
using MercadoPago.Wrapper.Models.MerchantOrders;

namespace MercadoPago.Wrapper.Services
{
    /// <summary>Servicio de Merchant Orders.</summary>
    public class MerchantOrderService : IMerchantOrderService
    {
        private readonly MpHttpClient _http;

        public MerchantOrderService(MpHttpClient http)
        {
            _http = http;
        }

        public async Task<MpApiResponse<MerchantOrderResponse>> CreateAsync(
            MerchantOrderCreateRequest request, CancellationToken ct = default)
        {
            return await _http.PostAsync<MerchantOrderResponse>(
                "/merchant_orders", request, ct: ct);
        }

        public async Task<MpApiResponse<MerchantOrderResponse>> GetAsync(
            long id, CancellationToken ct = default)
        {
            return await _http.GetAsync<MerchantOrderResponse>(
                $"/merchant_orders/{id}", ct);
        }

        public async Task<MpApiResponse<MerchantOrderResponse>> UpdateAsync(
            long id, MerchantOrderCreateRequest request,
            CancellationToken ct = default)
        {
            return await _http.PutAsync<MerchantOrderResponse>(
                $"/merchant_orders/{id}", request, ct);
        }

        public async Task<MpApiResponse<MerchantOrderSearchResponse>> SearchAsync(
            string externalReference = null, string preferenceId = null,
            CancellationToken ct = default)
        {
            var query = "";
            if (!string.IsNullOrEmpty(externalReference))
                query += $"?external_reference={externalReference}";
            if (!string.IsNullOrEmpty(preferenceId))
                query += (query == "" ? "?" : "&") +
                    $"preference_id={preferenceId}";

            return await _http.GetAsync<MerchantOrderSearchResponse>(
                $"/merchant_orders/search{query}", ct);
        }
    }
}
