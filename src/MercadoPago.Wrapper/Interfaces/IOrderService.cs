using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Models.Orders;

namespace MercadoPago.Wrapper.Interfaces
{
    /// <summary>Operaciones de órdenes (API unificada v1/orders).</summary>
    public interface IOrderService
    {
        Task<MpApiResponse<OrderResponse>> CreateAsync(OrderCreateRequest request, string idempotencyKey = null, CancellationToken ct = default);
        Task<MpApiResponse<OrderResponse>> GetAsync(string id, CancellationToken ct = default);
        Task<MpApiResponse<OrderResponse>> CancelAsync(string id, CancellationToken ct = default);
        Task<MpApiResponse<OrderResponse>> RefundAsync(string orderId, decimal? amount = null, CancellationToken ct = default);
    }
}
