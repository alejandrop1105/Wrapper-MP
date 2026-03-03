using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Models.Stores;
using MercadoPago.Wrapper.Models.Pos;
using MercadoPago.Wrapper.Models.QrCode;

namespace MercadoPago.Wrapper.Interfaces
{
    /// <summary>Operaciones de sucursales.</summary>
    public interface IStoreService
    {
        Task<MpApiResponse<StoreResponse>> CreateAsync(StoreCreateRequest request, CancellationToken ct = default);
        Task<MpApiResponse<StoreResponse>> GetAsync(string id, CancellationToken ct = default);
        Task<MpApiResponse<StoreSearchResponse>> SearchAsync(CancellationToken ct = default);
        Task<MpApiResponse<StoreResponse>> UpdateAsync(string id, StoreCreateRequest request, CancellationToken ct = default);
        Task<MpApiResponse<object>> DeleteAsync(string id, CancellationToken ct = default);
    }

    /// <summary>Operaciones de cajas / puntos de venta.</summary>
    public interface ICashierService
    {
        Task<MpApiResponse<PosResponse>> CreateAsync(PosCreateRequest request, CancellationToken ct = default);
        Task<MpApiResponse<PosResponse>> GetAsync(long id, CancellationToken ct = default);
        Task<MpApiResponse<PosSearchResponse>> SearchAsync(CancellationToken ct = default);
        Task<MpApiResponse<PosResponse>> UpdateAsync(long id, PosCreateRequest request, CancellationToken ct = default);
        Task<MpApiResponse<object>> DeleteAsync(long id, CancellationToken ct = default);
    }

    /// <summary>Operaciones de órdenes QR en cajas.</summary>
    public interface IQrCodeService
    {
        /// <summary>Crea una orden QR asociada a una caja (modelo atendido).</summary>
        Task<MpApiResponse<QrOrderResponse>> CreateOrderAsync(string userId, string externalPosId, QrOrderRequest request, CancellationToken ct = default);
        /// <summary>Elimina la orden QR de una caja.</summary>
        Task<MpApiResponse<object>> DeleteOrderAsync(string userId, string externalPosId, CancellationToken ct = default);
    }
}
