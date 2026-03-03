using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Models.Payments;
using MercadoPago.Wrapper.Models.Preferences;

namespace MercadoPago.Wrapper.Interfaces
{
    /// <summary>Operaciones de marketplace con split payments.</summary>
    public interface IMarketplaceService
    {
        Task<MpApiResponse<PaymentResponse>> CreatePaymentAsync(PaymentCreateRequest request, string idempotencyKey = null, CancellationToken ct = default);
        Task<MpApiResponse<PreferenceResponse>> CreatePreferenceAsync(PreferenceCreateRequest request, CancellationToken ct = default);
    }
}
