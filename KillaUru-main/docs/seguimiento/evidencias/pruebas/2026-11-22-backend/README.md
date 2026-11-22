# Pruebas automatizadas y cobertura del backend · 2026-11-22

> [Evidencias](../../README.md) · [Estado del proyecto](../../../estado-implementacion.md) · [Dónde está cada prueba](../../../../guias/pruebas.md)

| Campo | Valor |
|---|---|
| Comando | `pwsh tests/generar-resultados.ps1`, desde `backend/` |
| Qué hace | Compila, ejecuta cada proyecto de pruebas con cobertura en su carpeta, une la cobertura con ReportGenerator y aplica la puerta del 90 % por dominio, igual que el job `ci.yml` |
| Código | `f9f3ac3` + la deuda técnica y el script del mismo día, sin commit al ejecutar |
| Entorno | Windows 10, .NET 10, Docker 29.8.2. **La integración usa PostgreSQL/PostGIS real** (Testcontainers) |
| Detalle | [cobertura-por-clase.md](cobertura-por-clase.md) (ReportGenerator). El HTML completo queda en `backend/TestResults/cobertura/index.html` del PC |

## Resultados por proyecto

| Proyecto | Qué prueba | Total | Aprobadas | Fallidas | Omitidas |
|---|---|---:|---:|---:|---:|
| KillaUru.Anomalias.Tests | Reglas y casos de uso de 004 | 100 | 100 | 0 | 0 |
| KillaUru.Arquitectura.Tests | Capas, dependencias, legibilidad, pool | 497 | 497 | 0 | 0 |
| KillaUru.Auditoria.Tests | Reglas y casos de uso de 002 | 22 | 22 | 0 | 0 |
| KillaUru.Dashboard.Tests | Reglas y casos de uso de 007 | 23 | 23 | 0 | 0 |
| KillaUru.Identidad.Tests | Reglas y casos de uso de 003 | 89 | 89 | 0 | 0 |
| KillaUru.Ingesta.Tests | Reglas, casos de uso y lector del lote de 005 | 57 | 57 | 0 | 0 |
| KillaUru.Integracion.Tests | HTTP, BD, contratos y seguridad por spec | 251 | 248 | 0 | 3 |
| KillaUru.Mapa.Tests | Reglas, casos de uso y parámetros de 006 | 52 | 52 | 0 | 0 |
| KillaUru.Shared.Tests | Kernel compartido | 62 | 62 | 0 | 0 |
| **Total** | | **1 153** | **1 150** | **0** | **3** |

Las 3 omitidas son **pendientes**, no aprobadas:
- `FlujoPuntaAPuntaTests.Conductor_envia_lote_pipeline_procesa_y_anomalia_aparece_en_mapa` (E2E entre módulos).
- `FlujoPuntaAPuntaTests.Anonimo_puede_enviar_lotes_pero_no_consultar_usuarios` (E2E entre módulos).
- `RendimientoIdentidadTests.K6_con_300_cuentas_cumple_login_y_renovacion` (necesita k6 y cuentas sintéticas; se ejecuta a mano).

## Cobertura

**Puerta de la constitución (VII): 90 % de líneas por dominio. ✅ Aprobada.**

| Dominio | Líneas |
|---|---:|
| Anomalías | 100 % (270/270) |
| Auditoría | 100 % (37/37) |
| Dashboard | 100 % (124/124) |
| Identidad | 100 % (119/119) |
| Ingesta | 100 % (5/5) |
| Mapa | 97,0 % (129/133) |
| Shared | 100 % (41/41) |

**Cobertura de cada ensamblado completo**, con adaptadores, infraestructura y configuración (sin los proyectos de pruebas):

| Ensamblado | Líneas | Ramas |
|---|---:|---:|
| KillaUru.Shared | 100 % | 100 % |
| KillaUru.Identidad | 88,1 % | 75,0 % |
| KillaUru.Anomalias | 86,4 % | 70,8 % |
| KillaUru.Auditoria | 80,8 % | 72,4 % |
| KillaUru.Ingesta | 79,8 % | 63,0 % |
| KillaUru.Api | 78,0 % | 60,0 % |
| KillaUru.Mapa | 70,3 % | 51,8 % |
| KillaUru.Dashboard | **48,5 %** | 39,7 % |
| **Backend completo** | **78,8 %** (7 149 / 9 063) | **64,3 %** (2 433 / 3 778) |

## Lectura

- **Cero fallos** en 1 153 pruebas. La integración corre contra PostgreSQL real, no contra dobles.
- Las reglas de negocio (dominio) están cubiertas casi por completo; es lo que la constitución exige.
- Fuera del dominio, **Dashboard (48,5 %) y Mapa (70,3 %) son los menos cubiertos**. Mapa solo tiene, por ahora,
  su prueba de contrato en integración (las de API siguen pendientes en `specs/006-modulo-consulta-mapa/cierre.md`).
  Dashboard sí tiene integración y seguridad en `specs/007-modulo-dashboard/`; qué clases quedan sin cubrir se ve en
  [cobertura-por-clase.md](cobertura-por-clase.md). No se ha analizado la causa.
- La cobertura mide qué líneas se ejecutan, no si las aserciones son buenas. Por eso varias pruebas se validaron
  además con mutaciones (ver `specs/005-modulo-ingesta/pruebas.md`).
