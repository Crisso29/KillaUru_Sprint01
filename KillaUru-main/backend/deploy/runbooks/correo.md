# Correo y anti-bot de Identidad (003)

Proveedor implementado: Resend, por `IEnviadorCorreo`. Verificado el 2026-10-24:
plan gratuito de 3 000 correos/mes, máximo 100/día ([precios oficiales](https://resend.com/pricing)).
No habilitar sobreconsumo de pago. El volumen y la entrega real deben verificarse en el entorno de pruebas.

1. Verificar el dominio `killauru.pe` en Resend. Publicar exactamente los registros SPF y DKIM que asigne
   el proveedor. No crear un segundo registro SPF: integrar las fuentes autorizadas del dominio.
2. Publicar DMARC en `_dmarc.killauru.pe`; comenzar con observación, revisar alineación y luego endurecer
   la política. Los valores DNS concretos dependen de la cuenta y quedan pendientes de configuración externa.
3. Guardar `KU_CORREO_API_KEY`, `KU_TURNSTILE_SECRETO` y `KU_IDENTIDAD_CLAVE_ROTACION` en el inventario
   cifrado; Compose los traduce a `Identidad__...`. La clave de rotación debe tener al menos 32 bytes.
4. Turnstile valida cada desafío del panel por [Siteverify](https://developers.cloudflare.com/turnstile/get-started/server-side-validation/).
   Sin desafío válido o ante error del proveedor, el panel recibe `antibot_fallido`; la app no usa Turnstile.
5. Probar registro, verificación, recuperación e invitación con destinatarios controlados. Los enlaces usan
   `#token=` y los tokens se guardan solo como SHA-256. Nunca adjuntar enlaces reales al reporte de prueba.

El envío ocurre después del commit, con tres reintentos e idempotencia del proveedor. Si sigue fallando,
se registra solo un error técnico genérico; el usuario puede reenviar. No hay cola durable: si el proceso
cae después del commit y antes del envío, se necesita reenviar. Esta limitación coincide con R-007.

En desarrollo, `Identidad:CorreoConsola=true` solo imprime una confirmación sin destinatario ni enlace.
Las pruebas automáticas usan un buzón en memoria para canjear enlaces. Producción rechaza el modo consola.
El comando inicial usa la salida privada y el override sin logs descritos en `desplegar.md`.

Pendiente operativo: activar dominio, verificar SPF/DKIM/DMARC, crear widget/secretos de Turnstile y probar
entrega real. Las pruebas con dobles HTTP no certifican estos servicios externos (T041).
