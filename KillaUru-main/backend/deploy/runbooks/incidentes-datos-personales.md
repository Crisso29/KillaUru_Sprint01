# Incidentes con datos personales · EQ-13

Estado: procedimiento preparado el 2026-10-23; simulacro y entrega de alertas pendientes.

| Función | Persona | Responsabilidad |
|---|---|---|
| Responsable | CRISOLOGO | Coordinar, evaluar alcance, aprobar y presentar las comunicaciones |
| Reemplazo | CRISSO | Asumir la coordinación si CRISOLOGO no está disponible |
| Contactos | Inventario privado de operación | Completar teléfono/correo de ambos antes del simulacro; no guardar datos de contacto en Git |

## Detección y contención

1. Registrar el instante UTC en que se conoce el incidente, la alerta y quién lo recibió. Iniciar el cronómetro de 48 h; una alerta no demuestra por sí sola una filtración.
2. Avisar a CRISOLOGO; si no está disponible, activar a CRISSO. Mantener una bitácora con cada decisión y su hora.
3. Aislar la operación comprometida, suspender la cuenta involucrada y revocar sesiones cuando corresponda. Si Identidad aún no está implementado, aislar la API mediante el túnel y el contenedor; no afirmar que un JWT fue revocado.
4. Preservar una copia cifrada y restringida de la evidencia antes de restaurar. No modificar ni borrar auditoría, ni copiar contraseñas, tokens o datos de titulares a logs, tickets públicos o este repositorio.

## Reconstrucción

Consultar como administrador `GET /api/v1/auditoria?desde=...&hasta=...&actorId=...`.
Filtrar por `DatasetExportado`, `RolCambiado`, `CuentaBloqueada` y `AccesoAdministrativoDatos`.
Conservar los ids de registros, hora UTC, actor interno, objetivo y contexto. Para una exportación, comprobar formato, filtros y número de filas; el registro acredita la acción, no demuestra quién recibió una copia.

Identidad resuelve la cuenta del actor cuando exista 003. Una cuenta eliminada conserva solo su id.
Cruzar la bitácora de operación sin introducir vínculos entre cuentas y eventos de conducción.

## Notificación y canal oficial

El objetivo operativo es preparar y tramitar la comunicación sin superar 48 h desde el conocimiento.
Evaluar los supuestos de notificación y afectación a titulares del artículo 34; resolver internamente el incidente no elimina una obligación de notificar. Fuente: [D.S. 016-2024-JUS, artículo 34](https://www.smv.gob.pe/Uploads/DS016_2024_JUS.pdf).

Canal verificado el 2026-10-23: [Formulario Virtual Único](https://reporte.cnsd.gob.pe/home/minjus).
Su carácter oficial está descrito por la [PCM y el MINJUSDH](https://www.gob.pe/institucion/pcm/noticias/1249567-pcm-y-ministerio-de-justicia-presentan-herramienta-digital-para-reportar-incidentes-de-seguridad-y-proteger-datos-personales).
Conservar constancia de envío y número de registro. Si el canal falla, documentar los intentos y consultar el canal alternativo publicado por la Autoridad; no sustituirlo por una dirección inventada.

### Plantilla para la Autoridad

- Identificación del responsable del tratamiento y contacto autorizado: [completar en expediente privado].
- Momento de ocurrencia estimado / detección / conocimiento, en UTC: [fechas].
- Naturaleza del incidente, sistemas, tipos de datos y número aproximado de afectados: [confirmado / pendiente de confirmar].
- Consecuencias posibles y medidas adoptadas o propuestas, con sus horas: [detalle].
- Punto de contacto para ampliar información: [CRISOLOGO o CRISSO, contacto privado].
- Evidencia de soporte y estado de investigación: [ids de registros, sin adjuntar datos excesivos].
- Si existe demora: [motivos y evidencia; no ocultarla].

### Plantilla para titulares afectados

«El [fecha] identificamos [hecho explicado sin tecnicismos]. Podrían haberse afectado [tipos de datos].
Hemos realizado [medidas]. Te recomendamos [acciones concretas aplicables].
Puedes comunicarte con [contacto autorizado]. Actualizaremos esta información por [canal].»

La comunicación a titulares se prepara en lenguaje claro según la afectación prevista en el artículo 34.
No enviar mensajes reales durante el simulacro.

## Recuperación y cierre

Corregir la causa, verificar permisos y repetir las pruebas afectadas antes de reabrir.
Usar `restaurar.md` si se requiere recuperación; repetir eliminaciones posteriores al respaldo cuando sea posible.
Registrar qué se recuperó, qué permanece incierto, tiempos y medidas preventivas.

## Simulacro semestral

En `killauru-pruebas`, con datos sintéticos, simular una exportación indebida, reconstruirla y redactar ambas plantillas.
Anotar inicio, detección, contención, reconstrucción y borrador listo en `simulacros.md`.
T031 permanece abierta hasta ejecutar el ejercicio: este procedimiento no es evidencia de un simulacro realizado.
