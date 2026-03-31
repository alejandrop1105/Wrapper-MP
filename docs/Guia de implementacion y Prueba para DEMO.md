# Guía de Implementación y Prueba para DEMO

## Índice

1. [Preparación de la Cuenta en MercadoPago](#1-preparación-de-la-cuenta-en-mercadopago)
2. [Generación de Tokens y Credenciales](#2-generación-de-tokens-y-credenciales)
3. [Configuración en la Plataforma de MP](#3-configuración-en-la-plataforma-de-mp)
4. [Configuración del Demo WinForms](#4-configuración-del-demo-winforms)
5. [Prueba por Tabs (Paso a Paso)](#5-prueba-por-tabs-paso-a-paso)
6. [Troubleshooting Común](#6-troubleshooting-común)
7. [Checklist Final de Homologación](#7-checklist-final-de-homologación)

---

## 1. Preparación de la Cuenta en MercadoPago

### 1.1 Crear cuenta de Developers

1. Ir a [https://www.mercadopago.com.ar/developers/panel](https://www.mercadopago.com.ar/developers/panel)
2. Iniciar sesión con tu cuenta de MercadoPago (la misma que usás para cobrar)
3. Si es la primera vez, aceptar términos y condiciones del programa de desarrolladores

### 1.2 Crear una Aplicación

1. En el panel de Developers, hacer clic en **"Crear aplicación"**
2. Completar:
   - **Nombre**: `Demo ERP Integración` (o el nombre de tu sistema)
   - **Modelo de integración**: Seleccionar **"Checkout Pro"** o **"Pagos en persona"** según corresponda
   - **Tipo de producto**: **Punto de venta presencial (QR o Point)**
3. Guardar. Se generará automáticamente un **App ID**

### 1.3 Crear Cuentas de Prueba (Test Users)

> ⚠️ **IMPORTANTE**: Para probar en sandbox necesitás al menos 2 cuentas de prueba: una vendedora y una compradora.

1. Ir a **"Cuentas de prueba"** en el menú lateral del panel de Developers
2. Crear **cuenta vendedora** (esta será la que reciba pagos)
3. Crear **cuenta compradora** (esta será la que pague)
4. Anotar las credenciales de ambas cuentas

---

## 2. Generación de Tokens y Credenciales

### 2.1 Access Token (Obligatorio)

1. En el panel de tu aplicación, ir a **"Credenciales"**
2. Seleccionar la pestaña **"Credenciales de prueba"** (sandbox) o **"Credenciales de producción"**
3. Copiar el **Access Token** — tiene formato: `APP_USR-XXXX-XXXXXXXXX-XXXXXXXXXXXXXXXXXXXX-XXXXXXXXX`
4. Copiar la **Public Key** — tiene formato: `APP_USR-XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX`

> 📝 Para sandbox, usá las credenciales de la **cuenta vendedora de prueba**, no las de tu cuenta real.

### 2.2 User ID (Obligatorio para QR y Sucursales)

1. Con el Access Token, consultar tu User ID:
   - Podés obtenerlo desde el Demo: Tab **"Cuenta / Recursos"** → botón **"👤 Mi Cuenta"**
   - O via API: `GET https://api.mercadopago.com/users/me`
2. Anotar el campo `id` de la respuesta

### 2.3 Platform ID (Homologación)

El `platform_id` es un identificador asignado por MercadoPago a tu integración. Se obtiene de dos formas:

- **Integración estándar**: Usá el valor genérico correspondiente a tu país:
  - Argentina: no hay valor genérico estándar; solicitarlo a MP
- **Integración personalizada**: Contactar a tu asesor de MercadoPago para que te asigne un `platform_id` específico para tu sistema ERP

> 💡 Si todavía no tenés el `platform_id`, dejá el campo vacío en la configuración. El sistema funcionará igualmente, pero necesitarás completarlo antes de la homologación final.

### 2.4 Credenciales OAuth (Opcional para Homologación)

Si necesitás OAuth (para integraciones marketplace o para auto-refresh de tokens):

1. En tu aplicación de MP Developers, ir a **"Credenciales"**
2. Copiar:
   - **Client ID**: `APP_USR-XXXX` (es el app ID)
   - **Client Secret**: Se muestra debajo del Client ID
3. Para obtener un **Refresh Token**:
   - Ejecutar el flujo de autorización OAuth: `https://auth.mercadopago.com.ar/authorization?client_id={CLIENT_ID}&response_type=code&platform_id=mp&redirect_uri={TU_REDIRECT_URI}`
   - Intercambiar el `code` recibido por un access_token + refresh_token

> 💡 Para pruebas básicas (sandbox), no necesitás OAuth. El Access Token directo es suficiente.

### 2.5 Webhook Secret (Para Validación de Notificaciones)

1. En tu aplicación de MP Developers, ir a **"Webhooks"** o **"Notificaciones IPN"**
2. Configurar la URL de notificación (necesitás un endpoint público, ver sección 3.2)
3. Copiar el **Secret** que genera MercadoPago para validar las firmas HMAC

---

## 3. Configuración en la Plataforma de MP

### 3.1 Sucursales y Cajas (Para QR)

Para que funcione el flujo de venta por QR, necesitás crear al menos:

1. **Una Sucursal** en MercadoPago:
   - Podés crearla desde el Demo (Tab "Sucursales / Cajas") o desde el panel de MP
   - Datos requeridos: nombre, dirección (calle, número, ciudad, provincia, lat/long)
   
   > ⚠️ **CUIDADO con las ciudades**: MercadoPago valida la ciudad contra su catálogo interior. Usá nombres oficiales. Ejemplo:
   > - ✅ `La Plata` (con state_name `Buenos Aires`)
   > - ❌ `Buenos Aires` como city_name (causa error de validación)

2. **Al menos una Caja/POS** vinculada a la sucursal:
   - Se crea automáticamente con un código QR asignado
   - Anotá el `external_id` de la caja (lo necesitás para enviar órdenes)

### 3.2 Webhooks (Para Notificaciones)

Para recibir notificaciones en tu red local necesitás exponer el puerto:

**Opción A — Cloudflare Tunnel (recomendado)**:
```bash
cloudflared tunnel --url http://localhost:5100
```

**Opción B — ngrok**:
```bash
ngrok http 5100
```

Luego configurar la URL pública generada en el panel de MP → Webhooks:
- URL: `https://tu-tunnel-url/webhooks/mp/`
- Eventos: `payment`, `merchant_order`, `point_integration_wh`

### 3.3 Terminales Point Smart (Para Orders Unificadas)

Si tenés un dispositivo Point Smart (dispositivo físico):
1. Vincular el dispositivo a tu cuenta de MercadoPago desde la app de MP o el panel
2. El dispositivo aparecerá automáticamente al listar terminales desde el Demo
3. Asegurate que esté en modo **PDV** para recibir órdenes desde el ERP

> 💡 Si NO tenés dispositivo físico, el tab "Point Smart" te permite probar la creación de órdenes igualmente. Obtendrás errores de dispositivo, pero podrás verificar que la API funciona.

---

## 4. Configuración del Demo WinForms

### 4.1 Iniciar el Demo

```bash
cd d:\DESARROLLO\ANTIGRAVITY\MP Wrapper
dotnet run --project src/MercadoPago.Demo.WinForms/MercadoPago.Demo.WinForms.csproj
```

### 4.2 Tab ⚙ Configuración — Completar Campos

| Campo | Valor | Obligatorio |
|-------|-------|:-----------:|
| **Access Token** | `APP_USR-xxxx...` (de tu cuenta vendedora de prueba) | ✅ |
| **Public Key** | `APP_USR-xxxx...` | Opcional |
| **User ID** | Tu ID numérico de MP (ej: `123456789`) | ✅ para QR |
| **Entorno** | `sandbox` para pruebas, `production` para real | ✅ |
| **País** | `AR` (o el que corresponda) | ✅ |
| **Puerto** | `5100` (o el que prefieras para webhooks) | ✅ |
| **Secreto** | El secret del webhook desde MP Developers | Recomendado |
| **Platform ID** | El ID de plataforma asignado por MP | Para homologación |
| **Client ID** | Solo si usás OAuth | Opcional |
| **Client Secret** | Solo si usás OAuth | Opcional |
| **Refresh Token** | Solo si usás OAuth | Opcional |

1. Completar los campos
2. Hacer clic en **"💾 Guardar"**
3. Hacer clic en **"🔌 Test Conexión"** para verificar que todo esté correcto
4. Si la conexión es exitosa, verás los datos de tu cuenta de MP en el resultado

---

## 5. Prueba por Tabs (Paso a Paso)

### Tab 1: ⚙ Configuración

**Qué probar:**
- [  ] Guardar credenciales → mensaje "Configuración guardada"
- [  ] Test de conexión → muestra datos de la cuenta (User ID, nombre, email)
- [  ] Verificar que la barra inferior diga "🟢 Conectado (Sandbox)"

**Resultado esperado:** Conexión exitosa y datos del usuario visibles.

---

### Tab 2: 🏪 Sucursales / Cajas

**Qué probar (en este orden):**
1. [  ] **Sincronizar Sucursales** → carga las sucursales existentes en MP
2. [  ] **Crear Sucursal** (si no existe):
   - Nombre: `Sucursal Demo`
   - Dirección: calle, número, ciudad, estado, lat/long
   - ⚠️ La ciudad debe coincidir con el catálogo de MP (ej: `CABA` para Capital Federal)
3. [  ] **Crear Caja** vinculada a la sucursal:
   - Nombre: `Caja 1`
   - External ID: `CAJA-DEMO-001`
4. [  ] Verificar que la caja aparece en la lista con su QR asignado

**Resultado esperado:** Sucursal y Caja creadas en MP, QR generado.

**Datos que obtenés para el siguiente tab:**
- `User ID` (ya lo tenías)
- `External POS ID` de la caja creada (ej: `CAJA-DEMO-001`)

---

### Tab 3: 📱 Venta QR Caja

**Qué probar:**
1. [  ] Seleccionar sucursal y caja del combo
2. [  ] Agregar items a la venta (producto, cantidad, precio)
3. [  ] Verificar que la referencia externa genera automáticamente un valor sin PII
4. [  ] **Enviar Orden a Caja** → la orden queda publicada en el QR de esa caja
5. [  ] Desde la **cuenta compradora de prueba** (en el celular con la app de MP):
   - Escanear el QR de la caja
   - Ver el detalle de la orden
   - Pagar
6. [  ] Verificar que llega el webhook de pago aprobado (Tab Webhooks)
7. [  ] **Consultar Pago** por referencia externa o Payment ID
8. [  ] **Eliminar Orden** de la caja (limpiar)

**Resultado esperado:** Flujo completo de venta QR: crear → pagar → notificación webhook.

**Validaciones de homologación demostradas:**
- ✅ `ExternalReferenceValidator` activo (advierte si detecta PII)
- ✅ Formato de monto correcto según país

---

### Tab 4: 📊 Movimientos Caja

**Qué probar:**
1. [  ] Consultar movimientos de la caja después de una venta
2. [  ] Verificar que el pago realizado en el tab anterior aparece

**Resultado esperado:** Historial de transacciones de la caja visible.

---

### Tab 5: 📟 Point Smart

> 💡 Este tab requiere un dispositivo Point Smart físico vinculado a tu cuenta. Si no tenés uno, podés probar la creación de la orden (obtendrás error de dispositivo, pero verificás la estructura del request).

**Qué probar:**
1. [  ] **🔄 Cargar** terminales → lista los dispositivos
2. [  ] Seleccionar una terminal
3. [  ] **Cambiar modo** a PDV (si estaba en STANDALONE)
4. [  ] Completar datos de la orden:
   - Monto, descripción, referencia externa, nro. ticket
   - Verificar ✅ "Imprimir ticket en terminal"
5. [  ] **📤 Crear Orden** → envía la orden a la terminal
   - Verificar que el resultado muestra:
     - ✅ Order ID
     - ✅ `platform_id` (si lo configuraste)
     - ✅ `config.point.print_on_terminal` y `ticket_number`
6. [  ] **🔍 Consultar** → ver el estado actual de la orden
7. [  ] Si hay terminal: el cliente paga en el dispositivo → webhook de aprobación
8. [  ] **🚫 Cancelar** → cancelar una orden activa
9. [  ] **↩ Reembolsar** → reembolsar una orden pagada

**Resultado esperado:** Flujo completo de Orders Unificadas con toda la metadata de homologación.

**Validaciones de homologación demostradas:**
- ✅ `platform_id` incluido en el request
- ✅ `config.point.print_on_terminal` + `ticket_number`
- ✅ `AmountFormatter` (monto formateado por país)
- ✅ `ExternalReferenceValidator` (validación PII)

---

### Tab 6: 📋 Log Operaciones

**Qué probar:**
1. [  ] Verificar que aparecen todas las operaciones realizadas
2. [  ] Cada entrada tiene: tipo, ID externo, monto, request/response JSON

**Resultado esperado:** Historial completo de operaciones con request/response para auditoría.

---

### Tab 7: 🏦 Cuenta / Recursos

**Qué probar:**
1. [  ] **👤 Mi Cuenta** → info del usuario autenticado
2. [  ] **💰 Balance** → balance disponible (puede dar 403 si no es marketplace)
3. [  ] **💳 Métodos Pago** → carga grilla con todos los métodos del país
4. [  ] **🪪 Tipos Doc.** → carga tipos de documento (DNI, CUIL, CUIT, etc.)
5. [  ] **📊 Cuotas** → ingresá un monto y verificá las opciones de cuotas

**Resultado esperado:** Datos de catálogo cargados correctamente.

---

### Tab 8: 💳 Pagos (Checkout Pro)

**Qué probar:**
1. [  ] Completar monto, descripción, referencia externa, email
2. [  ] Verificar que `ExternalReferenceValidator` advierte si usás datos personales
3. [  ] **🌐 Checkout Pro** → crea la preferencia y ofrece abrir en navegador
4. [  ] Pagar desde el navegador con la cuenta compradora de prueba
5. [  ] **🔍 Buscar Pago** por ID o por referencia externa
6. [  ] **↩ Reembolsar** un pago aprobado

**Resultado esperado:** Crear preferencia → pagar → buscar → reembolsar.

---

### Tab 9: 👤 Clientes

**Qué probar:**
1. [  ] **Crear** cliente con email, nombre, teléfono, documento
2. [  ] **Buscar** por email
3. [  ] **Consultar** por ID
4. [  ] **Editar** datos del cliente
5. [  ] **Eliminar** el cliente de prueba

**Resultado esperado:** CRUD completo de clientes operativo.

---

### Tab 10: 🔔 Webhooks

**Qué probar (requiere endpoint público o tunnel configurado):**
1. [  ] **▶ Iniciar** listener → debe decir "🟢 Escuchando en puerto XXXX"
2. [  ] Generar una operación (ej: venta QR o Checkout Pro) para provocar un webhook
3. [  ] Verificar que el webhook aparece en la lista con:
   - Tipo de evento
   - Acción
   - Resource ID
   - Indicador de validación ✅/❌
4. [  ] Si llega una cancelación: verificar badge 🚫 en la lista
5. [  ] Si llega un `action_required`: verificar alerta ⚠️ con MessageBox
6. [  ] Seleccionar un webhook → ver el JSON completo en el panel inferior
7. [  ] **⏹ Detener** listener

**Requisitos previos:**
- Puerto registrado si no es admin: `netsh http add urlacl url=http://+:5100/webhooks/mp/ user=EVERYONE`
- Tunnel (ngrok o Cloudflare) si probás desde red local
- Secret configurado en Tab Configuración

---

## 6. Troubleshooting Común

### Error "Acceso denegado" al iniciar Webhook Listener

**Causa:** HttpListener requiere permisos en Windows.  
**Solución:** Ejecutar en terminal como administrador:
```
netsh http add urlacl url=http://+:5100/webhooks/mp/ user=EVERYONE
```

### Error 405 al listar Sucursales

**Causa:** La API de MP requiere el sufijo `/search`.  
**Solución:** Ya está manejado en el Wrapper. Si ocurre, verificar que el `User ID` esté configurado.

### Error de validación de ciudad al crear Sucursal

**Causa:** MercadoPago valida la ciudad contra su catálogo interno.  
**Solución:** Usar nombres del catálogo oficial. Ejemplo para Buenos Aires:
- state_name: `Buenos Aires`
- city_name: `La Plata`, `Quilmes`, `Mar del Plata`, etc. (NO `Buenos Aires`)
- Para CABA: state_name = `Capital Federal`, city_name = `CABA`

### Error "Build: file in use"

**Causa:** El Demo está corriendo.  
**Solución:** Cerrar el Demo antes de recompilar, o:
```powershell
Stop-Process -Name "MercadoPago.Demo.WinForms" -ErrorAction SilentlyContinue
```

### Webhook no llega

**Causa posible:** Tunnel no está activo, o URL no configurada en MP.  
**Solución:**
1. Verificar que ngrok/cloudflared está corriendo
2. Verificar la URL configurada en MP Developers → Webhooks
3. Verificar que el listener dice "🟢 Escuchando"

### "La referencia externa podría contener PII"

**Causa:** `ExternalReferenceValidator` detectó posible información personal.  
**Solución:** Usar referencias genéricas sin datos personales:
- ✅ `VENTA-2024-001`, `ORD-ABC123`, `PS-20240325140000`
- ❌ `Juan-Perez-DNI12345678`, `cliente@email.com-001`

---

## 7. Checklist Final de Homologación

Antes de la entrevista de homologación, verificar que podés demostrar:

### Mandatorios (Críticos)
- [  ] `platform_id` configurado y enviado en las órdenes
- [  ] `config.point.print_on_terminal` y `ticket_number` presentes en Orders
- [  ] Cambio de modo de operación (PDV ↔ STANDALONE) funciona
- [  ] Manejo de `OnOrderCancelled` y `OnActionRequired` en webhooks
- [  ] Validación PII en `external_reference` (ExternalReferenceValidator)
- [  ] Formateo de montos por país (AmountFormatter)

### Flujos Completos a Demostrar
- [  ] **Venta QR**: crear orden → cliente paga → webhook recibido → consulta pago
- [  ] **Point Smart**: crear orden → pago en terminal → webhook → cancelación → reembolso
- [  ] **Checkout Pro**: crear preferencia → pagar online → buscar → reembolsar

### OAuth (Si aplica)
- [  ] Token refresh funciona
- [  ] Auto-refresh configurado

### Documentación
- [  ] Tener a mano este documento y la [guía de uso](guia-de-uso.md) durante la entrevista
