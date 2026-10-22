# Quickstart: comprobar el panel «KillaUru Ops»

## Requisitos previos

Node 22 LTS. Para el modo real: «killauru-pruebas» levantado (001 R-012) con 003, 004, 006 y 007, `Api:OrigenPanel = http://localhost:4173` y claves de prueba de Turnstile (H-02), y las cuentas y datos de `panel/README.md` (T088). Para el mapa base: URL del estilo y del PMTiles en `cdn.killauru.pe` (H-04); sin ellas, el mapa muestra la lista y el aviso.

## 1. Pruebas automáticas

```powershell
cd panel
npm ci
npm run tipos; git diff --exit-code src/api/generado      # tipos al día con los YAML
npm run lint; npm run deps                                  # ESLint y regla de dependencia
npm test                                                    # unitarias, componentes, contrato, líneas, contraste, cabeceras
npm run cobertura                                           # modelo/ ≥ 90 %
npm run build; npm run build:simulado; npm run size         # 120 KB inicial · 300 KB mapa
npm run e2e                                                 # Playwright simulado + axe, 3 navegadores
```

## 2. Escenarios que deben pasar

| # | Escenario | Esperado |
|---|---|---|
| 1 | Entrar con un administrador | `POST /auth/inicio-sesion` con `canal: "panel"` y `turnstileToken`; abre «Resumen»; «Sesión hasta HH:MM» |
| 2 | Contraseñas malas seguidas (modo real) | «Correo o contraseña incorrectos.» hasta que la API bloquea (RN-19); en cuanto responde 423: «Cuenta bloqueada 5 min por intentos fallidos. Vuelve a intentar a las HH:MM.» con el formulario desactivado hasta esa hora |
| 3 | Entrar con un conductor | «Esta cuenta no tiene acceso al panel.» |
| 4 | Borrar la cookie `ku_acceso` y cambiar de pantalla | 1 `POST /auth/renovar` y la pantalla carga sin pasar por el login |
| 5 | Revocar la sesión en la API y cambiar de pantalla | login con «Tu sesión terminó. Vuelve a entrar.» |
| 6 | Dos pestañas abiertas; «Salir» en una | las dos en el login |
| 7 | Abrir `/mapa` sin parámetros | la petición lleva `estado=Activa,PendienteConfirmacion`; el filtro muestra esas dos marcadas (D6) |
| 8 | Lista del mapa | «Bache · km 287,4», id corto, «hace 2 h»; ninguna anomalía solo por color |
| 9 | Detalle de una `Reparada` | «Reabrir» habilitada; «Marcar reparada» deshabilitada con «No disponible desde Reparada. Solo desde: Activa, Pendiente de confirmar.» |
| 10 | Marcar reparada | diálogo del prototipo; detalle actualizado; aparece en «Últimos cambios» y en «Auditoría» («Marcó reparada») |
| 11 | «Por revisar» → «Mantener descartada» | sale de la lista; auditoría «Mantuvo descartada» |
| 12 | Resumen con un indicador `null` | «—» y «Sin calcular»; «Estado del servicio» sin cifras |
| 13 | Usuarios: desactivar mi propia cuenta | la opción no aparece; forzada por la API → 409 con su texto |
| 14 | Revisión mensual | `POST` con `confirmados` y `retirados`; aviso con la nueva fecha de vencimiento |
| 15 | Auditoría con actor «admin.operaciones@killauru.pe» | 1 `GET /usuarios?busqueda=` + `GET /auditoria?actorId=…` |
| 16 | Exportar CSV | archivo `killauru-anomalias-AAAAMMDD-AAAAMMDD.csv` con la cabecera exacta y sin `seudonimo`; aparece en «Recientes» |
| 17 | `/cuenta/verificar#token=…` | el fragmento desaparece de la barra; «Listo, tu correo está verificado.»; repetir → «El enlace no es válido o ya venció…» |
| 18 | Revisar el almacenamiento del navegador | `localStorage`, `sessionStorage` e IndexedDB vacíos; `document.cookie` sin `ku_` |

## 3. Cabeceras y despliegue

```powershell
curl.exe -sI https://panel.killauru.pe/ | Select-String "strict-transport|content-security|x-frame|x-content-type|referrer-policy|permissions-policy|cross-origin-opener"
node scripts/verificar-cabeceras.mjs https://panel.killauru.pe
```

- Las 7 cabeceras presentes; CSP sin `unsafe-inline`/`unsafe-eval`; Mozilla HTTP Observatory ≥ A (SC-003).
- En DevTools → Red: el PMTiles se pide con `Range` y responde 206 desde `cdn.killauru.pe` (ADR-011).
- Volver atrás: Cloudflare Pages → `killauru-panel` → despliegue anterior → «Rollback» (< 5 min).

## 4. Calidad

| Atributo | Cómo se mide | Meta |
|---|---|---|
| Accesibilidad | axe por ruta (`accesibilidad.spec.ts`), teclado (`teclado.spec.ts`), NVDA + Firefox a mano | 0 violaciones WCAG 2.1 A/AA (SC-004) |
| Rendimiento | `size-limit`; `rendimiento.spec.ts` (marca `mapa-listo`, 10 cargas); Lighthouse CI | ≤ 120 / 300 KB gzip; ≤ 3 s p95; LCP ≤ 2,5 s (SC-005, SC-006) |
| Seguridad | `cabeceras.test.ts`, `verificar-cabeceras.mjs`, Observatory, `almacenamiento.spec.ts` | SC-002, SC-003 |
| Compatibilidad | Playwright en Chromium, Firefox y Edge | SC-009 |
| Mantenibilidad | `lineas.test.ts`, `dependency-cruiser`, cobertura de `modelo/` | SC-010 |
| Contrato | tipos al día + Ajv sobre los datos simulados | SC-011 |
