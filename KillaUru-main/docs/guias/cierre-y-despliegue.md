# Cierre por spec y preparación del despliegue

> Guía de trabajo. [Volver al índice de documentación](../README.md).

**Revisión:** 2026-11-15. **Decisión actual:** todavía no está listo para producción.
Este documento ordena el trabajo restante. No registra un despliegue ni un CI remoto aprobado.
[Estado local](../seguimiento/estado-implementacion.md) · [Pruebas](pruebas.md) · [Revisión documental](../historial/revision-documental.md)

## Orden de trabajo

Cerrar primero el backend en su orden de dependencias. Adelantar tareas independientes
de Android/ML es posible, pero la detección de 008 necesita el modelo aprobado de 009.

| Orden | Spec | Antes de cerrar localmente | Última validación desplegada |
|---|---|---|---|
| 1 | [001 Plataforma](../../specs/001-plataforma-backend/cierre.md) | Integración, cobertura, quickstart y composición; preparar E2E compartido | Access, protección de rama, alertas, restauración, rollback y carga |
| 2 | [002 Auditoría](../../specs/002-modulo-auditoria/cierre.md) | Revisión de emisores y transacciones | Alertas y simulacro EQ-13 |
| 3 | [003 Identidad](../../specs/003-modulo-identidad-accesos/cierre.md) | Revalidar suites y contrato del candidato | Correo/Turnstile, sesión, revocación y admin inicial |
| 4 | [004 Anomalías](../../specs/004-modulo-anomalias/cierre.md) | Integración y escenario completo EQ-12 | p95 con 005 y tráfico legítimo/abusivo controlado |
| 5 | [005 Ingesta](../../specs/005-modulo-ingesta/cierre.md) | Tests HTTP/PostgreSQL, reinicio, purgas, salud y contrato | Conciliación, resistencia, cola y alerta |
| 6 | [006 Mapa](../../specs/006-modulo-consulta-mapa/cierre.md) | Geometría demo, ETag, serialización, roles y contrato | Caché de borde, recorte anónimo y carga |
| 7 | [007 Dashboard](../../specs/007-modulo-dashboard/cierre.md) | Contrato dedicado y trazabilidad | Cálculo diario, recuperación y consistencia |
| 8 | [009 Dataset/modelo](../../specs/009-dataset-y-modelo/cierre.md) | Completar CLI real, evaluación, puerta y publicación | Campaña, benchmark, versión aprobada y rollback R2 |
| 9 | [008 Android](../../specs/008-app-android/cierre.md) | Tests faltantes, suite instrumentada y modelo real | Offline, alertas, batería, dispositivos y conciliación |
| 10 | [010 Panel](../../specs/010-panel-admin/cierre.md) | Sustituir skip/fixme, configurar E2E real y semillas | Flujos reales, accesibilidad, rendimiento y cabeceras |

001/T080 y la carga combinada solo pueden cerrarse después de integrar 004–007.
La base de 001 habilita continuar, pero su aceptación final permanece abierta.
No exigir cerrar una dependencia circular ni marcar ese E2E como hecho anticipadamente.

## Tres estados separados

1. **Cierre local:** código, documentación, contratos, tests y controles del commit aprobados.
2. **Candidato en pruebas:** mismo SHA y artefactos desplegados en entorno aislado con datos sintéticos;
   verificar flujos completos, configuración, seguridad y operación.
3. **Apto para producción:** evidencia desplegada revisada, incidencias resueltas, modelo aprobado
   y mecanismos de recuperación ensayados. CI verde por sí solo no acredita este estado.

## Puertas para el candidato

- [ ] Fijar SHA; árbol de trabajo revisado y cambios incluidos en el candidato.
- [ ] Compilar y ejecutar todas las suites locales, integración PostgreSQL y contratos.
- [ ] Cobertura de dominio según constitución, lint, arquitectura y límites aprobados.
- [ ] Sin skip/fixme críticos presentados como pruebas aprobadas.
- [ ] Revisar dependencias, secretos, autorización, consultas y privacidad según la guía de seguridad.
- [ ] Configurar entorno de pruebas, migraciones y datos de prueba sin secretos de producción.
- [ ] Desplegar el mismo SHA; registrar imagen, versión de esquema, APK/panel/modelo y configuración.
- [ ] Ejecutar pruebas desplegadas y conservar informes con entorno/fecha/SHA.
- [ ] Restaurar un respaldo y ensayar vuelta atrás; confirmar revocación y purga.
- [ ] Resolver incidencias y repetir la regresión afectada antes de promover.

## Escenarios desplegados mínimos

| Recorrido | Evidencia que debe conservarse |
|---|---|
| Cuenta → correo → acceso → renovación → revocación | Entrega/canje, cookies y códigos de acceso; sin tokens en logs |
| App offline → lote → recepción durable → anomalía → mapa | Conteos conciliados, idempotencia, reanudación, ETag y tiempos |
| Moderación → auditoría → exportación | Mismo efecto transaccional, roles y ausencia de datos prohibidos |
| Procesamiento → indicadores diarios → panel | Día local, recuperación y resultados agregados consistentes |
| Carga/picos/estrés/resistencia/caos | Datos sintéticos, concurrencia, duración, errores, p95 y límites |
| Restore/rollback | Duraciones, integridad, sesiones revocadas y limpieza de seudónimos |
| Android/modelo | Tres celulares, latencia, consumo y resultados de campo según 008/009 |
| Panel y borde | E2E real, accesibilidad, cabeceras/CSP, CORS y caché por rol |

Las pruebas de carga tienen sus condiciones en [el runbook k6](../../backend/tests/carga/README.md).
Una prueba de cuota local no acredita resistencia DDoS del despliegue.

## Configuración y comportamiento real de los workflows

- Backend: `CI` valida; `CD` publica imagen tras un push aprobado a main y despliega
  si los secretos del entorno produccion están completos. La publicación de imagen
  puede ocurrir aunque falten secretos para desplegar.
- Panel: `Panel Deploy` escucha `Panel CI`, publica su SHA y requiere variables/secretos.
- Modelo: workflow manual existente, actualmente bloqueado por la puerta CLI incompleta.
- El pipeline actual no demuestra una promoción automática pruebas → producción.
  Preparar y verificar esa separación antes de usar main como mecanismo de entrega.
- No se consultaron secretos, reglas de rama, ejecuciones remotas ni disponibilidad de los dominios.

Nombres efectivos: [operación CI](../historial/operacion-ci.md) e [inventario de secretos](../../backend/deploy/runbooks/secretos.md).
La planificación de 001 usa otros nombres para Access/SSH; el cierre debe conciliar ese contrato.
Documentar un nombre no prueba que el secreto exista.

## Evidencia de cada candidato

| SHA / versión | Entorno | Suites y resultados | Incidencias | Restore/rollback | Decisión |
|---|---|---|---|---|---|
| Pendiente | Pruebas | Sin ejecutar | Ver cada cierre.md | Sin acreditar | No promover |

El próximo bloque concreto es **001: ejecutar integración con Docker, medir cobertura,
completar T042/T067, revisar quickstart y preparar la configuración de pruebas**. Su E2E final queda enlazado
al cierre de 004–007.
