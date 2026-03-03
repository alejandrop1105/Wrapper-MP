using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MercadoPago.Wrapper.Models.Webhooks;
using Newtonsoft.Json;
using Serilog;

namespace MercadoPago.Wrapper.Webhooks
{
    /// <summary>
    /// Servidor HTTP embebido para recibir webhooks de MercadoPago.
    /// Usa HttpListener y emite eventos tipados.
    /// </summary>
    public class WebhookListener : IDisposable
    {
        private readonly int _port;
        private readonly string _path;
        private readonly string _secret;
        private readonly ILogger _logger;
        private HttpListener _listener;
        private CancellationTokenSource _cts;
        private Task _listenerTask;
        private bool _disposed;

        /// <summary>Se dispara ante cualquier notificación de webhook recibida.</summary>
        public event EventHandler<WebhookEventArgs> OnNotificationReceived;

        /// <summary>Se dispara específicamente ante notificaciones de pago.</summary>
        public event EventHandler<WebhookEventArgs> OnPaymentNotification;

        /// <summary>Se dispara específicamente ante notificaciones de orden.</summary>
        public event EventHandler<WebhookEventArgs> OnOrderNotification;

        /// <summary>Indica si el listener está activo.</summary>
        public bool IsRunning { get; private set; }

        /// <summary>Puerto en el que escucha.</summary>
        public int Port => _port;

        public WebhookListener(
            int port = 5100,
            string path = "/webhooks/mp",
            string secret = null,
            ILogger logger = null)
        {
            _port = port;
            _path = path.StartsWith("/") ? path : "/" + path;
            _secret = secret;
            _logger = logger ?? Log.Logger;
        }

        /// <summary>Inicia el listener HTTP en un hilo secundario.</summary>
        public void Start()
        {
            if (IsRunning) return;

            _cts = new CancellationTokenSource();
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://+:{_port}{_path}/");

            try
            {
                _listener.Start();
                IsRunning = true;
                _logger.Information(
                    "Webhook listener iniciado en http://localhost:{Port}{Path}/",
                    _port, _path);

                _listenerTask = Task.Run(
                    () => ListenAsync(_cts.Token), _cts.Token);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "Error al iniciar webhook listener en puerto {Port}", _port);
                throw;
            }
        }

        /// <summary>Detiene el listener.</summary>
        public void Stop()
        {
            if (!IsRunning) return;

            _cts?.Cancel();
            _listener?.Stop();
            IsRunning = false;
            _logger.Information("Webhook listener detenido.");
        }

        private async Task ListenAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _listener.IsListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = Task.Run(() => ProcessRequestAsync(context), ct);
                }
                catch (ObjectDisposedException) { break; }
                catch (HttpListenerException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error en webhook listener loop.");
                }
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                if (request.HttpMethod != "POST")
                {
                    response.StatusCode = 405;
                    response.Close();
                    return;
                }

                string body;
                using (var reader = new StreamReader(
                    request.InputStream, Encoding.UTF8))
                {
                    body = await reader.ReadToEndAsync();
                }

                // Validar firma si se proporcionó un secreto
                bool isValid = true;
                if (!string.IsNullOrEmpty(_secret))
                {
                    var xSignature = request.Headers["x-signature"];
                    var xRequestId = request.Headers["x-request-id"];
                    isValid = ValidateSignature(
                        xSignature, xRequestId, body);
                }

                var notification = JsonConvert.DeserializeObject<WebhookNotification>(body);

                var args = new WebhookEventArgs
                {
                    Notification = notification,
                    RawJson = body,
                    IsValid = isValid
                };

                _logger.Information(
                    "Webhook recibido: Type={Type}, Action={Action}, " +
                    "DataId={DataId}, Valid={Valid}",
                    notification?.Type, notification?.Action,
                    notification?.Data?.Id, isValid);

                // Emitir eventos
                OnNotificationReceived?.Invoke(this, args);

                if (notification?.Type == "payment")
                    OnPaymentNotification?.Invoke(this, args);
                else if (notification?.Type == "merchant_order"
                    || notification?.Type == "orders")
                    OnOrderNotification?.Invoke(this, args);

                // Responder 200 OK a MP
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error procesando webhook.");
                response.StatusCode = 500;
            }
            finally
            {
                response.Close();
            }
        }

        /// <summary>Valida la firma HMAC del webhook.</summary>
        public bool ValidateSignature(
            string xSignature, string xRequestId, string body)
        {
            if (string.IsNullOrEmpty(xSignature) ||
                string.IsNullOrEmpty(_secret))
                return false;

            try
            {
                // Parsear ts y v1 del header x-signature
                var parts = xSignature.Split(',')
                    .Select(p => p.Trim().Split('='))
                    .ToDictionary(p => p[0], p => p[1]);

                if (!parts.ContainsKey("ts") || !parts.ContainsKey("v1"))
                    return false;

                var ts = parts["ts"];
                var signature = parts["v1"];

                // Construir el string a firmar
                // Formato: id:[data.id];request-id:[x-request-id];ts:[ts];
                var notification = JsonConvert.DeserializeObject<WebhookNotification>(body);
                var dataId = notification?.Data?.Id ?? "";

                var signTemplate = $"id:{dataId};request-id:{xRequestId};ts:{ts};";

                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secret)))
                {
                    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signTemplate));
                    var computed = BitConverter.ToString(hash)
                        .Replace("-", "").ToLowerInvariant();
                    return computed == signature;
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Error validando firma de webhook.");
                return false;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Stop();
                _listener?.Close();
                _cts?.Dispose();
                _disposed = true;
            }
        }
    }
}
