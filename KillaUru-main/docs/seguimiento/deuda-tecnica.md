# Deuda técnica conocida

> Seguimiento actual. [Volver al índice de documentación](../README.md) · [Estado del proyecto](estado-implementacion.md) · [Evidencias](evidencias/README.md)

Problemas **ya demostrados** que hoy no bloquean, pero lo harán cuando crezca el uso. Cada uno indica dónde está
en el código, con qué evidencia se detectó, cómo arreglarlo, qué tarea lo sigue y **qué señal avisa** de que llegó el momento.
Al arreglar uno: cerrar su tarea, mover la fila a «Resueltos» con la fecha y enlazar la evidencia nueva.

| ID | Problema | Prioridad | Tarea | Señal de que ya es un problema |
|---|---|---|---|---|
| DT-01 | ~~El procesamiento es más lento que la llegada en el pico de RNF-03~~ **RESUELTO 2026-11-24** | ~~🔴 Alta~~ ✅ | 005/T041 (y 004/T053) | — |
| DT-02 | ~~La API tiene 3 pools de conexiones (techo real 90, no 30)~~ **RESUELTO 2026-11-24** | ~~🟠 Media~~ ✅ | 001/T082 | — |

## DT-01 · Procesamiento más lento que la llegada (RNF-23)

- **Evidencia:** [carga progresiva](evidencias/carga/2026-11-21-local-progresiva-2000/README.md) y
  [diagnóstico](evidencias/carga/2026-11-22-local-trabajadores-y-pool/README.md). Se procesan 77 eventos/s con
  160 000 reportes en la base y 150 eventos/s con la base vacía. En el pico de RNF-03 llegan ~250 eventos/s.
  004/T053 ya había medido un p95 de 186 ms por evento frente a la meta de 100 ms.
- **Qué NO es:** ni CPU, ni bloqueos entre trabajadores (3 %), ni falta de trabajadores. Con 4 u 8 va **más lento**.
- **Dónde está, por orden de impacto medido:**
  1. **Un commit por evento, y otro aparte para marcarlo.** El 38 % del tiempo se espera al disco (`IO:WalSync`).
     La transacción por evento está en `ProcesarReporteUseCase.cs` (`transaccion.EjecutarAsync`, en el módulo de Anomalías),
     y la marca separada en `backend/src/Modules/KillaUru.Ingesta/Application/Procesamiento/ProcesarLoteUseCase.cs:36` (`MarcarAsync`).
  2. **Historial de 15 días cargado en cada evento.** En `backend/src/Modules/KillaUru.Anomalias/Application/Procesamiento/ProcesarReporteUseCase.cs:58`
     (`reportes.DesdeAsync`). Es la explicación más probable de que el procesamiento se frene al crecer los datos.
     **Sin medir:** primero cronometrar esa consulta con y sin datos.
  3. **Unas 10 idas y vueltas a PostgreSQL por evento**, en serie (contexto, bloqueo, ¿existe?, anterior, más cercana, historial, guardar, marcar).
- **Arreglos propuestos, de menor a mayor impacto en el diseño:**
  1. Medir (2) y, si se confirma, leer solo los agregados que usan las reglas (conteos por día y mediana de severidad) en vez de todas las filas.
  2. Unir `MarcarAsync` a la transacción de Anomalías o marcar el lote entero de una vez: un commit menos por evento.
  3. Procesar varios eventos de un lote en una sola transacción. **Toca decisiones** de la spec 004: Anomalías es dueña
     de su transacción por evento, con idempotencia por `eventoId` y bloqueo de celdas. Requiere un ADR nuevo antes de programar.
- **Descartado:** subir `Ingesta:Trabajadores`. Un caché tampoco ataca la espera de disco, que es la parte mayor.

## DT-02 · Tres pools de conexiones (diseño §3.7) — RESUELTO 2026-11-24

- **Evidencia original:** con `Maximum Pool Size=30` se midieron 35 conexiones de la API ([diagnóstico](evidencias/carga/2026-11-22-local-trabajadores-y-pool/README.md)).
- **Dónde estaba:** cada uno de estos creaba un pool propio con tope 30:
  - `ConfiguracionExtensions.cs`: `NpgsqlDataSource.Create(cadena)` para los chequeos de salud.
  - `AnomaliasModule.cs`: `UseNpgsql(cadena, n => n.UseNetTopologySuite())`; EF creaba su propio origen de datos.
  - Auditoría, Dashboard, Identidad e Ingesta: `UseNpgsql(cadena)` con cadena literal.
- **Arreglo aplicado:** `NpgsqlDataSourceBuilder` + `UseNetTopologySuite()` en `ConfiguracionExtensions`; singleton registrado en DI; los 5 módulos reciben la fuente con `sp.GetRequiredService<NpgsqlDataSource>()`. Un solo pool, techo 30 real.
- **Protección nueva:** `backend/tests/KillaUru.Arquitectura.Tests/UnFuenteConexionesTests.cs` (3 pruebas, verde).

## Por confirmar en la VM (`killauru-pruebas`)

Medido solo en local, con k6 en la misma máquina. Repetir antes de cerrar 005/T035, 006/T031 y la fase 10 de 001:
- La velocidad de procesamiento de DT-01, porque el disco de la VM puede confirmar commits más rápido que Docker Desktop.
- Las 29 conexiones TCP cortadas con 2 000 usuarios: se atribuyen al reenvío de puertos de Docker Desktop, sin confirmar.

## Resueltos

| Fecha | Problema | Arreglo | Evidencia |
|---|---|---|---|
| 2026-11-24 | DT-01: procesamiento más lento que la llegada (77 eventos/s vs ~250 eventos/s en pico RNF-03) | **Fix A**: `MarcarLoteAsync` une todos los marcados en 1 transacción WAL por lote (antes: N fsyncs); **Fix B**: `DesdeAsync` proyección slim (5 columnas, sin geometría `Posicion`). 9 tests unitarios nuevos en `KillaUru.Ingesta.Tests`; 005/T041 | specs/005-modulo-ingesta/tasks.md T041 |
| 2026-11-23 | DT-03: carrera del *healthcheck* de PostgreSQL (el migrador salía con `exit 2` al crear la VM) | `pg_isready -h 127.0.0.1` en `docker-compose.yml` y `docker-compose.pruebas.yml` + `ChequeoDeSaludPostgresTests`, que revisa todo compose de `deploy/` (001/T083) | [evidencia](evidencias/pruebas/2026-11-23-backend-parte3/README.md) |
| 2026-11-23 | DT-04: prueba intermitente `ConsolaPrivacidadTests` (`chunkLength`) | La causa no era la escritura (ya tenía candado) sino **leer** el `StringWriter` sin candado mientras el host registraba. `SinkDePrueba` guarda el texto privado y solo se lee con `Capturado`, bajo el mismo candado; `SinkDePruebaTests` lo reproducía 3/3 antes y pasa 5/5 después (001/T084) | [evidencia](evidencias/pruebas/2026-11-23-backend-parte3/README.md) |
| 2026-11-23 | `005/RecepcionApiTests.Lote_de_50…` fallaba de vez en cuando | Ya sondeaba; el tope de 15 s era menor que lo medido (11,3 s en la suite, 17 s con carga, por DT-01). Tope de 60 s y mensaje con los pendientes | [evidencia](evidencias/pruebas/2026-11-23-backend-parte3/README.md) |
| 2026-11-22 | Pool sin tope: 500 por error 53300 bajo ráfaga (§3.7, ARQ-22) | `Maximum Pool Size=30` en las cadenas + `PoolDeConexionesTests` | [diagnóstico](evidencias/carga/2026-11-22-local-trabajadores-y-pool/README.md) |
| 2026-11-21 | Cuerpo de más de 256 KB respondía 500 en Kestrel | `LectorCuerpoLote` con tope configurable | `specs/005-modulo-ingesta/cierre.md` |
| 2026-11-21 | `?severidad=4,5` respondía 400 en el mapa | `ParametrosMapaDto` con listas separadas por comas | `specs/006-modulo-consulta-mapa/cierre.md` |
| 2026-11-21 | Ningún script k6 arrancaba (`comun.js`) | Lectura del corredor como `Feature` | `backend/tests/carga/README.md` |
