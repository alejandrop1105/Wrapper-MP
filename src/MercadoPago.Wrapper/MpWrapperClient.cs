using System;
using MercadoPago.Wrapper.Configuration;
using MercadoPago.Wrapper.Http;
using MercadoPago.Wrapper.Interfaces;
using MercadoPago.Wrapper.Services;
using MercadoPago.Wrapper.Webhooks;
using Serilog;

namespace MercadoPago.Wrapper
{
    /// <summary>
    /// Punto de entrada principal de la librería MercadoPago Wrapper.
    /// Proporciona acceso a todos los servicios de la API de MercadoPago.
    /// 
    /// Uso:
    /// <code>
    /// var config = new MpWrapperConfig.Builder()
    ///     .WithAccessToken("APP_USR-...")
    ///     .WithEnvironment(MpEnvironment.Sandbox)
    ///     .Build();
    /// 
    /// using (var mp = new MpWrapperClient(config))
    /// {
    ///     var payment = await mp.Payments.CreateAsync(request);
    /// }
    /// </code>
    /// </summary>
    public class MpWrapperClient : IDisposable
    {
        private readonly MpHttpClient _httpClient;
        private readonly ILogger _logger;
        private bool _disposed;

        /// <summary>Configuración activa.</summary>
        public MpWrapperConfig Config { get; }

        /// <summary>Cliente HTTP subyacente para operaciones avanzadas.</summary>
        public MpHttpClient Http => _httpClient;

        // ─── Servicios ───

        /// <summary>Operaciones de pagos directos y reembolsos.</summary>
        public IPaymentService Payments { get; }

        /// <summary>Operaciones de órdenes (API unificada).</summary>
        public IOrderService Orders { get; }

        /// <summary>Preferencias de Checkout Pro.</summary>
        public IPreferenceService Preferences { get; }

        /// <summary>Gestión de clientes y tarjetas guardadas.</summary>
        public ICustomerService Customers { get; }

        /// <summary>Gestión de sucursales.</summary>
        public IStoreService Stores { get; }

        /// <summary>Gestión de cajas / puntos de venta.</summary>
        public ICashierService Cashiers { get; }

        /// <summary>Órdenes QR para cajas.</summary>
        public IQrCodeService QrCodes { get; }

        /// <summary>Terminales Point Smart.</summary>
        public IPointDeviceService PointDevices { get; }

        /// <summary>Suscripciones recurrentes.</summary>
        public ISubscriptionService Subscriptions { get; }

        /// <summary>Marketplace / Split payments.</summary>
        public IMarketplaceService Marketplace { get; }

        /// <summary>Webhook listener embebido.</summary>
        public WebhookListener WebhookListener { get; private set; }

        /// <summary>
        /// Crea una nueva instancia del cliente MercadoPago.
        /// </summary>
        /// <param name="config">Configuración del wrapper.</param>
        /// <param name="userId">
        /// ID del usuario de MP (requerido para operaciones de Stores).
        /// Se puede obtener con GET /users/me.
        /// </param>
        /// <param name="logger">Logger opcional de Serilog.</param>
        public MpWrapperClient(
            MpWrapperConfig config,
            string userId = null,
            ILogger logger = null)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? Log.Logger;
            _httpClient = new MpHttpClient(config, _logger);

            // Inicializar todos los servicios
            Payments = new PaymentService(_httpClient);
            Orders = new OrderService(_httpClient);
            Preferences = new PreferenceService(_httpClient);
            Customers = new CustomerService(_httpClient);
            Stores = new StoreService(_httpClient, userId ?? "me");
            Cashiers = new CashierService(_httpClient);
            QrCodes = new QrCodeService(_httpClient);
            PointDevices = new PointDeviceService(_httpClient);
            Subscriptions = new SubscriptionService(_httpClient);
            Marketplace = new MarketplaceService(_httpClient);

            _logger.Information(
                "MpWrapperClient inicializado. Entorno={Environment}, País={Country}",
                config.Environment, config.Country);
        }

        /// <summary>
        /// Configura y devuelve el webhook listener embebido.
        /// </summary>
        public WebhookListener ConfigureWebhookListener(
            int port = 5100,
            string path = "/webhooks/mp",
            string secret = null)
        {
            WebhookListener?.Dispose();
            WebhookListener = new WebhookListener(port, path, secret, _logger);
            return WebhookListener;
        }

        /// <summary>
        /// Prueba la conexión con MercadoPago obteniendo los datos del usuario.
        /// </summary>
        public async System.Threading.Tasks.Task<MpApiResponse<dynamic>> TestConnectionAsync()
        {
            return await _httpClient.GetAsync<dynamic>("/users/me");
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                WebhookListener?.Dispose();
                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}
