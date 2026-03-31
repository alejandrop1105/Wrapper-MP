# Walkthrough — Fase 1: Mandatorios Críticos

## Resumen

Se implementaron las **7 correcciones** para cerrar los gaps mandatorios detectados en el checklist de homologación de MercadoPago.

---

## Cambios Realizados

### 1. `platform_id` en órdenes

render_diffs(file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Models/Orders/OrderModels.cs)

- Campo `PlatformId` en `OrderCreateRequest`
- Clases `OrderConfigRequest` y `OrderPointConfigRequest` para `config.point`
- `PlatformId` también en [MpWrapperConfig.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Configuration/MpWrapperConfig.cs) con `WithPlatformId()`

### 2. API de cambio de modo de terminal

render_diffs(file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Services/PointDeviceService.cs)

- Método `ChangeOperatingModeAsync(deviceId, "PDV" | "STANDALONE")`
- Nuevo `PatchAsync<T>()` en [MpHttpClient.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Http/MpHttpClient.cs)
- Interfaz actualizada en [IPointDeviceService.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Interfaces/IPointDeviceService.cs)

### 3. Eventos de webhook mejorados

render_diffs(file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Webhooks/WebhookListener.cs)

- `OnOrderCancelled` — cancelación desde la terminal
- `OnActionRequired` — el operador debe registrar manualmente la operación

### 4. Nuevos helpers

| Archivo | Propósito |
|---|---|
| [AmountFormatter.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Helpers/AmountFormatter.cs) | Formatea montos por país (CL/CO sin decimales, resto con 2 decimales) |
| [ExternalReferenceValidator.cs](file:///d:/DESARROLLO/ANTIGRAVITY/MP%20Wrapper/src/MercadoPago.Wrapper/Helpers/ExternalReferenceValidator.cs) | Warning si `external_reference` contiene PII (email, teléfono, DNI) |

---

## Verificación

| Proyecto | Resultado |
|---|---|
| `MercadoPago.Wrapper` | ✅ Build exitoso |
| `MercadoPago.Demo.WinForms` | ✅ Build exitoso |
| `MercadoPago.Wrapper.Tests` | ✅ Build exitoso (sin tests existentes) |

```
Compilación realizado correctamente en 1,3s
```

---

## Impacto en el Checklist Mandatorio

| Ítem | Antes | Después |
|---|---|---|
| ID de plataforma | ❌ | ✅ |
| Config Point (print/ticket) | ❌ | ✅ |
| API cambio de modo | ❌ | ✅ |
| Cancelación orden en terminal | ❌ | ✅ |
| Status action_required | ❌ | ✅ |
| Información sensible (PII) | ⚠️ | ✅ |
| Formato del monto | ⚠️ | ✅ |

**Mandatorios resueltos: 15/15 ✅**
