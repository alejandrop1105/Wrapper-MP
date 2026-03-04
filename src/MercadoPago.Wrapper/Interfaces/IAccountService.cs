using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Models.Account;

namespace MercadoPago.Wrapper.Interfaces
{
    /// <summary>Servicio de cuenta y balance de MercadoPago.</summary>
    public interface IAccountService
    {
        /// <summary>Obtiene los datos del usuario autenticado.</summary>
        Task<MpApiResponse<UserInfo>> GetUserInfoAsync(
            CancellationToken ct = default);

        /// <summary>Obtiene el balance de la cuenta.</summary>
        Task<MpApiResponse<AccountBalance>> GetAccountBalanceAsync(
            CancellationToken ct = default);
    }
}
