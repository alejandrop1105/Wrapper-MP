using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Models.PointDevice;

namespace MercadoPago.Wrapper.Interfaces
{
    /// <summary>Operaciones de terminales Point Smart.</summary>
    public interface IPointDeviceService
    {
        Task<MpApiResponse<PointDeviceListResponse>> ListDevicesAsync(string storeId = null, string posId = null, CancellationToken ct = default);
        Task<MpApiResponse<PointPaymentIntentResponse>> CreatePaymentIntentAsync(string deviceId, PointPaymentIntentRequest request, CancellationToken ct = default);
        Task<MpApiResponse<PointPaymentIntentResponse>> CancelPaymentIntentAsync(string deviceId, string intentId, CancellationToken ct = default);
        Task<MpApiResponse<PointPaymentIntentResponse>> GetPaymentIntentStatusAsync(string intentId, CancellationToken ct = default);

        /// <summary>Cambia el modo de operación de un terminal: "PDV" (integrado) o "STANDALONE" (autónomo).</summary>
        Task<MpApiResponse<PointDeviceResponse>> ChangeOperatingModeAsync(string deviceId, string operatingMode, CancellationToken ct = default);
    }
}
