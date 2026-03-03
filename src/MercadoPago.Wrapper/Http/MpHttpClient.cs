using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Configuration;
using MercadoPago.Wrapper.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace MercadoPago.Wrapper.Http
{
    /// <summary>
    /// Cliente HTTP especializado para la API de MercadoPago.
    /// Maneja autenticación, idempotencia, retry y logging automáticamente.
    /// </summary>
    public class MpHttpClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly MpWrapperConfig _config;
        private readonly ILogger _logger;
        private readonly JsonSerializerSettings _jsonSettings;
        private bool _disposed;

        public MpHttpClient(MpWrapperConfig config, ILogger logger = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? Log.Logger;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_config.BaseUrl),
                Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds)
            };

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _config.AccessToken);
            _httpClient.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.UserAgent
                .ParseAdd(_config.UserAgent);

            _jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ"
            };
        }

        /// <summary>Realiza un GET y deserializa la respuesta.</summary>
        public async Task<MpApiResponse<T>> GetAsync<T>(
            string endpoint,
            CancellationToken ct = default)
        {
            return await ExecuteWithRetryAsync<T>(
                HttpMethod.Get, endpoint, null, ct);
        }

        /// <summary>Realiza un POST con body JSON.</summary>
        public async Task<MpApiResponse<T>> PostAsync<T>(
            string endpoint,
            object body,
            string idempotencyKey = null,
            CancellationToken ct = default)
        {
            return await ExecuteWithRetryAsync<T>(
                HttpMethod.Post, endpoint, body, ct, idempotencyKey);
        }

        /// <summary>Realiza un PUT con body JSON.</summary>
        public async Task<MpApiResponse<T>> PutAsync<T>(
            string endpoint,
            object body,
            CancellationToken ct = default)
        {
            return await ExecuteWithRetryAsync<T>(
                HttpMethod.Put, endpoint, body, ct);
        }

        /// <summary>Realiza un DELETE.</summary>
        public async Task<MpApiResponse<T>> DeleteAsync<T>(
            string endpoint,
            CancellationToken ct = default)
        {
            return await ExecuteWithRetryAsync<T>(
                HttpMethod.Delete, endpoint, null, ct);
        }

        /// <summary>Realiza un DELETE sin esperar body de respuesta.</summary>
        public async Task<MpApiResponse<object>> DeleteAsync(
            string endpoint,
            CancellationToken ct = default)
        {
            return await ExecuteWithRetryAsync<object>(
                HttpMethod.Delete, endpoint, null, ct);
        }

        private async Task<MpApiResponse<T>> ExecuteWithRetryAsync<T>(
            HttpMethod method,
            string endpoint,
            object body,
            CancellationToken ct,
            string idempotencyKey = null)
        {
            int maxAttempts = _config.MaxRetries + 1;
            MpApiResponse<T> lastResponse = null;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    lastResponse = await ExecuteAsync<T>(
                        method, endpoint, body, idempotencyKey, ct);

                    // No reintentar en errores de cliente (4xx) excepto 429
                    if (lastResponse.IsSuccess ||
                        (lastResponse.StatusCode >= 400 &&
                         lastResponse.StatusCode < 500 &&
                         lastResponse.StatusCode != 429))
                    {
                        return lastResponse;
                    }

                    if (attempt < maxAttempts)
                    {
                        int delay = CalculateBackoff(attempt);
                        _logger.Warning(
                            "MP API reintento {Attempt}/{Max} para {Method} {Endpoint} " +
                            "(status={Status}). Esperando {Delay}ms...",
                            attempt, maxAttempts, method, endpoint,
                            lastResponse.StatusCode, delay);
                        await Task.Delay(delay, ct);
                    }
                }
                catch (TaskCanceledException) when (!ct.IsCancellationRequested)
                {
                    if (attempt >= maxAttempts)
                        throw new MpException(
                            $"Timeout al conectar con MercadoPago ({_config.TimeoutSeconds}s).",
                            statusCode: 408);

                    int delay = CalculateBackoff(attempt);
                    _logger.Warning(
                        "MP API timeout en intento {Attempt}/{Max}. Reintentando en {Delay}ms...",
                        attempt, maxAttempts, delay);
                    await Task.Delay(delay, ct);
                }
            }

            return lastResponse;
        }

        private async Task<MpApiResponse<T>> ExecuteAsync<T>(
            HttpMethod method,
            string endpoint,
            object body,
            string idempotencyKey,
            CancellationToken ct)
        {
            using (var request = new HttpRequestMessage(method, endpoint))
            {
                // Idempotencia en POSTs
                if (method == HttpMethod.Post)
                {
                    var key = idempotencyKey ?? Guid.NewGuid().ToString("N");
                    request.Headers.Add("X-Idempotency-Key", key);
                }

                // Body JSON
                if (body != null)
                {
                    var json = JsonConvert.SerializeObject(body, _jsonSettings);
                    request.Content = new StringContent(
                        json, Encoding.UTF8, "application/json");

                    _logger.Debug("MP API {Method} {Endpoint} → {Body}",
                        method, endpoint, json);
                }
                else
                {
                    _logger.Debug("MP API {Method} {Endpoint}", method, endpoint);
                }

                using (var httpResponse = await _httpClient.SendAsync(request, ct))
                {
                    var rawJson = await httpResponse.Content.ReadAsStringAsync();
                    var statusCode = (int)httpResponse.StatusCode;

                    _logger.Debug("MP API ← {StatusCode} {Endpoint}: {Response}",
                        statusCode, endpoint,
                        rawJson.Length > 500 ? rawJson.Substring(0, 500) + "..." : rawJson);

                    var response = new MpApiResponse<T>
                    {
                        StatusCode = statusCode,
                        RawJson = rawJson
                    };

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        if (!string.IsNullOrWhiteSpace(rawJson))
                        {
                            response.Data = JsonConvert.DeserializeObject<T>(
                                rawJson, _jsonSettings);
                        }
                    }
                    else
                    {
                        ParseErrorResponse(response, rawJson, statusCode);
                    }

                    return response;
                }
            }
        }

        private void ParseErrorResponse<T>(
            MpApiResponse<T> response,
            string rawJson,
            int statusCode)
        {
            try
            {
                var errorObj = JObject.Parse(rawJson);
                response.ErrorMessage = errorObj["message"]?.ToString()
                    ?? errorObj["error"]?.ToString()
                    ?? "Error desconocido";
                response.ErrorCode = errorObj["error"]?.ToString();

                var causes = errorObj["cause"] as JArray;
                if (causes != null)
                {
                    response.Causes = causes
                        .Select(c => c["description"]?.ToString()
                            ?? c["code"]?.ToString()
                            ?? c.ToString())
                        .ToList();
                }
            }
            catch
            {
                response.ErrorMessage = $"Error HTTP {statusCode}: {rawJson}";
            }

            _logger.Error(
                "MP API Error {StatusCode}: {Error} | Causas: {Causes}",
                statusCode, response.ErrorMessage,
                string.Join("; ", response.Causes));
        }

        /// <summary>
        /// Lanza una excepción tipada si la respuesta no fue exitosa.
        /// </summary>
        public void ThrowIfError<T>(MpApiResponse<T> response)
        {
            if (response.IsSuccess)
                return;

            var causes = response.Causes?.ToArray();

            switch (response.StatusCode)
            {
                case 401:
                case 403:
                    throw new MpAuthenticationException(response.ErrorMessage);
                case 400:
                    throw new MpValidationException(response.ErrorMessage, causes);
                case 404:
                    throw new MpNotFoundException(response.ErrorMessage);
                case 429:
                    throw new MpRateLimitException(response.ErrorMessage);
                default:
                    if (response.StatusCode >= 500)
                        throw new MpServerException(
                            response.ErrorMessage, response.StatusCode);
                    throw new MpException(
                        response.ErrorMessage, response.StatusCode,
                        response.ErrorCode, causes);
            }
        }

        private static int CalculateBackoff(int attempt)
        {
            // Backoff exponencial: 500ms, 1s, 2s, 4s...
            return (int)(Math.Pow(2, attempt - 1) * 500);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}
