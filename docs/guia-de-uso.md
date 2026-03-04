# MercadoPago Wrapper — Guía de Configuración y Uso

## Índice

1. [Introducción](#introducción)
2. [Requisitos previos](#requisitos-previos)
3. [Instalación](#instalación)
4. [Configuración inicial](#configuración-inicial)
5. [Módulos disponibles](#módulos-disponibles)
6. [Uso detallado por módulo](#uso-detallado-por-módulo)
   - [Pagos directos](#1-pagos-directos)
   - [Checkout Pro (Preferencias)](#2-checkout-pro-preferencias)
   - [Órdenes](#3-órdenes)
   - [Clientes](#4-clientes)
   - [Sucursales (Stores)](#5-sucursales-stores)
   - [Cajas / POS](#6-cajas--pos)
   - [Órdenes QR](#7-órdenes-qr)
   - [Terminales Point Smart](#8-terminales-point-smart)
   - [Suscripciones](#9-suscripciones)
   - [Marketplace (Split Payments)](#10-marketplace-split-payments)
7. [Webhooks](#webhooks)
8. [Manejo de errores](#manejo-de-errores)
9. [Logging](#logging)
10. [Referencia de configuración](#referencia-de-configuración)
11. [Preguntas frecuentes](#preguntas-frecuentes)

---

## Introducción

**MercadoPago.Wrapper** es una librería en C# para .NET Framework 4.8 que encapsula todas las operaciones de la API REST de MercadoPago. Fue diseñada para ser consumida desde sistemas ERP o cualquier aplicación .NET sin depender del SDK oficial (que requiere .NET Standard 2.1+).

### Características principales

- Compatible con **.NET Framework 4.8** (ideal para sistemas legacy y ERPs)
- Llamadas HTTP directas a la API REST de MercadoPago (sin SDK oficial)
- Autenticación Bearer Token automática
- Retry con backoff exponencial en errores transitorios
- Idempotencia automática en operaciones POST
- Logging integrado con Serilog
- Webhook listener embebido con validación HMAC-SHA256
- 10 módulos que cubren todos los modos de venta de MercadoPago

---

## Requisitos previos

- **.NET Framework 4.8** instalado
- Cuenta de **MercadoPago Developers** ([crear aquí](https://www.mercadopago.com.ar/developers/panel/app))
- **Access Token** de prueba o producción
- Paquetes NuGet (se resuelven automáticamente):
  - `Newtonsoft.Json` ≥ 13.0.3
  - `Serilog` ≥ 3.1.1

---

## Instalación

### Opción 1: Referencia directa al proyecto

Agregar referencia al `.csproj` del wrapper en tu solución:

```xml
<ItemGroup>
  <ProjectReference Include="..\MercadoPago.Wrapper\MercadoPago.Wrapper.csproj" />
</ItemGroup>
```

### Opción 2: Referencia al DLL compilado

Copiar `MercadoPago.Wrapper.dll` (y sus dependencias) desde `bin/Release/net48/` a tu proyecto y agregar la referencia.

---

## Configuración inicial

### Paso 1 — Obtener credenciales

1. Ingresar a [MercadoPago Developers](https://www.mercadopago.com.ar/developers/panel/app)
2. Crear una aplicación o seleccionar una existente
3. Copiar el **Access Token** (de prueba para sandbox, de producción para live)
4. Opcionalmente copiar la **Public Key** (solo necesaria para tokenización de tarjetas en frontend)

### Paso 2 — Crear la configuración

```csharp
using MercadoPago.Wrapper;
using MercadoPago.Wrapper.Configuration;

// Configuración con Builder Pattern
var config = new MpWrapperConfig.Builder()
    .WithAccessToken("APP_USR-xxxxxxxxxxxx-xxxxxx-xxxxxxxxxxxxxxxx-xxxxxxxxx")
    .WithEnvironment(MpEnvironment.Sandbox)  // o MpEnvironment.Production
    .WithCountry("AR")                       // AR, BR, MX, CO, CL, UY, PE
    .WithMaxRetries(2)                       // reintentos en errores transitorios
    .WithTimeout(30)                         // timeout en segundos
    .Build();
```

### Paso 3 — Crear el cliente

```csharp
// Sin logger personalizado (usa Serilog global)
using (var mp = new MpWrapperClient(config))
{
    // Usar los servicios...
}

// Con logger personalizado y User ID para operaciones de Stores
using (var mp = new MpWrapperClient(config, userId: "123456789", logger: myLogger))
{
    // Usar los servicios...
}
```

### Paso 4 — Verificar la conexión

```csharp
var result = await mp.TestConnectionAsync();
if (result.IsSuccess)
{
    Console.WriteLine("Conexión exitosa!");
    // result.Data contiene los datos del usuario de MP
}
```

---

## Módulos disponibles

| Propiedad | Interfaz | Descripción |
|---|---|---|
| `mp.Payments` | `IPaymentService` | Pagos directos y reembolsos |
| `mp.Preferences` | `IPreferenceService` | Checkout Pro |
| `mp.Orders` | `IOrderService` | Órdenes (API unificada) |
| `mp.Customers` | `ICustomerService` | Clientes y tarjetas |
| `mp.Stores` | `IStoreService` | Sucursales |
| `mp.Cashiers` | `ICashierService` | Cajas / POS |
| `mp.QrCodes` | `IQrCodeService` | Órdenes QR en cajas |
| `mp.PointDevices` | `IPointDeviceService` | Terminales Point Smart |
| `mp.Subscriptions` | `ISubscriptionService` | Suscripciones recurrentes |
| `mp.Marketplace` | `IMarketplaceService` | Split payments |
| `mp.Site` | `ISiteService` | Métodos de pago, tipos de doc, cuotas |
| `mp.MerchantOrders` | `IMerchantOrderService` | Merchant Orders |
| `mp.Account` | `IAccountService` | Info de cuenta y balance |
| `mp.Chargebacks` | `IChargebackService` | Contracargos |
| `mp.Disbursements` | `IDisbursementService` | Desembolsos (advanced payments) |

---

## Uso detallado por módulo

### 1. Pagos directos

#### Crear un pago

```csharp
var request = new PaymentCreateRequest
{
    TransactionAmount = 1500.00m,
    Description = "Producto de ejemplo",
    PaymentMethodId = "visa",
    Token = "card_token_from_frontend",  // token de tarjeta generado en frontend
    Installments = 3,
    ExternalReference = "VENTA-001",
    Payer = new PayerRequest
    {
        Email = "cliente@email.com",
        Identification = new IdentificationRequest
        {
            Type = "DNI",
            Number = "12345678"
        }
    }
};

var result = await mp.Payments.CreateAsync(request);

if (result.IsSuccess)
{
    Console.WriteLine($"Pago creado: ID={result.Data.Id}, Status={result.Data.Status}");
    // Status: "approved", "pending", "rejected"
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
    // result.Causes tiene el detalle de las causas
}
```

#### Buscar pagos

```csharp
// Por ID
var payment = await mp.Payments.GetAsync(123456789);

// Por criterios de búsqueda
var results = await mp.Payments.SearchAsync(new PaymentSearchRequest
{
    ExternalReference = "VENTA-001",
    Status = "approved",
    DateCreatedFrom = DateTime.Today.AddDays(-7),
    Limit = 50
});

foreach (var p in results.Data.Results)
{
    Console.WriteLine($"  {p.Id} | {p.Status} | ${p.TransactionAmount}");
}
```

#### Cancelar un pago

```csharp
var result = await mp.Payments.CancelAsync(123456789);
// Solo se pueden cancelar pagos en estado "pending"
```

#### Reembolsar un pago

```csharp
// Reembolso total
var refund = await mp.Payments.RefundAsync(123456789);

// Reembolso parcial
var partialRefund = await mp.Payments.RefundAsync(123456789, amount: 500.00m);

// Ver reembolsos de un pago
var refunds = await mp.Payments.GetRefundsAsync(123456789);
```

---

### 2. Checkout Pro (Preferencias)

Ideal para ventas online donde el cliente paga a través de una página de MercadoPago.

#### Crear una preferencia

```csharp
var preference = await mp.Preferences.CreateAsync(new PreferenceCreateRequest
{
    Items = new List<PreferenceItemRequest>
    {
        new PreferenceItemRequest
        {
            Id = "SKU-001",
            Title = "Notebook Lenovo",
            Description = "Notebook Lenovo ThinkPad 14 pulgadas",
            Quantity = 1,
            UnitPrice = 850000m,
            CurrencyId = "ARS"
        },
        new PreferenceItemRequest
        {
            Id = "SKU-002",
            Title = "Mouse inalámbrico",
            Quantity = 2,
            UnitPrice = 15000m,
            CurrencyId = "ARS"
        }
    },
    ExternalReference = "PEDIDO-2024-001",
    Payer = new PreferencePayerRequest
    {
        Name = "Juan",
        Surname = "Pérez",
        Email = "juan@email.com"
    },
    BackUrls = new BackUrlsRequest
    {
        Success = "https://mitienda.com/success",
        Failure = "https://mitienda.com/failure",
        Pending = "https://mitienda.com/pending"
    },
    AutoReturn = "approved",
    NotificationUrl = "https://mitienda.com/webhooks/mp",
    PaymentMethods = new PreferencePaymentMethodsRequest
    {
        Installments = 12,
        ExcludedPaymentTypes = new List<PaymentMethodIdRequest>
        {
            new PaymentMethodIdRequest { Id = "ticket" }  // excluir pagos en efectivo
        }
    },
    BinaryMode = false,  // false = permite pending; true = solo approved/rejected
    Expires = true,
    ExpirationDateTo = DateTime.Now.AddHours(24)
});

if (preference.IsSuccess)
{
    // URL para sandbox (testing)
    string checkoutUrl = preference.Data.SandboxInitPoint;

    // URL para producción
    // string checkoutUrl = preference.Data.InitPoint;

    Console.WriteLine($"Checkout URL: {checkoutUrl}");
    // Abrir en navegador: Process.Start(checkoutUrl);
}
```

#### Obtener una preferencia

```csharp
var pref = await mp.Preferences.GetAsync("preference-id-xxx");
```

---

### 3. Órdenes

La API de Órdenes es la nueva API unificada de MercadoPago (`/v1/orders`).

```csharp
// Crear orden
var order = await mp.Orders.CreateAsync(new OrderCreateRequest
{
    Type = "online",
    TotalAmount = "1500.00",
    ExternalReference = "ORDER-001",
    Description = "Pedido de prueba"
});

// Obtener orden
var orderDetail = await mp.Orders.GetAsync("order-id");

// Cancelar orden
var cancelled = await mp.Orders.CancelAsync("order-id");

// Reembolsar orden
var refunded = await mp.Orders.RefundAsync("order-id");
```

---

### 4. Clientes

#### CRUD de clientes

```csharp
// Crear
var customer = await mp.Customers.CreateAsync(new CustomerCreateRequest
{
    Email = "cliente@email.com",
    FirstName = "María",
    LastName = "García",
    Phone = new PhoneRequest { AreaCode = "11", Number = "55551234" },
    Identification = new IdentificationRequest { Type = "DNI", Number = "33445566" }
});

// Buscar por email
var found = await mp.Customers.SearchAsync(new CustomerSearchRequest
{
    Email = "cliente@email.com"
});

// Obtener por ID
var detail = await mp.Customers.GetAsync("customer-id");

// Actualizar
await mp.Customers.UpdateAsync("customer-id", new CustomerCreateRequest
{
    FirstName = "María José"
});

// Eliminar
await mp.Customers.DeleteAsync("customer-id");
```

#### Tarjetas guardadas

```csharp
// Guardar tarjeta tokenizada
var card = await mp.Customers.SaveCardAsync("customer-id", new CardCreateRequest
{
    Token = "card_token_from_frontend"
});

// Listar tarjetas del cliente
var cards = await mp.Customers.GetCardsAsync("customer-id");

// Eliminar tarjeta
await mp.Customers.DeleteCardAsync("customer-id", "card-id");
```

---

### 5. Sucursales (Stores)

Representan las sucursales físicas del negocio en MercadoPago.

```csharp
// Crear sucursal
var store = await mp.Stores.CreateAsync(new StoreCreateRequest
{
    Name = "Sucursal Centro",
    ExternalId = "SUC-001",
    Location = new StoreLocationRequest
    {
        StreetName = "Av. Corrientes",
        StreetNumber = "1234",
        CityName = "Buenos Aires",
        StateName = "CABA",
        Latitude = -34.603,
        Longitude = -58.381
    },
    BusinessHours = new BusinessHoursRequest
    {
        Monday = new List<HourBlock>
        {
            new HourBlock { Open = "08:00", Close = "20:00" }
        }
        // ... otros días
    }
});

// Listar sucursales
var stores = await mp.Stores.SearchAsync();

// Actualizar
await mp.Stores.UpdateAsync("store-id", new StoreCreateRequest { Name = "Nuevo nombre" });

// Eliminar
await mp.Stores.DeleteAsync("store-id");
```

---

### 6. Cajas / POS

Puntos de venta asociados a sucursales, con código QR asignado.

```csharp
// Crear caja
var pos = await mp.Cashiers.CreateAsync(new PosCreateRequest
{
    Name = "Caja 1",
    ExternalId = "CAJA-001",
    StoreId = "store-id",
    FixedAmount = false  // false = monto dinámico por QR
});

// Listar cajas
var allPos = await mp.Cashiers.SearchAsync();

// La respuesta incluye la imagen del QR
if (pos.IsSuccess)
{
    string qrImageUrl = pos.Data.Qr?.Image;
    Console.WriteLine($"QR: {qrImageUrl}");
}

// Eliminar caja
await mp.Cashiers.DeleteAsync(posId);
```

---

### 7. Órdenes QR

Para cobrar con QR en cajas (modelo **atendido/dinámico**): el sistema envía la orden a la caja y el cliente la paga con la app de MercadoPago.

```csharp
// Crear orden QR en una caja
var qrOrder = await mp.QrCodes.CreateOrderAsync(
    userId: "123456789",          // User ID de MP
    externalPosId: "CAJA-001",    // External ID de la caja
    request: new QrOrderRequest
    {
        ExternalReference = "VENTA-POS-001",
        Title = "Venta en mostrador",
        TotalAmount = 5000m,
        Items = new List<QrItemRequest>
        {
            new QrItemRequest
            {
                SkuNumber = "SKU-001",
                Title = "Producto A",
                Quantity = 2,
                UnitPrice = 2000m,
                TotalAmount = 4000m,
                UnitMeasure = "unit"
            },
            new QrItemRequest
            {
                SkuNumber = "SKU-002",
                Title = "Producto B",
                Quantity = 1,
                UnitPrice = 1000m,
                TotalAmount = 1000m,
                UnitMeasure = "unit"
            }
        },
        NotificationUrl = "https://mitienda.com/webhooks/mp"
    });

// Eliminar orden QR de la caja (limpiar)
await mp.QrCodes.DeleteOrderAsync("123456789", "CAJA-001");
```

**Flujo típico de cobro con QR:**
1. Operador del ERP arma la venta
2. Se llama `CreateOrderAsync` → la orden aparece en la caja
3. El cliente escanea el QR con la app de MP y paga
4. MP envía un webhook de pago completado
5. El ERP procesa la notificación y marca la venta como pagada

---

### 8. Terminales Point Smart

```csharp
// Listar terminales
var devices = await mp.PointDevices.ListDevicesAsync();

// Crear intent de pago (envía el cobro a la terminal)
var intent = await mp.PointDevices.CreatePaymentIntentAsync(
    deviceId: "DEVICE-ID",
    request: new PointPaymentIntentRequest
    {
        Amount = 5000m,
        Description = "Venta en punto",
        ExternalReference = "VENTA-POINT-001",
        Installments = 1,
        PaymentType = "credit_card",
        PrintOnTerminal = true,
        TicketNumber = "T-001"
    });

// Consultar estado del intent
var status = await mp.PointDevices.GetPaymentIntentStatusAsync(intent.Data.Id);

// Cancelar intent
await mp.PointDevices.CancelPaymentIntentAsync("DEVICE-ID", intent.Data.Id);
```

---

### 9. Suscripciones

#### Crear un plan

```csharp
var plan = await mp.Subscriptions.CreatePlanAsync(new SubscriptionPlanCreateRequest
{
    Reason = "Plan Premium Mensual",
    AutoRecurring = new AutoRecurringRequest
    {
        Frequency = 1,
        FrequencyType = "months",
        TransactionAmount = 5000m,
        CurrencyId = "ARS",
        Repetitions = 12  // 12 meses, null = indefinido
    },
    BackUrl = "https://mitienda.com/suscripcion"
});

// El plan tiene un InitPoint para que el cliente se suscriba
string subscribeUrl = plan.Data.InitPoint;
```

#### Crear una suscripción

```csharp
var subscription = await mp.Subscriptions.CreateSubscriptionAsync(
    new SubscriptionCreateRequest
    {
        PlanId = plan.Data.Id,
        PayerEmail = "cliente@email.com",
        ExternalReference = "SUB-CLIENTE-001"
    });

// Pausar suscripción
await mp.Subscriptions.UpdateSubscriptionAsync(subscription.Data.Id,
    new SubscriptionUpdateRequest { Status = "paused" });

// Reactivar
await mp.Subscriptions.UpdateSubscriptionAsync(subscription.Data.Id,
    new SubscriptionUpdateRequest { Status = "authorized" });
```

---

### 10. Marketplace (Split Payments)

Para modelos donde un marketplace cobra la venta y distribuye el pago entre vendedores.

```csharp
// Pago con comisión de marketplace
var payment = await mp.Marketplace.CreatePaymentAsync(new PaymentCreateRequest
{
    TransactionAmount = 10000m,
    Token = "card_token",
    PaymentMethodId = "visa",
    Installments = 1,
    ApplicationFee = 500m,  // comisión del marketplace
    Payer = new PayerRequest { Email = "comprador@email.com" }
});

// Preferencia con fee de marketplace
var preference = await mp.Marketplace.CreatePreferenceAsync(new PreferenceCreateRequest
{
    Items = new List<PreferenceItemRequest>
    {
        new PreferenceItemRequest
        {
            Title = "Producto vendedor",
            Quantity = 1,
            UnitPrice = 10000m
        }
    },
    MarketplaceFee = 1000m  // comisión del marketplace
});
```

---

### 11. Recursos del sitio (Site)

Endpoints de catálogo/referencia que proveen datos dinámicos de MercadoPago.

```csharp
// Tipos de documento (DNI, CUIT, etc.)
var idTypes = await mp.Site.GetIdentificationTypesAsync();
foreach (var t in idTypes.Data)
    Console.WriteLine($"{t.Id}: {t.Name} ({t.MinLength}-{t.MaxLength} chars)");

// Métodos de pago disponibles
var methods = await mp.Site.GetPaymentMethodsAsync();
foreach (var m in methods.Data)
    Console.WriteLine($"{m.Id}: {m.Name} ({m.PaymentTypeId}) - {m.Status}");

// Cuotas para un monto
var installments = await mp.Site.GetInstallmentsAsync(amount: 50000m);
foreach (var info in installments.Data)
    foreach (var cost in info.PayerCosts)
        Console.WriteLine($"  {cost.Installments}x ${cost.InstallmentAmount:N2} = ${cost.TotalAmount:N2}");

// Emisores de tarjeta
var issuers = await mp.Site.GetCardIssuersAsync("visa");
```

---

### 12. Merchant Orders

Órdenes de comercio que agrupan pagos y shipments.

```csharp
// Crear
var order = await mp.MerchantOrders.CreateAsync(new MerchantOrderCreateRequest
{
    ExternalReference = "ORDEN-001",
    Items = new List<MerchantOrderItem>
    {
        new MerchantOrderItem { Title = "Producto", Quantity = 1, UnitPrice = 5000m }
    }
});

// Obtener
var detail = await mp.MerchantOrders.GetAsync(orderId);

// Buscar
var results = await mp.MerchantOrders.SearchAsync(externalReference: "ORDEN-001");
```

---

### 13. Cuenta y Balance (Account)

```csharp
// Info del usuario autenticado
var user = await mp.Account.GetUserInfoAsync();
Console.WriteLine($"Hola {user.Data.FirstName} ({user.Data.Email})");

// Balance (requiere permisos de marketplace)
var balance = await mp.Account.GetAccountBalanceAsync();
Console.WriteLine($"Disponible: ${balance.Data.AvailableBalance:N2}");
```

---

### 14. Contracargos (Chargebacks)

```csharp
var chargeback = await mp.Chargebacks.GetAsync("chargeback-id");
Console.WriteLine($"Monto: {chargeback.Data.Amount} - Estado: {chargeback.Data.Status}");
```

---

### 15. Desembolsos (Disbursements)

Para pagos avanzados con split (marketplace).

```csharp
// Obtener reembolsos de un desembolso
var refunds = await mp.Disbursements.GetRefundsAsync(
    advancedPaymentId: 123, disbursementId: 456);

// Crear reembolso
var refund = await mp.Disbursements.RefundAsync(123, 456,
    new DisbursementRefundRequest { Amount = 1000m });
```

---

## Webhooks

El wrapper incluye un **servidor HTTP embebido** para recibir notificaciones de MercadoPago en tiempo real.

### Configurar el listener

```csharp
var listener = mp.ConfigureWebhookListener(
    port: 5100,
    path: "/webhooks/mp",
    secret: "tu_webhook_secret"  // se obtiene de la config de webhooks en MP
);

// Suscribirse a eventos
listener.OnNotificationReceived += (sender, args) =>
{
    Console.WriteLine($"Webhook: {args.Notification.Type} - {args.Notification.Data.Id}");
    Console.WriteLine($"Válido: {args.IsValid}");
    Console.WriteLine($"JSON: {args.RawJson}");
};

listener.OnPaymentNotification += (sender, args) =>
{
    // Procesar pago: obtener detalles con mp.Payments.GetAsync(...)
    var paymentId = long.Parse(args.Notification.Data.Id);
    Console.WriteLine($"Pago recibido: {paymentId}");
};

listener.OnOrderNotification += (sender, args) =>
{
    Console.WriteLine($"Orden actualizada: {args.Notification.Data.Id}");
};

// Iniciar
listener.Start();

// Detener (también se detiene con Dispose)
listener.Stop();
```

### Requisitos para webhooks

1. **En desarrollo local**: Usar [Cloudflare Tunnel](https://developers.cloudflare.com/cloudflare-one/connections/connect-apps/) o [ngrok](https://ngrok.com/) para exponer el puerto local
2. **Permisos**: Si ejecutás en Windows sin ser administrador, registrar el prefijo URL:
   ```
   netsh http add urlacl url=http://+:5100/webhooks/mp/ user=EVERYONE
   ```
3. **Configurar en MP**: En la app de MercadoPago Developers, ir a la sección de Webhooks y configurar la URL de notificación (ej: `https://tu-dominio.com/webhooks/mp/`)

### Validación HMAC

El listener valida automáticamente la firma `x-signature` de los webhooks si se proporcionó un `secret`. Si el webhook no pasa la validación, `args.IsValid` será `false`.

---

## Manejo de errores

### Respuestas de la API

Cada llamada retorna un `MpApiResponse<T>`:

```csharp
var result = await mp.Payments.CreateAsync(request);

if (result.IsSuccess)
{
    // result.Data contiene la respuesta tipada
    var payment = result.Data;
}
else
{
    // Información del error
    Console.WriteLine($"Status: {result.StatusCode}");
    Console.WriteLine($"Error: {result.ErrorMessage}");
    Console.WriteLine($"Código: {result.ErrorCode}");
    Console.WriteLine($"Causas: {string.Join(", ", result.Causes)}");
    Console.WriteLine($"JSON: {result.RawJson}");
}
```

### Excepciones tipadas

Si preferís trabajar con excepciones, usá `ThrowIfError`:

```csharp
try
{
    var result = await mp.Payments.CreateAsync(request);
    mp.Http.ThrowIfError(result);  // lanza excepción si no es 2xx
    var payment = result.Data;
}
catch (MpAuthenticationException)
{
    // Token inválido o expirado (401/403)
}
catch (MpValidationException ex)
{
    // Datos inválidos (400)
    Console.WriteLine($"Causas: {string.Join(", ", ex.Causes)}");
}
catch (MpNotFoundException)
{
    // Recurso no encontrado (404)
}
catch (MpRateLimitException)
{
    // Demasiadas solicitudes (429)
}
catch (MpServerException)
{
    // Error del servidor de MP (500+)
}
catch (MpException ex)
{
    // Cualquier otro error
    Console.WriteLine($"Error {ex.StatusCode}: {ex.Message}");
}
```

---

## Logging

El wrapper usa **Serilog** para logging. Configurá Serilog antes de crear el cliente:

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()           // Debug para ver request/response HTTP
    .MinimumLevel.Information()     // Information para operación normal
    .WriteTo.Console()
    .WriteTo.File("logs/mp_.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var mp = new MpWrapperClient(config);  // usa Log.Logger automáticamente

// O pasar un logger personalizado:
var mp = new MpWrapperClient(config, logger: myCustomLogger);
```

### Niveles de log

| Nivel | Qué se registra |
|---|---|
| `Debug` | Request/response HTTP completos (body JSON) |
| `Information` | Inicialización, webhooks recibidos |
| `Warning` | Reintentos, timeout, firmas inválidas |
| `Error` | Errores de API, excepciones |

---

## Referencia de configuración

### MpWrapperConfig.Builder

| Método | Tipo | Default | Descripción |
|---|---|---|---|
| `WithAccessToken(string)` | **Obligatorio** | — | Token de acceso de MP |
| `WithPublicKey(string)` | Opcional | `""` | Clave pública (para frontend) |
| `WithEnvironment(MpEnvironment)` | Opcional | `Sandbox` | `Sandbox` o `Production` |
| `WithCountry(string)` | Opcional | `"AR"` | País ISO (AR, BR, MX, etc.) |
| `WithBaseUrl(string)` | Opcional | `api.mercadopago.com` | URL base de la API |
| `WithMaxRetries(int)` | Opcional | `2` | Reintentos en errores transitorios |
| `WithTimeout(int)` | Opcional | `30` | Timeout HTTP en segundos |
| `WithUserAgent(string)` | Opcional | `MpWrapperClient/1.0` | User-Agent personalizado |

### MpWrapperClient constructor

| Parámetro | Tipo | Default | Descripción |
|---|---|---|---|
| `config` | **Obligatorio** | — | Configuración del wrapper |
| `userId` | Opcional | `"me"` | User ID de MP (para Stores) |
| `logger` | Opcional | `Log.Logger` | Logger de Serilog |

---

## Preguntas frecuentes

### ¿Puedo usar esta librería en .NET 6/8/9?

Sí. .NET Framework 4.8 es compatible hacia adelante. Las aplicaciones .NET modernas pueden referenciar DLLs de net48.

### ¿Cómo obtengo mi User ID de MercadoPago?

Llamá a `TestConnectionAsync()` — la respuesta incluye el campo `id` de tu cuenta.

### ¿Qué pasa si MercadoPago devuelve un error transitorio?

El `MpHttpClient` reintenta automáticamente (según `MaxRetries`) con backoff exponencial en:
- Errores HTTP 429 (rate limit)
- Errores HTTP 500+ (errores del servidor)
- Timeouts de conexión

No reintenta en errores 4xx (excepto 429) porque son errores de validación.

### ¿Es thread-safe el `MpWrapperClient`?

Sí. Podés compartir una instancia entre múltiples hilos. El `HttpClient` subyacente es thread-safe y se reutiliza.

### ¿Cómo paso a producción?

1. Cambiar `WithEnvironment(MpEnvironment.Production)`
2. Usar el Access Token de **producción** (no el de test)
3. Verificar que tus webhooks apunten a una URL pública con HTTPS
4. Asegurar que tu app de MP esté en modo "activa" en el panel de developers

### ¿Puedo personalizar los headers HTTP?

Para casos avanzados, accedé al `MpHttpClient` directamente vía `mp.Http` y usá los métodos `GetAsync`, `PostAsync`, etc. con endpoints personalizados.

---

> **Nota**: Esta librería interactúa directamente con la [API REST de MercadoPago](https://www.mercadopago.com.ar/developers/es/reference). Para detalles sobre cada endpoint, campos opcionales y reglas de negocio, consultá la [documentación oficial](https://www.mercadopago.com.ar/developers/es/docs).
