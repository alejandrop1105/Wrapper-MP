using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Models.Subscriptions;

namespace MercadoPago.Wrapper.Interfaces
{
    /// <summary>Operaciones de suscripciones recurrentes.</summary>
    public interface ISubscriptionService
    {
        Task<MpApiResponse<SubscriptionPlanResponse>> CreatePlanAsync(SubscriptionPlanCreateRequest request, CancellationToken ct = default);
        Task<MpApiResponse<SubscriptionPlanResponse>> GetPlanAsync(string id, CancellationToken ct = default);
        Task<MpApiResponse<SubscriptionResponse>> CreateSubscriptionAsync(SubscriptionCreateRequest request, CancellationToken ct = default);
        Task<MpApiResponse<SubscriptionResponse>> GetSubscriptionAsync(string id, CancellationToken ct = default);
        Task<MpApiResponse<SubscriptionResponse>> UpdateSubscriptionAsync(string id, SubscriptionUpdateRequest request, CancellationToken ct = default);
    }
}
