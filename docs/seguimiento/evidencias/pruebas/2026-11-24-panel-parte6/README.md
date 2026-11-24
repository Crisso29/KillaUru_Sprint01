# Evidencia — Panel parte 6 (2026-11-24)

**Rama:** main · **Responsable:** DevCrisso  
**Spec:** 010-panel-admin · **Sprint 2** (entrega 2026-11-28)

## Resumen de resultados

| Suite | Resultado | Notas |
|---|---|---|
| Vitest (499 casos) | ✅ 499/499 | `npm test` sin fallas |
| E2E Chromium (62 casos) | ✅ 62/62 | `npm run e2e -- --project=chromium` |
| E2E Edge (62 casos) | ✅ 62/62 | `npm run e2e -- --project=msedge` |
| E2E Firefox (62 casos) | ✅ 62/62 | 2 casos fallaron por latencia del servidor preview; pasan al reintentar |
| ESLint | ✅ 0 errores | `npm run lint` |
| TypeScript | ✅ sin errores | `tsc -b` |

## Cambios realizados en esta sesión

### Deuda de calidad (T035)
- Funciones ≤ 20 líneas en todos los archivos del panel: auditoria, login, cuenta, exportación, mapa, resumen, armazón, compartido, sesión.
- 0 errores ESLint (antes: 47 archivos con funciones largas).

### AAA en todas las pruebas (T036)
- Detector TypeScript en `pruebas/arquitectura/aaa.arquitectura.test.ts`.
- Scripts de marcado automático: `marcar_aaa.mjs`, `corregir_acto.mjs`, `acto_en_expect.mjs`.
- Todas las pruebas Vitest y Playwright con secciones Arrange / Act / Assert en orden.

### Bugs corregidos con TDD (encontrados durante T035/T036)
| Bug | Prueba que lo detectó |
|---|---|
| Rutas de correo (`/verificar`, `/establecer-contrasena`) erróneas | `enlacesCorreo.contrato.test.ts` |
| Filtro de severidad enviaba `severidad_min` en vez de `severidad` | `consultaMapa.contrato.test.ts` |
| `ProveedorSesion` no avisaba a otras pestañas ni detenía el renovador al salir | `ProveedorSesion.componente.test.tsx` |
| Login con `?motivo=sesion` no mostraba el aviso | `PaginaLogin.componente.test.tsx` |
| Caché no se vaciaba al cerrar sesión | `ProveedorSesion.componente.test.tsx` |
| Diálogos no recibían foco (WCAG 2.4.3) | `Dialogo.componente.test.tsx` |
| `--ink-3` con contraste insuficiente (4.33:1) | `contraste.accesibilidad.test.ts` |
| Segundo 401 borraba el motivo de la sesión terminada | `estadoSesion.unitaria.test.ts` |

### Simulador MSW (soporte E2E)
- `pruebas/msw/simulador/`: estado en memoria con tabla RN-07 del backend.
- `manejadores/auditoria.ts`: filtros por `accion`, `categoria`, `actorId` (igual que el backend).
- `manejadores/exportacion.ts`: CSV/GeoJSON según `formato`, con cabecera correcta del contrato 002.
- `usuarios.json`: 4 cuentas (admin, conductor, pendiente, desactivada).
- `auditoria.json`: 4 registros (AnomaliaModerada, AccesoAdministrativoDatos, RolCambiado, DatasetExportado).

### E2E implementados (T032–T034, T050, T058, T062, T068, T077, T080, T081, T085, T086, T087 parcial)

**Funcionales (9 archivos, 46 casos):**
- `login.spec.ts`: entrar, credenciales malas, bloqueo 423, conductor rechazado.
- `sesion.spec.ts`: renovación ante 401, sesión revocada → login con aviso, salida entre pestañas.
- `mapa.spec.ts`: carga sin parámetros (SC-001), filtros, dirección copiada, lista navegable con teclado.
- `moderacion.spec.ts`: MarcarReparada, Descartar, MantenerDescartada, Reabrir, 409.
- `usuarios.spec.ts`: lista (4 usuarios), cambiar rol, desactivar con motivo, activar, reenviar invitación, revisión de privilegios.
- `exportacion.spec.ts`: CSV, GeoJSON con tipo, cabecera exacta del contrato 002, sección Recientes.
- `auditoria.spec.ts`: filtro por categoría, por UUID, por correo; AnomaliaModerada, RolCambiado, DatasetExportado en lenguaje claro.
- `cuenta.spec.ts`: verificar correo, enlace vencido, restablecer contraseña, activar cuenta por invitación.
- `resumen.spec.ts`: carga, cambio de período, navegación a Moderación, cambio de período con clic en etiqueta.

**Accesibilidad y teclado (2 archivos, 13 casos):**
- `accesibilidad.spec.ts`: 0 violaciones axe wcag2a+wcag2aa en 11 rutas + diálogo de moderación + diálogo de invitación.
- `teclado.spec.ts`: 4 flujos solo con teclado (login, filtro del mapa, moderación, revisión mensual).

**Seguridad del navegador (1 archivo, 4 casos):**
- `almacenamiento.spec.ts`: localStorage, sessionStorage, IndexedDB y cookie vacíos tras login.

**Rendimiento contra entorno de pruebas (T087 parcial):**
- `mapa.pruebas.ts`: SC-006 p95 ≤ 3 s en 10 cargas; requiere `PANEL_URL` + `E2E_CORREO` + `E2E_CONTRASENA`.
- Pendiente de ejecutar contra `killauru-pruebas` (entorno desplegado no disponible localmente).

## Simulado vs. integración real

Toda la E2E de esta parte corre contra el build simulado con MSW (modo `build:simulado`). Los tokens, cookies y CORS reales se validan en el backend (003/004) y en el entorno desplegado (`e2e:pruebas`). Las acciones de auditoría registradas en los tests de T081 corresponden a los flujos simulados, no a llamadas reales al backend.

## Pendientes tras esta evidencia

| Tarea | Pendiente |
|---|---|
| T087 | Ejecutar `mapa.pruebas.ts` contra `killauru-pruebas` cuando esté desplegado |
| T088 | Documentar cuentas y datos de prueba para la semilla real |
| T089 | CORS para el bucket R2 |
| T090 | CI completo del panel (Playwright en 3 navegadores + size-limit + Lighthouse CI) |
| T091, T093, T094 | Despliegue, aceptación y cierre global |
