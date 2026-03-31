using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Configuration;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Interfaces;
using MercadoPago.Wrapper.Models.OAuth;
using Newtonsoft.Json;
using Serilog;

namespace MercadoPago.Wrapper.Services
{
    /// <summary>
    /// Servicio de OAuth para MercadoPago.
    /// Maneja el flujo de authorization_code y refresh_token,
    /// con soporte para renovación automática de tokens.
    /// </summary>
    public class OAuthService : IOAuthService, IDisposable
    {
        private readonly MpWrapperConfig _config;
        private readonly MpHttpClient _httpClient;
        private readonly ILogger _logger;
        private Timer _refreshTimer;
        private string _currentRefreshToken;
        private bool _disposed;

        /// <inheritdoc />
        public bool IsAutoRefreshEnabled => _refreshTimer != null;

        /// <inheritdoc />
        public event EventHandler<OAuthTokenResponse> OnTokenRefreshed;

        /// <inheritdoc />
        public event EventHandler<Exception> OnTokenRefreshFailed;

        /// <summary>
        /// Callback opcional para actualizar el access token en el HttpClient
        /// cuando se renueva exitosamente.
        /// </summary>
        internal Action<string> OnAccessTokenUpdated { get; set; }

        public OAuthService(
            MpWrapperConfig config, MpHttpClient httpClient,
            ILogger logger = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = httpClient;
            _logger = logger ?? Log.Logger;
        }

        /// <inheritdoc />
        public async Task<MpApiResponse<OAuthTokenResponse>> ExchangeCodeAsync(
            string authorizationCode,
            string redirectUri = null,
            CancellationToken ct = default)
        {
            ValidateOAuthConfig();

            var request = new OAuthTokenRequest
            {
                ClientId = _config.ClientId,
                ClientSecret = _config.ClientSecret,
                GrantType = "authorization_code",
                Code = authorizationCode,
                RedirectUri = redirectUri
            };

            _logger.Information(
                "OAuth: Intercambiando código de autorización...");

            var response = await PostOAuthAsync(request, ct);

            if (response.IsSuccess && response.Data != null)
            {
                _logger.Information(
                    "OAuth: Token obtenido exitosamente. " +
                    "UserId={UserId}, ExpiresIn={ExpiresIn}s",
                    response.Data.UserId, response.Data.ExpiresIn);
            }

            return response;
        }

        /// <inheritdoc />
        public async Task<MpApiResponse<OAuthTokenResponse>> RefreshTokenAsync(
            string refreshToken = null,
            CancellationToken ct = default)
        {
            ValidateOAuthConfig();

            var token = refreshToken
                ?? _currentRefreshToken
                ?? _config.RefreshToken;

            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException(
                    "No se proporcionó un refresh_token. " +
                    "Usar ExchangeCodeAsync primero o configurar " +
                    "WithRefreshToken() en el Builder.");

            var request = new OAuthTokenRequest
            {
                ClientId = _config.ClientId,
                ClientSecret = _config.ClientSecret,
                GrantType = "refresh_token",
                RefreshToken = token
            };

            _logger.Information("OAuth: Renovando access token...");

            var response = await PostOAuthAsync(request, ct);

            if (response.IsSuccess && response.Data != null)
            {
                // Almacenar el nuevo refresh token
                _currentRefreshToken = response.Data.RefreshToken;

                // Actualizar el access token en el HttpClient
                OnAccessTokenUpdated?.Invoke(response.Data.AccessToken);

                _logger.Information(
                    "OAuth: Token renovado exitosamente. " +
                    "ExpiresIn={ExpiresIn}s, NuevoRefreshToken={HasNew}",
                    response.Data.ExpiresIn,
                    !string.IsNullOrEmpty(response.Data.RefreshToken));

                OnTokenRefreshed?.Invoke(this, response.Data);
            }
            else
            {
                _logger.Error(
                    "OAuth: Error al renovar token. Status={Status}, " +
                    "Error={Error}",
                    response.StatusCode, response.ErrorMessage);
            }

            return response;
        }

        /// <inheritdoc />
        public void StartAutoRefresh(
            string refreshToken,
            long expiresInSeconds,
            int refreshMarginSeconds = 3600)
        {
            StopAutoRefresh();

            _currentRefreshToken = refreshToken;

            // Calcular cuándo renovar: (expiración - margen)
            var refreshInMs = Math.Max(
                (expiresInSeconds - refreshMarginSeconds) * 1000,
                60000); // mínimo 1 minuto

            _logger.Information(
                "OAuth: Auto-refresh programado en {Minutes} minutos " +
                "(expiración en {ExpiresIn}s, margen {Margin}s)",
                refreshInMs / 60000, expiresInSeconds, refreshMarginSeconds);

            _refreshTimer = new Timer(
                async _ => await ExecuteAutoRefreshAsync(),
                null,
                (long)refreshInMs,
                Timeout.Infinite);
        }

        /// <inheritdoc />
        public void StopAutoRefresh()
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Dispose();
                _refreshTimer = null;
                _logger.Information("OAuth: Auto-refresh detenido.");
            }
        }

        private async Task ExecuteAutoRefreshAsync()
        {
            try
            {
                var response = await RefreshTokenAsync();

                if (response.IsSuccess && response.Data != null)
                {
                    // Re-programar el próximo refresh
                    var expiresIn = response.Data.ExpiresIn ?? 21600;
                    StartAutoRefresh(
                        response.Data.RefreshToken, expiresIn);
                }
                else
                {
                    _logger.Error(
                        "OAuth: Auto-refresh falló. No se re-programa. " +
                        "Error: {Error}", response.ErrorMessage);
                    OnTokenRefreshFailed?.Invoke(this,
                        new Exception(
                            $"OAuth refresh failed: {response.ErrorMessage}"));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "OAuth: Excepción en auto-refresh.");
                OnTokenRefreshFailed?.Invoke(this, ex);
            }
        }

        private async Task<MpApiResponse<OAuthTokenResponse>> PostOAuthAsync(
            OAuthTokenRequest request, CancellationToken ct)
        {
            // OAuth usa un POST sin Bearer token (las credenciales van en el body)
            return await _httpClient.PostAsync<OAuthTokenResponse>(
                "/oauth/token", request, ct: ct);
        }

        private void ValidateOAuthConfig()
        {
            if (string.IsNullOrWhiteSpace(_config.ClientId))
                throw new InvalidOperationException(
                    "ClientId no configurado. Usar WithClientId() en el Builder.");

            if (string.IsNullOrWhiteSpace(_config.ClientSecret))
                throw new InvalidOperationException(
                    "ClientSecret no configurado. Usar WithClientSecret() en el Builder.");
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                StopAutoRefresh();
                _disposed = true;
            }
        }
    }
}
