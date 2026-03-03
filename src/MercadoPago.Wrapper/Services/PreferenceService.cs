using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Interfaces;
using MercadoPago.Wrapper.Models.Preferences;

namespace MercadoPago.Wrapper.Services
{
    /// <summary>Servicio de preferencias (Checkout Pro).</summary>
    public class PreferenceService : IPreferenceService
    {
        private readonly MpHttpClient _http;

        public PreferenceService(MpHttpClient http)
        {
            _http = http;
        }

        public async Task<MpApiResponse<PreferenceResponse>> CreateAsync(
            PreferenceCreateRequest request,
            string idempotencyKey = null,
            CancellationToken ct = default)
        {
            return await _http.PostAsync<PreferenceResponse>(
                "/checkout/preferences", request, idempotencyKey, ct);
        }

        public async Task<MpApiResponse<PreferenceResponse>> GetAsync(
            string id, CancellationToken ct = default)
        {
            return await _http.GetAsync<PreferenceResponse>(
                $"/checkout/preferences/{id}", ct);
        }

        public async Task<MpApiResponse<PreferenceResponse>> UpdateAsync(
            string id, PreferenceCreateRequest request,
            CancellationToken ct = default)
        {
            return await _http.PutAsync<PreferenceResponse>(
                $"/checkout/preferences/{id}", request, ct);
        }
    }
}
