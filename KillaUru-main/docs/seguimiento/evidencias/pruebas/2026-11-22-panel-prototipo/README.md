# Panel: fidelidad al prototipo

> El panel real, vista por vista, junto al prototipo `referencia/prototipo/killauru-diseno.html`. [Volver a evidencias](../../README.md).

| Dato | Valor |
|---|---|
| Fecha | 2026-11-22 |
| Entorno | `npm run build:simulado` + `vite preview` (API simulada con MSW), Chromium de Playwright a 1600 × 1100 |
| Cómo se hicieron | Las del prototipo, pulsando su propia navegación (`#opsJump`); las reales, entrando con `ops@killauru.pe` |
| Pruebas automáticas | 416 Vitest, ESLint, `tsc -b` y las nuevas de arquitectura y contrato (ver [pruebas de la spec 010](../../../../../specs/010-panel-admin/pruebas.md)) |

## Vistas

| Vista | Prototipo | Real |
|---|---|---|
| Inicio de sesión | [prototipo](prototipo-01-login.png) | [real](real-01-login.png) |
| Resumen | [prototipo](prototipo-02-resumen.png) | [real](real-02-resumen.png) |
| Mapa | [prototipo](prototipo-03-mapa.png) | [real](real-03-mapa.png) |
| Moderación | [prototipo](prototipo-04-moderacion.png) | [real](real-04-moderacion.png) |
| Usuarios | [prototipo](prototipo-05-usuarios.png) | [real](real-05-usuarios.png) |
| Auditoría | [prototipo](prototipo-06-auditoria.png) | [real](real-06-auditoria.png) |
| Exportaciones | [prototipo](prototipo-07-exportaciones.png) | [real](real-07-exportacion.png) |
| Marca elegida («Señal», opción C) | [hoja de logos](prototipo-logos.png) | logo en la barra lateral y en el login |

## Qué estaba mal antes y se corrigió

- **Sin estilos:** cinco hojas CSS no las importaba nadie y `Boton` usaba clases inexistentes; el CSS del prototipo se portó con sus
  mismos nombres de clase (`compartido/estilos/componentes.css` y `ops.css`).
- **Sin armazón:** `Marco` existía pero no se montaba; ahora es la ruta de diseño de todas las páginas protegidas.
- **El Mapa nunca abría:** carga perezosa doble (error #306 de React), capas usadas antes de cargar el estilo, eventos sobre la
  fuente en vez de la capa, e íconos sin preparar. Los marcadores ahora siguen FR-023 (círculo con marca por estado).
- **Contrato de identidad:** `iniciar` → `inicio-sesion`, `cerrar` → `cerrar-sesion`, respuesta `SesionPanel` en camelCase.
- **10 variables CSS inexistentes** en 20 componentes (`--border`, `--error`…): bordes y errores en rojo no se veían.
- Enlaces rotos (`/login/recuperar`), `<main>` anidados, la franja del corredor y la leyenda sin mostrar, campos sin `name`.

## Diferencias que quedan, a propósito o por dependencia

- **Mapa base:** en modo simulado no hay CDN; el mapa muestra las anomalías sobre un fondo neutro y lo avisa en el pie.
  Con el CDN desplegado se ve el relieve y la vía (FR-021).
- **Selector de periodo global** de la barra superior: el prototipo lo comparte entre vistas; aquí cada vista tiene el suyo
  (Resumen 7/14/30 d, Mapa 24 h/7 d/30 d), porque los contratos definen periodos distintos por vista.
- **«Actualizado hh:mm:ss»** de la barra superior: falta la fuente de esa hora en los contratos.
- **`s2` y `s3`** más oscuros que en el prototipo para llegar a 3:1 sobre blanco (WCAG 1.4.11), igual en la app.
