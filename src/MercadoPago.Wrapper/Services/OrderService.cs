using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Interfaces;
using MercadoPago.Wrapper.Models.Orders;

namespace MercadoPago.Wrapper.Services
{
    /// <summary>Servicio de órdenes (API unificada v1/orders).</summary>
    public class OrderService : IOrderService
    {
        private readonly MpHttpClient _http;

        public OrderService(MpHttpClient http)
        {
            _http = http;
        }

        public async Task<MpApiResponse<OrderResponse>> CreateAsync(
            OrderCreateRequest request,
            string idempotencyKey = null,
            CancellationToken ct = default)
        {
            return await _http.PostAsync<OrderResponse>(
                "/v1/orders", request, idempotencyKey, ct);
        }

        public async Task<MpApiResponse<OrderResponse>> GetAsync(
            string id, CancellationToken ct = default)
        {
            return await _http.GetAsync<OrderResponse>(
                $"/v1/orders/{id}", ct);
        }

        public async Task<MpApiResponse<OrderResponse>> CancelAsync(
            string id, CancellationToken ct = default)
        {
            return await _http.PutAsync<OrderResponse>(
                $"/v1/orders/{id}",
                new { status = "cancelled" }, ct);
        }

        public async Task<MpApiResponse<OrderResponse>> RefundAsync(
            string orderId, decimal? amount = null,
            CancellationToken ct = default)
        {
            var body = amount.HasValue
                ? (object)new { amount = amount.Value }
                : new { };
            return await _http.PostAsync<OrderResponse>(
                $"/v1/orders/{orderId}/refund", body, ct: ct);
        }
    }
}
