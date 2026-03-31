# Análisis de Homologación MercadoPago — MP Wrapper

## Contexto

MercadoPago envió el checklist de homologación (`checklist_homologator_2026-01-14.pdf`) para validar la integración realizada en el proyecto **MP Wrapper**. Este documento analiza el cumplimiento de cada ítem contra el código fuente actual del proyecto.

**Producto evaluado:** Integrated Point - OU (Orders Unificadas)

---

## Resumen Ejecutivo

| Categoría | Total | ✅ Cumple | ⚠️ Parcial | ❌ No cumple |
|---|---|---|---|---|
| **Mandatorios** | 15 | 7 | 2 | 6 |
| **Opcionales** | 17 | 8 | 3 | 6 |
| **Total** | 32 | 15 | 5 | 12 |

> [!IMPORTANT]
> De los 15 ítems mandatorios, **6 no están implementados** y **2 están parcialmente cubiertos**. Estos deben resolverse antes de la entrevista de homologación.

---

## Matriz de Cumplimiento Completa

### 🔴 ÍTEMS MANDATORIOS (Verdadero)

| # | Nombre | Estado | Detalle |
|---|---|---|---|
| 1 | **Renovación de Tokens OAuth** | ❌ No cumple | No existe proceso automático de refresh de `access_token`. La config actual acepta un token fijo en `MpWrapperConfig.Builder.WithAccessToken()`. No hay lógica de detección de expiración ni regeneración. |
| 2 | **ID de plataforma** | ❌ No cumple | No existe el campo `platform_id` en `OrderCreateRequest` ni en ningún otro modelo. El HTTP client no agrega este header/parámetro a las requests. |
| 3 | **Uso de Logs** | ✅ Cumple | Logging completo con Serilog en [MpHttpClient.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Http/MpHttpClient.cs), [WebhookListener.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Webhooks/WebhookListener.cs), y [MpWrapperClient.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/MpWrapperClient.cs). Logs de request/response, errores y webhooks. |
| 4 | **Referencia externa** | ✅ Cumple | `OrderCreateRequest.ExternalReference` existe en [OrderModels.cs:19](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Models/Orders/OrderModels.cs#L19). |
| 5 | **Información sensible** | ⚠️ Parcial | El wrapper no impide que se envíe PII en `external_reference`, pero tampoco lo inyecta. **Es responsabilidad del consumidor**. Falta documentación/validación que advierta sobre esto. |
| 6 | **Formato del monto** | ⚠️ Parcial | `OrderTransactionPaymentRequest.Amount` es `string` en [OrderModels.cs:46](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Models/Orders/OrderModels.cs#L46), lo que permite formateo flexible. Pero `PointPaymentIntentRequest.Amount` es `decimal` en [PointDeviceModels.cs:47](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Models/PointDevice/PointDeviceModels.cs#L47) sin formateo por país. Falta utility de formateo automático según `Country`. |
| 7 | **Llave de idempotencia** | ✅ Cumple | `MpHttpClient.ExecuteAsync()` auto-genera un `X-Idempotency-Key` en cada POST si no se provee uno. Ver [MpHttpClient.cs:168-172](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Http/MpHttpClient.cs#L168-L172). |
| 8 | **Notificaciones webhooks** | ✅ Cumple | [WebhookListener.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Webhooks/WebhookListener.cs) implementa servidor HTTP embebido con eventos `OnNotificationReceived`, `OnPaymentNotification`, `OnOrderNotification`, validación HMAC-SHA256. |
| 9 | **Lógica de backup de notificaciones** | ✅ Cumple | `OrderService.GetAsync(id)` en [OrderService.cs:28-33](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Services/OrderService.cs#L28-L33) permite consultar el estado de una orden por ID como fallback. |
| 10 | **Pago aprobado** | ✅ Cumple | La API `OrderService.CreateAsync()` + webhook listener permiten el flujo completo de pago aprobado. Documentado en la guía de uso. |
| 11 | **Pago rechazado** | ✅ Cumple | Mismo flujo que pago aprobado, el wrapper procesa cualquier status de respuesta. |
| 12 | **Cancelación de orden vía API** | ✅ Cumple | `OrderService.CancelAsync(id)` en [OrderService.cs:35-41](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Services/OrderService.cs#L35-L41). |
| 13 | **API cambio de modo** | ❌ No cumple | No existe método `ChangeOperatingModeAsync()` en `PointDeviceService`. El modelo `PointDeviceResponse` sí tiene `operating_mode` pero falta la API PUT para cambiarlo. |
| 14 | **Cancelación de orden en la terminal** | ❌ No cumple | El `WebhookListener` no diferencia notificaciones de cancelación en terminal. No hay lógica específica para procesar el webhook de cancelación vs otros estados. |
| 15 | **Status de acción requerida** | ❌ No cumple | No existe manejo del status `action_required` en ninguna parte del código. No hay documentación ni modelo que contemple este estado. |

---

### 🟡 ÍTEMS OPCIONALES (Falso)

| # | Nombre | Estado | Detalle |
|---|---|---|---|
| 16 | Notificaciones OAuth | ❌ No cumple | No hay suscripción a tópico `mp-connect` para vinculaciones/desvinculaciones OAuth. |
| 17 | Uso del parámetro state | ❌ No cumple | No existe flujo OAuth implementado, por lo tanto no se usa el parámetro `state`. |
| 18 | Impresión de ticket | ⚠️ Parcial | `PointPaymentIntentRequest.PrintOnTerminal` existe, pero en la API de Orders no existe el campo `config.point.print_on_terminal` para el modelo unificado. |
| 19 | Manual de implementación | ✅ Cumple | Existe [guia-de-uso.md](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/docs/guia-de-uso.md) con documentación detallada de todos los módulos. |
| 20 | Devoluciones | ✅ Cumple | `OrderService.RefundAsync()` en [OrderService.cs:43-52](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Services/OrderService.cs#L43-L52). |
| 21 | Administración de sucursales | ✅ Cumple | `StoreService` completo con CRUD en [StoreAndPosServices.cs:12-58](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Services/StoreAndPosServices.cs#L12-L58). |
| 22 | Keys Mercado Pago | ✅ Cumple | `MpWrapperConfig.Builder` permite configurar `AccessToken` y `PublicKey`. Ver [MpWrapperConfig.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Configuration/MpWrapperConfig.cs). |
| 23 | Credenciales centralizadas | ✅ Cumple | La config se inyecta una sola vez en `MpWrapperClient` y se comparte por todos los servicios. Las credenciales están centralizadas en el cliente. |
| 24 | Manual Operativo | ⚠️ Parcial | La guía de uso cubre la implementación técnica, pero falta un manual operativo orientado al usuario final (operador de caja). |
| 25 | Buscar terminales por API | ✅ Cumple | `PointDeviceService.ListDevicesAsync()` con filtros por `storeId` y `posId`. Ver [PointDeviceService.cs:20-33](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Services/PointDeviceService.cs#L20-L33). |
| 26 | Administración de cajas | ✅ Cumple | `CashierService` completo con CRUD en [StoreAndPosServices.cs:61-103](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Services/StoreAndPosServices.cs#L61-L103). |
| 27 | Notificaciones de eventos de terminal | ❌ No cumple | No existe suscripción a notificaciones por email de eventos de terminal. |
| 28 | ID de tienda y de PDV | ✅ Cumple | `PointDeviceResponse` incluye `StoreId` y `PosId`. Ver [PointDeviceModels.cs:15-22](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Models/PointDevice/PointDeviceModels.cs#L15-L22). |
| 29 | Tiempo de Expiración | ⚠️ Parcial | `OrderCreateRequest.ExpirationTime` existe en [OrderModels.cs:27-28](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Models/Orders/OrderModels.cs#L27-L28), pero no se promueve en la documentación ni en ejemplos. |
| 30 | Botón actualizar | ❌ No cumple | No hay UI ni documentación que indique al operador presionar "actualizar" en el dispositivo. |
| 31 | Pago aprobado + reembolso | ❌ No cumple | Aunque `RefundAsync` existe, no hay flujo documentado/testeado de pago + reembolso posterior con verificación de consistencia de estados. |
| 32 | Número de ticket | ❌ No cumple | `PointPaymentIntentRequest.TicketNumber` existe para la API Point legacy, pero no existe `config.point.ticket_number` en el modelo de Orders unificada. |

---

## Análisis de Gaps Críticos

### Gap 1: No hay OAuth / Token Refresh
La integración actual solo acepta un token estático. Para producción OAuth, se necesita:
- Flujo de autorización OAuth inicial
- Almacenamiento seguro de `refresh_token`
- Proceso automático de renovación previo a expiración
- Manejo de desvinculaciones

### Gap 2: Falta `platform_id`
MercadoPago asigna un `platform_id` único al integrador homologado. Este debe enviarse en cada creación de orden. Es un campo nuevo que falta en `OrderCreateRequest`.

### Gap 3: No hay API de cambio de modo de operación
La API `PATCH /point/integration-api/devices/{device_id}` para cambiar el `operating_mode` del terminal no está implementada.

### Gap 4: No hay manejo de `action_required`
Cuando una orden queda en estado `action_required`, el sistema debe notificar al operador para que registre manualmente la operación. No hay lógica ni documentación para este caso.

### Gap 5: Cancelación de orden en terminal no se diferencia
El webhook listener no emite un evento específico para cancelaciones en terminal. No se actualiza el status diferenciándolo de otros estados.

### Gap 6: Modelo de Orders no alineado con Point OU
El `OrderCreateRequest` actual no tiene campos `config.point` (como `print_on_terminal` y `ticket_number`) que son propios del modelo Point OU. El producto evaluado es Point con Orders Unificadas, y estos campos deben estar en la orden, no en la API legacy de Point.

---

## Plan de Implementación por Fases

### Fase 1 — Mandatorios Críticos (Bloqueantes para homologación)
**Prioridad:** 🔴 Alta | **Estimación:** 3-4 días

| Cambio | Archivo(s) afectado(s) |
|---|---|
| Agregar campo `platform_id` a `OrderCreateRequest` | [OrderModels.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Models/Orders/OrderModels.cs) |
| Agregar `config.point` (print_on_terminal, ticket_number) a `OrderCreateRequest` | [OrderModels.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Models/Orders/OrderModels.cs) |
| Implementar `ChangeOperatingModeAsync()` en `PointDeviceService` | [PointDeviceService.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Services/PointDeviceService.cs), [IPointDeviceService.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Interfaces/IPointDeviceService.cs) |
| Agregar evento `OnOrderCancelled` al WebhookListener para cancelaciones en terminal | [WebhookListener.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Webhooks/WebhookListener.cs) |
| Documentar/implementar manejo de `action_required` como guía para el consumidor | [WebhookListener.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Webhooks/WebhookListener.cs), docs |
| Validación de no-PII en external_reference (warning en docs o validación leve) | [OrderModels.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Models/Orders/OrderModels.cs), docs |
| Formateo de monto según país | [MpWrapperConfig.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Configuration/MpWrapperConfig.cs), utility nuevo |

---

### Fase 2 — OAuth y Token Management
**Prioridad:** 🔴 Alta | **Estimación:** 4-5 días

| Cambio | Archivo(s) afectado(s) |
|---|---|
| Crear `OAuthService` con flujo de autorización y refresh token | [NEW] `Services/OAuthService.cs`, `Interfaces/IOAuthService.cs` |
| Crear modelo `OAuthTokenResponse` | [NEW] `Models/OAuth/OAuthModels.cs` |
| Agregar `WithClientId()`, `WithClientSecret()`, `WithRefreshToken()` al Builder | [MpWrapperConfig.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Configuration/MpWrapperConfig.cs) |
| Implementar auto-refresh del access token con timer/scheduler | [MpHttpClient.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Http/MpHttpClient.cs) o servicio dedicado |
| Actualizar `MpWrapperClient` para exponer OAuth service | [MpWrapperClient.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/MpWrapperClient.cs) |

---

### Fase 3 — Opcionales Recomendados (Mejoran la evaluación)
**Prioridad:** 🟡 Media | **Estimación:** 2-3 días

| Cambio | Archivo(s) afectado(s) |
|---|---|
| Agregar `expiration_time` a ejemplos en guía de uso | [guia-de-uso.md](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/docs/guia-de-uso.md) |
| Crear manual operativo para operadores de caja (no técnico) | [NEW] `docs/manual-operativo.md` |
| Documentar flujo pago + reembolso con verificación de consistencia | [guia-de-uso.md](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/docs/guia-de-uso.md) |
| Agregar evento suscripción a notificaciones OAuth (tópico `mp-connect`) | Documentación y WebhookListener |

---

### Fase 4 — Testing y Validación
**Prioridad:** 🟡 Media | **Estimación:** 2-3 días

| Cambio | Archivo(s) afectado(s) |
|---|---|
| Tests unitarios para `OrderCreateRequest` con `platform_id` y `config.point` | [NEW] Tests en proyecto `MercadoPago.Wrapper.Tests` |
| Tests para `ChangeOperatingModeAsync` | [NEW] Tests en proyecto `MercadoPago.Wrapper.Tests` |
| Tests para OAuth flow y token refresh | [NEW] Tests en proyecto `MercadoPago.Wrapper.Tests` |
| Tests para formateo de monto por país | [NEW] Tests en proyecto `MercadoPago.Wrapper.Tests` |
| Test E2E: flujo de pago aprobado completo con webhook | Manual con terminal de test |
| Test E2E: flujo de pago rechazado completo con webhook | Manual con terminal de test |

---

## Verificación

### Tests Automatizados
El proyecto de tests está vacío actualmente. Se crearán tests unitarios usando el framework de la solución existente:
```
dotnet test tests/MercadoPago.Wrapper.Tests/MercadoPago.Wrapper.Tests.csproj
```

### Verificación Manual
- **Flujo de pago aprobado/rechazado**: Requiere terminal Point Smart conectada y credenciales de sandbox. Ejecutar el Demo WinForms y probar:
  1. Crear orden → verificar que llega al dispositivo
  2. Pagar en la terminal → verificar webhook recibido
  3. Verificar status en el sistema
- **OAuth**: Probar flujo de autorización con app de test en MP Developers
- **Cambio de modo**: Verificar con `PATCH` a la API que el modo del terminal cambia

---

## Recomendaciones para la Entrevista

1. **Llevar los flujos de Fase 1 implementados** — son los bloqueantes
2. **Mostrar los logs** — MP valora que la integración tenga trazabilidad completa
3. **Tener el `platform_id`** listo — consultar a MP cuál es el valor asignado antes de la entrevista
4. **Preparar una demo de pago completo** incluyendo aprobado, rechazado, cancelación y `action_required`
5. **OAuth puede negociarse** — si la integración es directa (sin marketplace), el token refresh puede ser simplificado
