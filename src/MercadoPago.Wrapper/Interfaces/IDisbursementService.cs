using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Models.Disbursements;

namespace MercadoPago.Wrapper.Interfaces
{
    /// <summary>Servicio de desembolsos (advanced payments disbursements).</summary>
    public interface IDisbursementService
    {
        /// <summary>Obtiene los reembolsos de un desembolso.</summary>
        Task<MpApiResponse<List<DisbursementRefundResponse>>> GetRefundsAsync(
            long advancedPaymentId, long disbursementId,
            CancellationToken ct = default);

        /// <summary>Crea un reembolso de un desembolso.</summary>
        Task<MpApiResponse<DisbursementRefundResponse>> RefundAsync(
            long advancedPaymentId, long disbursementId,
            DisbursementRefundRequest request = null,
            CancellationToken ct = default);
    }
}
