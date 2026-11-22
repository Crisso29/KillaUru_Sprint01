# Pruebas automatizadas y cobertura del panel · 2026-11-22

> [Evidencias](../../README.md) · [Estado del proyecto](../../../estado-implementacion.md) · [Pruebas de la spec 010](../../../../../specs/010-panel-admin/pruebas.md)

| Campo | Valor |
|---|---|
| Vitest | `npm run cobertura` (igual que `panel-ci.yml`), desde `panel/`; resumen por tipo con `--reporter=json` |
| Playwright | `npm run e2e`: build en modo simulado (MSW) + `vite preview`, en Chromium, Firefox y Edge |
| Entorno | Windows 10, Node 24.11.1, Playwright 1.47.2 |
| Código | `main` tras el commit de deuda técnica del 2026-11-22 |
| Salidas locales | `panel/coverage/` y `panel/playwright-report/` (ignoradas por git) |

## Vitest: 389 pruebas, 0 fallidas, 0 omitidas

El sufijo del archivo indica el tipo de prueba ([guía de pruebas](../../../../guias/pruebas.md)).

| Tipo | Archivos | Pruebas | Aprobadas |
|---|---:|---:|---:|
| Unitarias (`.unitaria.test`) | 29 | 262 | 262 |
| Componentes (`.componente.test`) | 18 | 98 | 98 |
| Seguridad (`.seguridad.test`) | 1 | 12 | 12 |
| Accesibilidad (`.accesibilidad.test`) | 1 | 8 | 8 |
| Contrato con el OpenAPI (`.contrato.test`) | 1 | 4 | 4 |
| Integración (`.integracion.test`) | 1 | 4 | 4 |
| Arquitectura (`.arquitectura.test`) | 1 | 1 | 1 |
| **Total** | **52** | **389** | **389** |

## Cobertura (Vitest, V8)

**Umbral del proyecto: 90 % en las carpetas `modelo/` (lógica sin interfaz), T009. ✅ Cumplido** en conjunto.

| Área | Líneas | Ramas | `modelo/` líneas | `modelo/` ramas |
|---|---:|---:|---:|---:|
| funcionalidades/cuenta | 97,9 % | 90,0 % | 100 % | 100 % |
| funcionalidades/moderacion | 97,8 % | 86,6 % | 100 % | 90,9 % |
| funcionalidades/auditoria | 96,2 % | 81,0 % | 100 % | **83,3 %** |
| funcionalidades/exportacion | 95,3 % | 79,1 % | 100 % | 100 % |
| funcionalidades/usuarios | 86,8 % | 84,7 % | 100 % | 100 % |
| funcionalidades/resumen | 74,1 % | 88,2 % | 98,6 % | 97,3 % |
| compartido | 74,5 % | 87,5 % | — | — |
| config | 75,0 % | 33,3 % | — | — |
| funcionalidades/login | 64,4 % | 89,4 % | 100 % | 100 % |
| funcionalidades/mapa | 60,7 % | 74,4 % | 100 % | 98,0 % |
| api | 57,1 % | 73,7 % | — | — |
| app | 47,4 % | 77,8 % | — | — |
| sesion | 29,7 % | 81,8 % | — | — |
| main.tsx | 0 % | 0 % | — | — |
| **Panel completo** | **75,1 %** (3 112 / 4 142) | **82,8 %** (704 / 850) | | |

- El umbral se evalúa sobre **todas** las carpetas `modelo/` juntas. Mirada sola, `auditoria/modelo` tiene 83,3 % de ramas.
- Lo menos cubierto es el arranque y la sesión (`main.tsx`, `sesion/`, `app/`): lo que solo se ejercita navegando, es decir, con E2E.

## Playwright (E2E): solo 1 de 13 archivos está implementado

| Archivo | Estado | Casos (× 3 navegadores) |
|---|---|---:|
| `funcionales/resumen.spec.ts` | ✅ **Implementado**: 12/12 aprobados en 2 repeticiones | 12 |
| login, sesión, mapa, `seguridad/almacenamiento` | `fixme` (pendientes) | 45 |
| auditoría, cuenta, exportación, moderación, usuarios, accesibilidad, teclado, rendimiento | `skip` (pendientes) | 102 |
| **Total** | 12 ejecutados, **147 pendientes** | 159 |

En la primera ejecución completa, 1 caso de Firefox falló con `NS_ERROR_CONNECTION_REFUSED` (no llegó a conectar
con `vite preview`). Al repetir el archivo dos veces pasaron 12/12: fue un fallo de conexión momentáneo, no del panel.

**Alcance:** estos E2E usan **MSW** (API simulada). Prueban el panel, no la API real; la API real se acredita con
`npm run e2e:pruebas` contra `killauru-pruebas` (010/T093). Los pendientes figuran en
[specs/010-panel-admin/cierre.md](../../../../../specs/010-panel-admin/cierre.md).
