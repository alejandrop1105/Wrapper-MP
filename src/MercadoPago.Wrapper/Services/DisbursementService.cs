using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Interfaces;
using MercadoPago.Wrapper.Models.Disbursements;

namespace MercadoPago.Wrapper.Services
{
    /// <summary>Servicio de desembolsos.</summary>
    public class DisbursementService : IDisbursementService
    {
        private readonly MpHttpClient _http;

        public DisbursementService(MpHttpClient http)
        {
            _http = http;
        }

        public async Task<MpApiResponse<List<DisbursementRefundResponse>>> GetRefundsAsync(
            long advancedPaymentId, long disbursementId,
            CancellationToken ct = default)
        {
            return await _http.GetAsync<List<DisbursementRefundResponse>>(
                $"/v1/advanced_payments/{advancedPaymentId}/disbursements/{disbursementId}/refunds", ct);
        }

        public async Task<MpApiResponse<DisbursementRefundResponse>> RefundAsync(
            long advancedPaymentId, long disbursementId,
            DisbursementRefundRequest request = null,
            CancellationToken ct = default)
        {
            return await _http.PostAsync<DisbursementRefundResponse>(
                $"/v1/advanced_payments/{advancedPaymentId}/disbursements/{disbursementId}/refunds",
                request ?? new DisbursementRefundRequest(), ct: ct);
        }
    }
}
