# Simulacros previos a cada versión

## Checklist de verificación pre-producción

- [ ] Implementar los casos `*.pruebas.ts` y configurar Playwright contra «killauru-pruebas» sin MSW; actualmente `e2e:pruebas` no acredita backend real.
- [ ] Ejecutar `npm run e2e:pruebas` después de esa preparación; registrar SHA, entorno y resultados, incluidos omitidos.
- [ ] Revisión manual con NVDA + Firefox (accesibilidad lectores de pantalla)
- [ ] Mozilla HTTP Observatory — puntuación ≥ A en `https://panel.killauru.pe`
- [ ] Verificar cabeceras: `node scripts/verificar-cabeceras.mjs`

## Resultados (anotar por versión)

Este runbook complementa los [simulacros del backend](../../backend/deploy/runbooks/simulacros.md).
Seguir las puertas de [cierre y despliegue](../../docs/guias/cierre-y-despliegue.md).
El script actual de cabeceras comprueba presencia de cinco cabeceras, no valores ni todo SC-003.

| Versión | Fecha | E2E | NVDA | Observatory | Cabeceras | Notas |
|---|---|---|---|---|---|---|
| v0.1.0 | (pendiente) | — | — | — | — | MVP inicial |
