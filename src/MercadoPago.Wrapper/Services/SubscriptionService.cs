using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Interfaces;
using MercadoPago.Wrapper.Models.Subscriptions;

namespace MercadoPago.Wrapper.Services
{
    /// <summary>Servicio de suscripciones recurrentes.</summary>
    public class SubscriptionService : ISubscriptionService
    {
        private readonly MpHttpClient _http;

        public SubscriptionService(MpHttpClient http)
        {
            _http = http;
        }

        public async Task<MpApiResponse<SubscriptionPlanResponse>> CreatePlanAsync(
            SubscriptionPlanCreateRequest request,
            CancellationToken ct = default)
        {
            return await _http.PostAsync<SubscriptionPlanResponse>(
                "/preapproval_plan", request, ct: ct);
        }

        public async Task<MpApiResponse<SubscriptionPlanResponse>> GetPlanAsync(
            string id, CancellationToken ct = default)
        {
            return await _http.GetAsync<SubscriptionPlanResponse>(
                $"/preapproval_plan/{id}", ct);
        }

        public async Task<MpApiResponse<SubscriptionResponse>> CreateSubscriptionAsync(
            SubscriptionCreateRequest request,
            CancellationToken ct = default)
        {
            return await _http.PostAsync<SubscriptionResponse>(
                "/preapproval", request, ct: ct);
        }

        public async Task<MpApiResponse<SubscriptionResponse>> GetSubscriptionAsync(
            string id, CancellationToken ct = default)
        {
            return await _http.GetAsync<SubscriptionResponse>(
                $"/preapproval/{id}", ct);
        }

        public async Task<MpApiResponse<SubscriptionResponse>> UpdateSubscriptionAsync(
            string id, SubscriptionUpdateRequest request,
            CancellationToken ct = default)
        {
            return await _http.PutAsync<SubscriptionResponse>(
                $"/preapproval/{id}", request, ct);
        }
    }
}
