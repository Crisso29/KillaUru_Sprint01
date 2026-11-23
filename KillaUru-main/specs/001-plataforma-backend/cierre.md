# Cierre de spec 001 — Plataforma backend

**Revisado:** 2026-11-15. **Estado:** cierre de producción no acreditado.
**Seguimiento:** 74/85 tareas marcadas; 11 pendientes en tasks.md (T082–T084 añadidas el 2026-11-22 como deuda técnica; T083 y T084 hechas el 2026-11-23; T085, colección de Postman, hecha el 2026-11-23).
[Requisitos](spec.md) · [Tareas](tasks.md) · [Pruebas](pruebas.md) · [Plan de cierre global](../../docs/guias/cierre-y-despliegue.md)

## Dependencias

Base de todas las specs.

## Evidencia disponible

Host, seis módulos, migraciones, scripts y workflows presentes. La validación local anterior aprobó 62 casos Shared y 485 de arquitectura.

Tareas documentales cerradas en esta revisión: T055. Eso no certifica ejecución funcional.
## Qué falta localmente

Se reabren T042 y T067 por evidencia concreta: `ContratoOpenApiTests` enumera cuatro
contratos y comprueba rutas/métodos/presencia de códigos, sin comparar propiedades requeridas;
no recorre todos los YAML. `FiltroDatosPersonalesTests` usa eventos Serilog sintéticos,
no la captura completa de logs y trazas HTTP exigida por T067. Las pruebas parciales se conservan.

Falta el E2E compartido y ejecutar quickstart/integración. Los nombres de secretos KU_CF_ACCESS_* / KU_DEPLOY_SSH_KEY del diseño no coinciden con CF_ACCESS_* / SSH_PRIVATE_KEY leídos por cd.yml; conciliar explícitamente antes del cierre.

## Qué falta en el entorno desplegado

Protección de main y checks reales; SSH/Access; secretos; paneles/alertas; rollback y restauración; carga combinada; presupuesto.

## Inventario de tareas abiertas

Las casillas se mantienen únicamente en [tasks.md](tasks.md); esta tabla es una fotografía
de la revisión. “Local” incluye implementación/documentación y preparación de escenarios.
“Desplegado/campo” exige resultados externos. Una tarea puede necesitar ambos.

| Tarea | Etapa principal | Entregable pendiente |
|---|---|---|
| T042 | Local | Completar descubrimiento de contratos y comparación de propiedades requeridas, además de rutas/métodos/códigos |
| T067 | Local | Capturar y verificar logs y trazas reales de lote, login y error; el filtro sintético no basta |
| T050 | Desplegado/campo | Configurar en Grafana Cloud (sin tarjeta) los paneles y alertas: p95 por ruta (mapa 250 ms, lote 300 ms, resto 250 ms y sesión 500 ms como metas SDD), 5xx > 1 %, disco > 80 %, tarea > 26 h (incluido `backup`), uso del plan ≥ 80 %; en UptimeRobot el monitor de `/health/ready` y su página pública; documentarlo con el calendario de tareas de `ProgramaReloj` |
| T054 | Desplegado/campo | Simulacro en «killauru-pruebas»: vuelta atrás con `desplegar.sh <sha-anterior>` < 5 min y restauración con RTO ≤ 2 h, verificando 0 sesiones sin revocar y 0 seudónimos > 72 h; anotar tiempos en `deploy/runbooks/simulacros.md` |
| T070 | Desplegado/campo | Proteger `main` en GitHub: *check* `ci` obligatorio, sin uniones directas; documentarlo |
| T072 | Desplegado/campo | Configurar Cloudflare Access para SSH a la VM (token de servicio, sin puertos abiertos) y los secretos `KU_CF_ACCESS_CLIENT_ID`, `KU_CF_ACCESS_CLIENT_SECRET`, `KU_DEPLOY_SSH_KEY`; agregarlos a `secretos.md` |
| T079 | Desplegado/campo | Correr las 5 pruebas fuera de la VM, en horario de poco tráfico, y anotar resultados (incluido el retraso de cola de `retraso_cola_s`) en `deploy/runbooks/simulacros.md` |
| T080 | Local | Prueba de punta a punta: lote → anomalía → mapa (invalidación, `ETag` nuevo) → indicador del día |
| T056 | Local | Ejecutar `quickstart.md` completo y marcar `checklists/trazabilidad.md` |
| T057 | Desplegado/campo | Revisar costo: alertas de presupuesto en Oracle y Cloudflare a US$ 8 |
| T082 | Local | Un solo `NpgsqlDataSource` para que el tope de 30 conexiones sea real (hoy 3 pools). [DT-02](../../docs/seguimiento/deuda-tecnica.md) |

## Criterio de cierre

- Requisitos de esta spec trazados a escenarios y aserciones reales, sin pendientes críticos omitidos.
- Compilación, pruebas, arquitectura y cobertura exigida aprobadas para el mismo commit.
- Contratos y parámetros efectivos contrastados; sin sustituir servicios reales silenciosamente.
- Resultados desplegados con fecha, SHA, entorno, datos, conteos, latencias y enlaces a informes.
- Incidencias resueltas y regresión ejecutada; rollback/restauración cuando corresponda.

Los resultados de la sesión anterior están en [estado-implementacion.md](../../docs/seguimiento/estado-implementacion.md).
Las revisiones históricas conservan sus fechas; no acreditan este candidato por sí solas.
