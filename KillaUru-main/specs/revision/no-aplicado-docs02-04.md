# No aplicado en docs 02–04 y constitución (2026-10-21)

| # | Hallazgo / punto | Motivo |
|---|---|---|
| 1 | `workspace.dsl` (Alloy como contenedor, GitHub Actions/GHCR como sistema externo, firmas nuevas) | No existe en `04-modelo-C4/` (se eliminó en el commit 5d43951). Los cambios se hicieron en Nivel1–4 (mermaid y tablas). `componentes` §10 y `enfoque` §11 aún citan `workspace.dsl`: no se tocó para no cambiar otra cosa; decidir si se quita la mención. |
| 2 | Imágenes SVG (`VistaZonas.svg`, `C3_App.svg`) | No se editan archivos de imagen; el texto y los mermaid ya reflejan Alloy, GHCR y GitHub Actions. |
| 3 | ARQ-17: ¿Domain puede usar el `Contracts/` de **su propio** módulo? | Sin decisión del autor. Se aplicó solo «Domain no usa contratos de **otro** módulo» (D4). |
| 4 | ARQ-20: controladores «≤ 15 líneas» vs «≤ 15 líneas por acción» | Sin decisión del autor; `principios` §6 y la constitución quedan como estaban. |
| 5 | ARQ-15 (`IMetricas` con primitivos y 6 operaciones nuevas; `ContarEnLaHoraAsync` en `IPendientesRepositorio`) y ARQ-16 (`ResultadoMapa`, `IVersionesCelda`, `ZonaDemo.Clasificar`) en Nivel4/componentes | El plan no los lista para docs; solo se agregó `RegistrarRetrasoCola(TimeSpan)` (D11). Aplicarlos cambiaría varias firmas de Nivel4. |
| 6 | Extras de Identidad de 003 en `diseño-interno` §3.4 (`hash_contrasena` NULL, `ventana_fallos_desde_utc`, `creado_utc`/`limite_absoluto_utc` en sesión, `token_correo.proposito = 3`, tabla `revision_privilegios`, índices FK de BD-05) | El plan para §3.4 solo pide horas truncadas y purgas (D3); el resto no tiene decisión ni fila en el plan. |
| 7 | `ix_anomalia_pendiente` (004) | T-BD-CHK lo deja a criterio («quitar si sobra»); docs no lo tenía y no se agregó. |
| 8 | BD-11 (bloqueo de dos claves), BD-12 (orden por seudónimo entre lotes), BD-14 (autovacuum y volumen ~10 GB), BD-15 (versión de PostgreSQL), BD-18 (total de tramos desde la polilínea), ARQ-22 (pool y `postgresql.conf` en 001) | Sin decisión del autor y fuera del plan; §3.6/§3.7 conservan sus valores. |
| 9 | Purga de recepción solo de estados 1 y 2 (BD-13) | Es decisión de la spec 005 sin decisión del autor; `diseño-interno` §3.8 conserva la forma general (solo se cambió `now()` por `@ahora`). |
| 10 | Fila duplicada «Administrador» en Nivel1 §3 | Preexistente; no forma parte del plan. |
