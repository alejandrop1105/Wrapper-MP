using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Interfaces;
using MercadoPago.Wrapper.Models.Payments;

namespace MercadoPago.Wrapper.Services
{
    /// <summary>Servicio de pagos directos y reembolsos.</summary>
    public class PaymentService : IPaymentService
    {
        private readonly MpHttpClient _http;

        public PaymentService(MpHttpClient http)
        {
            _http = http;
        }

        public async Task<MpApiResponse<PaymentResponse>> CreateAsync(
            PaymentCreateRequest request,
            string idempotencyKey = null,
            CancellationToken ct = default)
        {
            return await _http.PostAsync<PaymentResponse>(
                "/v1/payments", request, idempotencyKey, ct);
        }

        public async Task<MpApiResponse<PaymentResponse>> GetAsync(
            long id, CancellationToken ct = default)
        {
            return await _http.GetAsync<PaymentResponse>(
                $"/v1/payments/{id}", ct);
        }

        public async Task<MpApiResponse<MpPaginatedResponse<PaymentResponse>>> SearchAsync(
            PaymentSearchRequest criteria,
            CancellationToken ct = default)
        {
            var qs = criteria?.ToQueryString() ?? string.Empty;
            var endpoint = string.IsNullOrEmpty(qs)
                ? "/v1/payments/search"
                : $"/v1/payments/search?{qs}";
            return await _http.GetAsync<MpPaginatedResponse<PaymentResponse>>(
                endpoint, ct);
        }

        public async Task<MpApiResponse<PaymentResponse>> CancelAsync(
            long id, CancellationToken ct = default)
        {
            return await _http.PutAsync<PaymentResponse>(
                $"/v1/payments/{id}",
                new { status = "cancelled" }, ct);
        }

        public async Task<MpApiResponse<RefundResponse>> RefundAsync(
            long id, decimal? amount = null,
            CancellationToken ct = default)
        {
            var body = amount.HasValue
                ? (object)new { amount = amount.Value }
                : new { };
            return await _http.PostAsync<RefundResponse>(
                $"/v1/payments/{id}/refunds", body, ct: ct);
        }

        public async Task<MpApiResponse<List<RefundResponse>>> GetRefundsAsync(
            long paymentId, CancellationToken ct = default)
        {
            return await _http.GetAsync<List<RefundResponse>>(
                $"/v1/payments/{paymentId}/refunds", ct);
        }
    }
}
