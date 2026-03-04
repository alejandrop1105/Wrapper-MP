using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Models.Chargebacks;

namespace MercadoPago.Wrapper.Interfaces
{
    /// <summary>Servicio de contracargos (chargebacks).</summary>
    public interface IChargebackService
    {
        /// <summary>Obtiene un contracargo por ID.</summary>
        Task<MpApiResponse<ChargebackResponse>> GetAsync(
            string id, CancellationToken ct = default);
    }
}
