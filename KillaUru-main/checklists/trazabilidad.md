# Trazabilidad: Panel de administración «KillaUru Ops»

Feature: `010-panel-admin`  
Actualizado: 2026-11-07

## Requisitos funcionales

| RF | Descripción breve | Fase | Archivo principal | Estado |
|---|---|---|---|---|
| FR-001 | SPA React en Cloudflare Pages | F1 | panel/vite.config.ts | ✓ |
| FR-002 | Panel no decide permisos | F7 | funcionalidades/moderacion/modelo/acciones.ts | ✓ |
| FR-003 | Español, coma decimal, hora Lima | F2 | compartido/formato/ | ✓ |
| FR-004 | Chrome, Firefox, Edge | F11 | playwright.config.ts | ✓ |
| FR-005 | Login con canal=panel y Turnstile | F3 | funcionalidades/login/ | ✓ |
| FR-006 | No localStorage/cookies JS | F2 | eslint.config.js, api/middleware.ts | ✓ |
| FR-007 | Renovación silenciosa | F2 | sesion/renovador.ts | ✓ |
| FR-008 | Errores uniformes por codigo | F2 | api/problema.ts | ✓ |
| FR-009 | Salir cierra todas las pestañas | F2 | sesion/canalSesion.ts | ✓ |
| FR-010 | «Sesión hasta HH:MM» | F2 | app/marco/ResumenSesion.tsx | ✓ |
| FR-011 | Cabeceras de seguridad | F2+F11 | scripts/generar-cabeceras.mjs | ✓ |
| FR-012 | CSRF — POST/PUT/DELETE + JSON | F2 | api/middleware.ts | ✓ |
| FR-013 | Recuperar contraseña | F3 | funcionalidades/login/ui/PaginaRecuperar.tsx | ✓ |
| FR-014 | Páginas de enlace de correo | F6 | funcionalidades/cuenta/ | ✓ |
| FR-015 | 6 indicadores de negocio | F8 | funcionalidades/resumen/ | ✓ |
| FR-016 | Gráfico eventos por día | F8 | funcionalidades/resumen/ui/GraficoEventos.tsx | ✓ |
| FR-017 | «Paneles técnicos» abre Grafana | F8 | funcionalidades/resumen/ui/PaginaResumen.tsx | ✓ |
| FR-018 | «Requiere atención» | F8 | funcionalidades/resumen/ui/RequiereAtencion.tsx | ✓ |
| FR-019 | Franja corredor | F8 | funcionalidades/resumen/ui/FranjaCorredor.tsx | ✓ |
| FR-020 | Últimos cambios de estado | F8 | funcionalidades/resumen/ui/UltimosCambios.tsx | ✓ |
| FR-021 | MapLibre + PMTiles R2 | F4 | funcionalidades/mapa/ui/LienzoMapa.tsx | ✓ |
| FR-022 | Filtros mapa con estado por defecto | F4 | funcionalidades/mapa/modelo/filtros.ts | ✓ |
| FR-023 | Forma por estado + número de severidad | F4 | funcionalidades/mapa/modelo/simbolos.ts | ✓ |
| FR-024 | Pestaña Lista como alternativa accesible | F4 | funcionalidades/mapa/ui/PanelLateral.tsx | ✓ |
| FR-025 | Detalle con kmVia y tramo demo | F4 | funcionalidades/mapa/ui/DetalleAnomalia.tsx | ✓ |
| FR-026 | Filtros y anomalía en la dirección | F4 | funcionalidades/mapa/ui/PaginaMapa.tsx | ✓ |
| FR-027 | 4 acciones de RN-07 | F7 | funcionalidades/moderacion/modelo/acciones.ts | ✓ |
| FR-028 | «Por revisar» y «Esperan confirmación» | F7 | funcionalidades/moderacion/ui/PaginaModeracion.tsx | ✓ |
| FR-029 | Pendientes de solo lectura | F7 | funcionalidades/moderacion/ui/TablaPendientes.tsx | ✓ |
| FR-030 | 409 transicion_no_permitida + recargar | F7 | funcionalidades/moderacion/api/useModerar.ts | ✓ |
| FR-031 | Lista de usuarios con filtros | F5 | funcionalidades/usuarios/ui/PaginaUsuarios.tsx | ✓ |
| FR-032 | Invitar usuario | F5 | funcionalidades/usuarios/ui/DialogoInvitar.tsx | ✓ |
| FR-033 | Cambiar rol, desactivar, activar | F5 | funcionalidades/usuarios/ui/MenuUsuario.tsx | ✓ |
| FR-034 | Revisión mensual de privilegios | F5 | funcionalidades/usuarios/ui/DialogoRevision.tsx | ✓ |
| FR-035 | Exportar CSV/GeoJSON | F10 | funcionalidades/exportacion/ | ✓ |
| FR-036 | «Recientes» de exportaciones | F10 | funcionalidades/exportacion/ui/ExportacionesRecientes.tsx | ✓ |
| FR-037 | Auditoría solo lectura | F9 | funcionalidades/auditoria/ | ✓ |
| FR-038 | Actor por correo → actorId | F9 | funcionalidades/auditoria/modelo/actor.ts | ✓ |
| FR-039 | useActores (un solo ?ids=) | F5 | funcionalidades/usuarios/api/useActores.ts | ✓ |
| FR-040 | Búsqueda rápida | F4 | app/marco/BusquedaRapida.tsx | ✓ |
| FR-041 | Menú con contador Por revisar | F2+F7 | app/marco/ | ✓ |
| FR-042 | Estado vacío y error con Reintentar | F2 | compartido/ui/ | ✓ |
| FR-043 | Accesibilidad WCAG 2.1 AA | F2+F11 | compartido/ui/ | ✓ |
| FR-044 | Presupuesto JS ≤ 120 kB inicial, ≤ 300 kB mapa | F11 | panel/.size-limit.json | ✓ |
| FR-045 | Formato de números y fechas | F2 | compartido/formato/ | ✓ |
| FR-046 | useQuery con staleTime y refetch=false | F4+ | api/consultas.ts | ✓ |
| FR-047 | No archivo > 199 líneas | F1+ | pruebas/arquitectura/lineas.test.ts | ✓ |
| FR-048 | Tipos generados + contrato MSW | F1+ | pruebas/contrato/ | ✓ |
| FR-049 | CI completo en GitHub Actions | F11 | .github/workflows/panel-ci.yml | ✓ |
| FR-050 | CSP estricta (sin unsafe-inline/eval) | F2+F11 | scripts/generar-cabeceras.mjs | ✓ |

## Huecos pendientes (H-01..H-06)

| Hueco | Descripción | Estado |
|---|---|---|
| H-01 | Verificación de `Origin = Api:OrigenPanel` en el servidor (001, SEG-21) | Pendiente — depende del backend |
| H-02 | `appsettings.Pruebas.json` con `Api:OrigenPanel` del panel de pruebas y claves Turnstile (001) | Pendiente — infraestructura |
| H-03 | Secretos `CLOUDFLARE_API_TOKEN` y `CLOUDFLARE_ACCOUNT_ID` en GitHub (001/RNF-34) | Pendiente — ops |
| H-04 | Estilo, glifos, sprite y PMTiles publicados por 008 en `cdn.killauru.pe`; CORS de R2 con GET/HEAD/Range | Pendiente — feature 008 (propuesta: `infra/r2-cors.json`) |
| H-06 | HSTS `includeSubDomains` y TLS mínimo 1.2 en zona Cloudflare (001, SEG-07) | Pendiente — ops |

> H-05 no se abrió: el `motivo` de las transiciones se deriva en el panel (R-014 de `research.md`).

## Pendientes de research.md

Ver `specs/010-panel-admin/research.md` para decisiones abiertas y por confirmar.

Los ítems marcados en §«Pendientes de confirmar por el autor» quedaron aprobados el 2026-10-22 (D18, `specs/revision/decisiones.md`). La enmienda de la constitución a la versión 1.2.0 también quedó aprobada.

Pendientes que siguen abiertos (backend o infraestructura):
- H-01, H-02, H-03, H-04, H-06 listados arriba.
