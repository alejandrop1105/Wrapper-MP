using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Models.Preferences;

namespace MercadoPago.Wrapper.Interfaces
{
    /// <summary>Operaciones de preferencias (Checkout Pro).</summary>
    public interface IPreferenceService
    {
        Task<MpApiResponse<PreferenceResponse>> CreateAsync(PreferenceCreateRequest request, string idempotencyKey = null, CancellationToken ct = default);
        Task<MpApiResponse<PreferenceResponse>> GetAsync(string id, CancellationToken ct = default);
        Task<MpApiResponse<PreferenceResponse>> UpdateAsync(string id, PreferenceCreateRequest request, CancellationToken ct = default);
    }
}
