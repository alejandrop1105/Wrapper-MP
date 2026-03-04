using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Interfaces;
using MercadoPago.Wrapper.Models.Chargebacks;

namespace MercadoPago.Wrapper.Services
{
    /// <summary>Servicio de contracargos.</summary>
    public class ChargebackService : IChargebackService
    {
        private readonly MpHttpClient _http;

        public ChargebackService(MpHttpClient http)
        {
            _http = http;
        }

        public async Task<MpApiResponse<ChargebackResponse>> GetAsync(
            string id, CancellationToken ct = default)
        {
            return await _http.GetAsync<ChargebackResponse>(
                $"/v1/chargebacks/{id}", ct);
        }
    }
}
