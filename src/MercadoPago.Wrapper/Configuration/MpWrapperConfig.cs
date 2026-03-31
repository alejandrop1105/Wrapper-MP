using System;

namespace MercadoPago.Wrapper.Configuration
{
    /// <summary>
    /// Entornos disponibles de MercadoPago.
    /// </summary>
    public enum MpEnvironment
    {
        Sandbox,
        Production
    }

    /// <summary>
    /// Configuración principal del wrapper de MercadoPago.
    /// Utilizar el Builder para construir instancias.
    /// </summary>
    public class MpWrapperConfig
    {
        /// <summary>Token de acceso (obligatorio).</summary>
        public string AccessToken { get; private set; }

        /// <summary>Clave pública (opcional, usada en frontend/tokenización).</summary>
        public string PublicKey { get; private set; }

        /// <summary>Entorno de operación.</summary>
        public MpEnvironment Environment { get; private set; } = MpEnvironment.Sandbox;

        /// <summary>País (código ISO 3166-1 alpha-2, ej: "AR", "BR", "MX").</summary>
        public string Country { get; private set; } = "AR";

        /// <summary>URL base de la API (se configura automáticamente según entorno).</summary>
        public string BaseUrl { get; private set; } = "https://api.mercadopago.com";

        /// <summary>Máximo de reintentos automáticos en caso de error transitorio.</summary>
        public int MaxRetries { get; private set; } = 2;

        /// <summary>Timeout de las solicitudes HTTP en segundos.</summary>
        public int TimeoutSeconds { get; private set; } = 30;

        /// <summary>User-Agent personalizado para las solicitudes.</summary>
        public string UserAgent { get; private set; } = "MpWrapperClient/1.0";

        /// <summary>
        /// Identificador de plataforma asignado por MercadoPago al integrador homologado.
        /// Se envía automáticamente en la creación de órdenes.
        /// </summary>
        public string PlatformId { get; private set; }

        // ─── OAuth (opcional, para flujo OAuth marketplace) ───

        /// <summary>Client ID de la aplicación de MercadoPago (para flujo OAuth).</summary>
        public string ClientId { get; private set; }

        /// <summary>Client Secret de la aplicación de MercadoPago (para flujo OAuth).</summary>
        public string ClientSecret { get; private set; }

        /// <summary>Refresh token para renovación automática del access token.</summary>
        public string RefreshToken { get; private set; }

        private MpWrapperConfig() { }

        /// <summary>
        /// Builder para construir la configuración de forma fluida.
        /// </summary>
        public class Builder
        {
            private readonly MpWrapperConfig _config = new MpWrapperConfig();

            /// <summary>Configura el Access Token (obligatorio).</summary>
            public Builder WithAccessToken(string accessToken)
            {
                _config.AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
                return this;
            }

            /// <summary>Configura la Public Key.</summary>
            public Builder WithPublicKey(string publicKey)
            {
                _config.PublicKey = publicKey;
                return this;
            }

            /// <summary>Configura el entorno (Sandbox o Production).</summary>
            public Builder WithEnvironment(MpEnvironment environment)
            {
                _config.Environment = environment;
                return this;
            }

            /// <summary>Configura el país.</summary>
            public Builder WithCountry(string country)
            {
                _config.Country = country ?? "AR";
                return this;
            }

            /// <summary>Configura una URL base personalizada.</summary>
            public Builder WithBaseUrl(string baseUrl)
            {
                _config.BaseUrl = baseUrl;
                return this;
            }

            /// <summary>Configura el máximo de reintentos (0 = sin reintentos).</summary>
            public Builder WithMaxRetries(int maxRetries)
            {
                _config.MaxRetries = Math.Max(0, maxRetries);
                return this;
            }

            /// <summary>Configura el timeout en segundos.</summary>
            public Builder WithTimeout(int seconds)
            {
                _config.TimeoutSeconds = Math.Max(5, seconds);
                return this;
            }

            /// <summary>Configura el User-Agent.</summary>
            public Builder WithUserAgent(string userAgent)
            {
                _config.UserAgent = userAgent;
                return this;
            }

            /// <summary>Configura el platform_id asignado por MercadoPago (se obtiene al homologar).</summary>
            public Builder WithPlatformId(string platformId)
            {
                _config.PlatformId = platformId;
                return this;
            }

            /// <summary>Configura el Client ID para flujo OAuth.</summary>
            public Builder WithClientId(string clientId)
            {
                _config.ClientId = clientId;
                return this;
            }

            /// <summary>Configura el Client Secret para flujo OAuth.</summary>
            public Builder WithClientSecret(string clientSecret)
            {
                _config.ClientSecret = clientSecret;
                return this;
            }

            /// <summary>Configura el refresh token para renovación automática.</summary>
            public Builder WithRefreshToken(string refreshToken)
            {
                _config.RefreshToken = refreshToken;
                return this;
            }

            /// <summary>Construye la configuración validando campos obligatorios.</summary>
            public MpWrapperConfig Build()
            {
                if (string.IsNullOrWhiteSpace(_config.AccessToken))
                    throw new InvalidOperationException("El AccessToken es obligatorio. Use WithAccessToken().");

                return _config;
            }
        }
    }
}
