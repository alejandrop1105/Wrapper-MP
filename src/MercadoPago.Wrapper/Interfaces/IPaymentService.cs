using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Models.Payments;

namespace MercadoPago.Wrapper.Interfaces
{
    /// <summary>Operaciones de pagos directos y reembolsos.</summary>
    public interface IPaymentService
    {
        Task<MpApiResponse<PaymentResponse>> CreateAsync(PaymentCreateRequest request, string idempotencyKey = null, CancellationToken ct = default);
        Task<MpApiResponse<PaymentResponse>> GetAsync(long id, CancellationToken ct = default);
        Task<MpApiResponse<MpPaginatedResponse<PaymentResponse>>> SearchAsync(PaymentSearchRequest criteria, CancellationToken ct = default);
        Task<MpApiResponse<PaymentResponse>> CancelAsync(long id, CancellationToken ct = default);
        Task<MpApiResponse<RefundResponse>> RefundAsync(long id, decimal? amount = null, CancellationToken ct = default);
        Task<MpApiResponse<List<RefundResponse>>> GetRefundsAsync(long paymentId, CancellationToken ct = default);
    }
}
