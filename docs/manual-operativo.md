# Manual Operativo — MercadoPago Point Smart

## Público objetivo
Este manual está dirigido al **operador de caja** que utiliza el sistema integrado con MercadoPago Point Smart.

---

## 1. Antes de empezar

### Verificar el terminal
- El terminal Point Smart debe estar **encendido** y conectado a WiFi
- La pantalla debe mostrar el logo de MercadoPago
- El modo de operación debe ser **"PDV"** (integrado con el punto de venta)

### Verificar la conexión
- El sistema ERP/PDV debe estar **conectado** al servidor de MercadoPago
- En la pantalla principal del sistema, debe aparecer un indicador de conexión ✅

---

## 2. Realizar un cobro

1. **Armar la venta** en el sistema (cargar productos, cantidades, precios)
2. **Seleccionar "Cobrar con MercadoPago"** o el botón de cobro correspondiente
3. El sistema envía la orden al terminal Point Smart
4. El terminal mostrará el **monto a cobrar** en su pantalla
5. El cliente acerca/inserta su tarjeta o paga con QR
6. Esperar a que la terminal muestre:
   - ✅ **APROBADO** → el sistema actualiza automáticamente la venta como pagada
   - ❌ **RECHAZADO** → el sistema notifica el rechazo; reintente con otra tarjeta
7. El ticket se imprime automáticamente en el terminal (si está configurado)

---

## 3. Cancelar un cobro desde el sistema

1. Seleccionar la venta pendiente en el sistema
2. Presionar **"Cancelar orden"**
3. El terminal dejará de esperar el pago
4. La venta vuelve a estado **borrador**

---

## 4. Cancelación desde el terminal

Si el cobro se cancela **directamente en el terminal** (botón rojo o timeout):

1. El sistema recibe una notificación automática
2. La venta se marca como **cancelada** en el sistema
3. No se requiere acción de su parte

---

## 5. Estado "Acción Requerida"

> ⚠️ **IMPORTANTE**: Este estado requiere intervención manual.

Si el sistema muestra un alerta de **"Acción Requerida"**:

1. **Mirar el terminal** — verificar qué pasó con la operación:
   - ¿El pago fue aprobado en el terminal?
   - ¿Fue rechazado?
   - ¿Se cortó la conexión?
2. **Registrar manualmente** el resultado en el sistema según lo que muestra el terminal
3. Si tiene dudas, consultar con su supervisor

---

## 6. Reembolsos / Devoluciones

1. Buscar la venta pagada en el sistema
2. Seleccionar **"Devolver"** o **"Reembolso"**
3. Elegir si es reembolso total o parcial
4. Confirmar la operación
5. El cliente recibirá el dinero de vuelta según el medio de pago

> 📌 Los reembolsos pueden tardar hasta **10 días hábiles** en reflejarse.

---

## 7. Problemas frecuentes

| Problema | Solución |
|---|---|
| No llega la orden al terminal | Verificar conexión WiFi del terminal. Presionar **"Actualizar"** en el dispositivo. |
| Terminal muestra "Modo Standalone" | Contactar al administrador para cambiar a modo "PDV" |
| Pago queda en "Pendiente" | Esperar hasta 5 minutos. Si persiste, verificar en el panel de MP |
| Error de conexión del sistema | Verificar internet. Reiniciar la aplicación si es necesario |
| Ticket no se imprime | Verificar que la impresión esté habilitada en la configuración del sistema |

---

## 8. Contacto de soporte

- **Soporte técnico del sistema:** contactar al equipo de desarrollo
- **Soporte MercadoPago:** [ayuda.mercadopago.com.ar](https://ayuda.mercadopago.com.ar)
