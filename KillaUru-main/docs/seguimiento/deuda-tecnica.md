# Deuda técnica conocida

> Seguimiento actual. [Volver al índice de documentación](../README.md) · [Estado del proyecto](estado-implementacion.md) · [Evidencias](evidencias/README.md)

Problemas **ya demostrados** que hoy no bloquean, pero lo harán cuando crezca el uso. Cada uno indica dónde está
en el código, con qué evidencia se detectó, cómo arreglarlo, qué tarea lo sigue y **qué señal avisa** de que llegó el momento.
Al arreglar uno: cerrar su tarea, mover la fila a «Resueltos» con la fecha y enlazar la evidencia nueva.

| ID | Problema | Prioridad | Tarea | Señal de que ya es un problema |
|---|---|---|---|---|
| DT-01 | El procesamiento es más lento que la llegada en el pico de RNF-03 | 🔴 Alta | 005/T041 (y 004/T053) | `killauru.ingesta.cola_antiguedad_s` > 30 s o `/health/ready` en 503 por pendientes |
| DT-02 | La API tiene 3 pools de conexiones (techo real 90, no 30) | 🟠 Media | 001/T082 | Error 53300 en logs, o más de 30 conexiones de `killauru_app` en `pg_stat_activity` |
| DT-03 | Carrera del *healthcheck* de PostgreSQL en el primer arranque | 🟠 Media | 001/T083 | El migrador termina con `exit 2` al crear o recrear la VM |
| DT-04 | Prueba intermitente `003/ConsolaPrivacidadTests` | 🟡 Baja | 001/T084 | CI en rojo con `ArgumentOutOfRangeException ... chunkLength` |

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

## DT-02 · Tres pools de conexiones (diseño §3.7)

- **Evidencia:** con `Maximum Pool Size=30` se midieron 35 conexiones de la API ([diagnóstico](evidencias/carga/2026-11-22-local-trabajadores-y-pool/README.md)).
- **Dónde está:** cada uno de estos crea un pool propio con tope 30:
  - `backend/src/KillaUru.Api/Configuracion/ConfiguracionExtensions.cs:19`: `NpgsqlDataSource.Create(cadena)` para los chequeos de salud.
  - `backend/src/Modules/KillaUru.Anomalias/AnomaliasModule.cs:25`: `UseNpgsql(cadena, n => n.UseNetTopologySuite())`; la extensión hace que EF cree su propio origen de datos.
  - Auditoría, Dashboard, Identidad e Ingesta: `UseNpgsql(cadena)`; comparten el pool por usar la misma cadena.
- **Arreglo:** un solo `NpgsqlDataSource` (construido con NetTopologySuite) registrado una vez y usado por todos los
  `DbContext` con `UseNpgsql(dataSource)`. Hay que mantener las reglas de capas: el registro va en la composición, no en Domain.
- **Protección actual:** `backend/tests/KillaUru.Arquitectura.Tests/PoolDeConexionesTests.cs` exige el tope en cada cadena.

## DT-03 · Carrera del *healthcheck* de PostgreSQL

- **Evidencia:** en `killauru-local` el migrador falló con `exit 2` en el primer arranque (2026-11-22). La imagen oficial
  levanta un servidor temporal solo por socket y luego se reinicia, y `pg_isready` sin `-h` ya lo da por sano.
- **Dónde está:** `backend/deploy/docker-compose.yml:38` y `backend/deploy/docker-compose.pruebas.yml:39`.
  Ya corregido en `docker-compose.local.yml`.
- **Arreglo:** `pg_isready -h 127.0.0.1 -U postgres -d killauru` en ambos.

## DT-04 · Prueba intermitente de privacidad de consola

- **Evidencia:** falló 1 de 2 veces en la suite completa del 2026-11-21; sola pasa 3/3.
- **Dónde está:** `backend/tests/KillaUru.Integracion.Tests/specs/003-modulo-identidad-accesos/seguridad/ConsolaPrivacidadTests.cs`.
  El `SinkDePrueba` escribe en un `StringWriter`, que no es seguro entre hilos, mientras servicios en segundo plano registran logs.
- **Arreglo:** que el sink escriba con un bloqueo o en una colección concurrente. No reapareció tras quitar las purgas de
  Ingesta de `FabricaApi`, pero eso no demuestra la causa.

## Por confirmar en la VM (`killauru-pruebas`)

Medido solo en local, con k6 en la misma máquina. Repetir antes de cerrar 005/T035, 006/T031 y la fase 10 de 001:
- La velocidad de procesamiento de DT-01, porque el disco de la VM puede confirmar commits más rápido que Docker Desktop.
- Las 29 conexiones TCP cortadas con 2 000 usuarios: se atribuyen al reenvío de puertos de Docker Desktop, sin confirmar.

## Resueltos

| Fecha | Problema | Arreglo | Evidencia |
|---|---|---|---|
| 2026-11-22 | Pool sin tope: 500 por error 53300 bajo ráfaga (§3.7, ARQ-22) | `Maximum Pool Size=30` en las cadenas + `PoolDeConexionesTests` | [diagnóstico](evidencias/carga/2026-11-22-local-trabajadores-y-pool/README.md) |
| 2026-11-21 | Cuerpo de más de 256 KB respondía 500 en Kestrel | `LectorCuerpoLote` con tope configurable | `specs/005-modulo-ingesta/cierre.md` |
| 2026-11-21 | `?severidad=4,5` respondía 400 en el mapa | `ParametrosMapaDto` con listas separadas por comas | `specs/006-modulo-consulta-mapa/cierre.md` |
| 2026-11-21 | Ningún script k6 arrancaba (`comun.js`) | Lectura del corredor como `Feature` | `backend/tests/carga/README.md` |
