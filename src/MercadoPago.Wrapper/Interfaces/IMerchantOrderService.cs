using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Models.MerchantOrders;

namespace MercadoPago.Wrapper.Interfaces
{
    /// <summary>Servicio de Merchant Orders (órdenes de comercio).</summary>
    public interface IMerchantOrderService
    {
        Task<MpApiResponse<MerchantOrderResponse>> CreateAsync(
            MerchantOrderCreateRequest request, CancellationToken ct = default);

        Task<MpApiResponse<MerchantOrderResponse>> GetAsync(
            long id, CancellationToken ct = default);

        Task<MpApiResponse<MerchantOrderResponse>> UpdateAsync(
            long id, MerchantOrderCreateRequest request,
            CancellationToken ct = default);

        Task<MpApiResponse<MerchantOrderSearchResponse>> SearchAsync(
            string externalReference = null, string preferenceId = null,
            CancellationToken ct = default);
    }
}
