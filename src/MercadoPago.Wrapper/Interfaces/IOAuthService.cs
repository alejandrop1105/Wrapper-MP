using System;
using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Models.OAuth;

namespace MercadoPago.Wrapper.Interfaces
{
    /// <summary>
    /// Servicio de OAuth para MercadoPago.
    /// Maneja la obtención y renovación de access tokens.
    /// </summary>
    public interface IOAuthService
    {
        /// <summary>
        /// Intercambia un código de autorización por un access token (primera vinculación).
        /// </summary>
        Task<MpApiResponse<OAuthTokenResponse>> ExchangeCodeAsync(
            string authorizationCode,
            string redirectUri = null,
            CancellationToken ct = default);

        /// <summary>
        /// Renueva el access token usando el refresh token actual.
        /// El nuevo refresh_token debe almacenarse para futuras renovaciones.
        /// </summary>
        Task<MpApiResponse<OAuthTokenResponse>> RefreshTokenAsync(
            string refreshToken = null,
            CancellationToken ct = default);

        /// <summary>Indica si el auto-refresh está activo.</summary>
        bool IsAutoRefreshEnabled { get; }

        /// <summary>
        /// Inicia la renovación automática del token antes de que expire.
        /// </summary>
        /// <param name="refreshToken">Refresh token actual.</param>
        /// <param name="expiresInSeconds">Tiempo de expiración del token actual en segundos.</param>
        /// <param name="refreshMarginSeconds">Margen antes de la expiración para renovar (default: 1 hora).</param>
        void StartAutoRefresh(
            string refreshToken,
            long expiresInSeconds,
            int refreshMarginSeconds = 3600);

        /// <summary>Detiene la renovación automática.</summary>
        void StopAutoRefresh();

        /// <summary>Se dispara cuando el token fue renovado exitosamente.</summary>
        event EventHandler<OAuthTokenResponse> OnTokenRefreshed;

        /// <summary>Se dispara cuando la renovación del token falla.</summary>
        event EventHandler<Exception> OnTokenRefreshFailed;
    }
}
