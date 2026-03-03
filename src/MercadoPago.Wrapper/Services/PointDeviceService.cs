using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Interfaces;
using MercadoPago.Wrapper.Models.PointDevice;

namespace MercadoPago.Wrapper.Services
{
    /// <summary>Servicio de terminales Point Smart.</summary>
    public class PointDeviceService : IPointDeviceService
    {
        private readonly MpHttpClient _http;

        public PointDeviceService(MpHttpClient http)
        {
            _http = http;
        }

        public async Task<MpApiResponse<PointDeviceListResponse>> ListDevicesAsync(
            string storeId = null, string posId = null,
            CancellationToken ct = default)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(storeId))
                parts.Add($"store_id={storeId}");
            if (!string.IsNullOrEmpty(posId))
                parts.Add($"pos_id={posId}");

            var qs = parts.Count > 0 ? "?" + string.Join("&", parts) : "";
            return await _http.GetAsync<PointDeviceListResponse>(
                $"/point/integration-api/devices{qs}", ct);
        }

        public async Task<MpApiResponse<PointPaymentIntentResponse>> CreatePaymentIntentAsync(
            string deviceId,
            PointPaymentIntentRequest request,
            CancellationToken ct = default)
        {
            return await _http.PostAsync<PointPaymentIntentResponse>(
                $"/point/integration-api/devices/{deviceId}/payment-intents",
                request, ct: ct);
        }

        public async Task<MpApiResponse<PointPaymentIntentResponse>> CancelPaymentIntentAsync(
            string deviceId, string intentId,
            CancellationToken ct = default)
        {
            return await _http.DeleteAsync<PointPaymentIntentResponse>(
                $"/point/integration-api/devices/{deviceId}/payment-intents/{intentId}",
                ct);
        }

        public async Task<MpApiResponse<PointPaymentIntentResponse>> GetPaymentIntentStatusAsync(
            string intentId, CancellationToken ct = default)
        {
            return await _http.GetAsync<PointPaymentIntentResponse>(
                $"/point/integration-api/payment-intents/{intentId}", ct);
        }
    }
}
