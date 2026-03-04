using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Interfaces;
using MercadoPago.Wrapper.Models.Account;

namespace MercadoPago.Wrapper.Services
{
    /// <summary>Servicio de cuenta y balance.</summary>
    public class AccountService : IAccountService
    {
        private readonly MpHttpClient _http;
        private readonly string _userId;

        public AccountService(MpHttpClient http, string userId = "me")
        {
            _http = http;
            _userId = userId ?? "me";
        }

        public async Task<MpApiResponse<UserInfo>> GetUserInfoAsync(
            CancellationToken ct = default)
        {
            return await _http.GetAsync<UserInfo>("/users/me", ct);
        }

        public async Task<MpApiResponse<AccountBalance>> GetAccountBalanceAsync(
            CancellationToken ct = default)
        {
            return await _http.GetAsync<AccountBalance>(
                $"/users/{_userId}/mercadopago_account/balance", ct);
        }
    }
}
