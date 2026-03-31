using System;
using Newtonsoft.Json;

namespace MercadoPago.Wrapper.Models.OAuth
{
    // ─── Request ───

    /// <summary>
    /// Solicitud para obtener o renovar un access token vía OAuth.
    /// </summary>
    public class OAuthTokenRequest
    {
        /// <summary>ID de la aplicación de MercadoPago.</summary>
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        /// <summary>Clave secreta de la aplicación.</summary>
        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        /// <summary>
        /// Tipo de grant: "authorization_code" (primera vez) o "refresh_token" (renovación).
        /// </summary>
        [JsonProperty("grant_type")]
        public string GrantType { get; set; }

        /// <summary>Código de autorización (solo para grant_type=authorization_code).</summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>URL de redirección registrada en la app (solo para authorization_code).</summary>
        [JsonProperty("redirect_uri")]
        public string RedirectUri { get; set; }

        /// <summary>Refresh token (solo para grant_type=refresh_token).</summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }

    // ─── Response ───

    /// <summary>
    /// Respuesta del endpoint /oauth/token de MercadoPago.
    /// </summary>
    public class OAuthTokenResponse
    {
        /// <summary>Token de acceso para autenticar las requests a la API.</summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>Tipo de token (generalmente "Bearer").</summary>
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        /// <summary>Tiempo de expiración del token en segundos.</summary>
        [JsonProperty("expires_in")]
        public long? ExpiresIn { get; set; }

        /// <summary>Scopes otorgados al token.</summary>
        [JsonProperty("scope")]
        public string Scope { get; set; }

        /// <summary>ID del usuario de MP que autorizó la vinculación.</summary>
        [JsonProperty("user_id")]
        public long? UserId { get; set; }

        /// <summary>
        /// Nuevo refresh token para la próxima renovación.
        /// IMPORTANTE: cada vez que se renueva el access token, el refresh_token
        /// también se actualiza y debe almacenarse.
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>Clave pública asociada al token.</summary>
        [JsonProperty("public_key")]
        public string PublicKey { get; set; }

        /// <summary>Indica si el token opera en modo live.</summary>
        [JsonProperty("live_mode")]
        public bool? LiveMode { get; set; }
    }
}
