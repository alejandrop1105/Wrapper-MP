using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Models.Site;

namespace MercadoPago.Wrapper.Interfaces
{
    /// <summary>Servicio de recursos de sitio (catálogos de referencia).</summary>
    public interface ISiteService
    {
        /// <summary>Obtiene los tipos de documento de identificación del país.</summary>
        Task<MpApiResponse<List<IdentificationType>>> GetIdentificationTypesAsync(
            CancellationToken ct = default);

        /// <summary>Obtiene todos los métodos de pago disponibles.</summary>
        Task<MpApiResponse<List<PaymentMethodInfo>>> GetPaymentMethodsAsync(
            CancellationToken ct = default);

        /// <summary>Obtiene un método de pago específico.</summary>
        Task<MpApiResponse<PaymentMethodInfo>> GetPaymentMethodAsync(
            string paymentMethodId, CancellationToken ct = default);

        /// <summary>Obtiene las cuotas disponibles para un monto y método de pago.</summary>
        Task<MpApiResponse<List<InstallmentInfo>>> GetInstallmentsAsync(
            decimal amount, string paymentMethodId = null,
            string issuerId = null, CancellationToken ct = default);

        /// <summary>Obtiene los emisores de tarjeta para un método de pago.</summary>
        Task<MpApiResponse<List<CardIssuer>>> GetCardIssuersAsync(
            string paymentMethodId, CancellationToken ct = default);
    }
}
