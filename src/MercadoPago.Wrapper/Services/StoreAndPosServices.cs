using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Interfaces;
using MercadoPago.Wrapper.Models.Stores;
using MercadoPago.Wrapper.Models.Pos;
using MercadoPago.Wrapper.Models.QrCode;

namespace MercadoPago.Wrapper.Services
{
    /// <summary>Servicio de sucursales.</summary>
    public class StoreService : IStoreService
    {
        private readonly MpHttpClient _http;
        private readonly string _userId;

        public StoreService(MpHttpClient http, string userId)
        {
            _http = http;
            _userId = userId;
        }

        public async Task<MpApiResponse<StoreResponse>> CreateAsync(
            StoreCreateRequest request, CancellationToken ct = default)
        {
            return await _http.PostAsync<StoreResponse>(
                $"/users/{_userId}/stores", request, ct: ct);
        }

        public async Task<MpApiResponse<StoreResponse>> GetAsync(
            string id, CancellationToken ct = default)
        {
            return await _http.GetAsync<StoreResponse>(
                $"/users/{_userId}/stores/{id}", ct);
        }

        public async Task<MpApiResponse<StoreSearchResponse>> SearchAsync(
            CancellationToken ct = default)
        {
            return await _http.GetAsync<StoreSearchResponse>(
                $"/users/{_userId}/stores/search", ct);
        }

        public async Task<MpApiResponse<StoreResponse>> UpdateAsync(
            string id, StoreCreateRequest request,
            CancellationToken ct = default)
        {
            return await _http.PutAsync<StoreResponse>(
                $"/users/{_userId}/stores/{id}", request, ct);
        }

        public async Task<MpApiResponse<object>> DeleteAsync(
            string id, CancellationToken ct = default)
        {
            return await _http.DeleteAsync(
                $"/users/{_userId}/stores/{id}", ct);
        }
    }

    /// <summary>Servicio de cajas / puntos de venta.</summary>
    public class CashierService : ICashierService
    {
        private readonly MpHttpClient _http;

        public CashierService(MpHttpClient http)
        {
            _http = http;
        }

        public async Task<MpApiResponse<PosResponse>> CreateAsync(
            PosCreateRequest request, CancellationToken ct = default)
        {
            return await _http.PostAsync<PosResponse>(
                "/pos", request, ct: ct);
        }

        public async Task<MpApiResponse<PosResponse>> GetAsync(
            long id, CancellationToken ct = default)
        {
            return await _http.GetAsync<PosResponse>(
                $"/pos/{id}", ct);
        }

        public async Task<MpApiResponse<PosSearchResponse>> SearchAsync(
            CancellationToken ct = default)
        {
            return await _http.GetAsync<PosSearchResponse>("/pos", ct);
        }

        public async Task<MpApiResponse<PosResponse>> UpdateAsync(
            long id, PosCreateRequest request,
            CancellationToken ct = default)
        {
            return await _http.PutAsync<PosResponse>(
                $"/pos/{id}", request, ct);
        }

        public async Task<MpApiResponse<object>> DeleteAsync(
            long id, CancellationToken ct = default)
        {
            return await _http.DeleteAsync($"/pos/{id}", ct);
        }
    }

    /// <summary>Servicio de órdenes QR para cajas.</summary>
    public class QrCodeService : IQrCodeService
    {
        private readonly MpHttpClient _http;

        public QrCodeService(MpHttpClient http)
        {
            _http = http;
        }

        public async Task<MpApiResponse<QrOrderResponse>> CreateOrderAsync(
            string userId, string externalPosId,
            QrOrderRequest request, CancellationToken ct = default)
        {
            return await _http.PutAsync<QrOrderResponse>(
                $"/instore/qr/seller/collectors/{userId}/pos/{externalPosId}/orders",
                request, ct);
        }

        public async Task<MpApiResponse<object>> DeleteOrderAsync(
            string userId, string externalPosId,
            CancellationToken ct = default)
        {
            return await _http.DeleteAsync(
                $"/instore/qr/seller/collectors/{userId}/pos/{externalPosId}/orders",
                ct);
        }
    }
}
