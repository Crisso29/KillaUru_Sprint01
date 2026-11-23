# Pruebas 005 — Ingesta

> Enlaces y organización revisados (2026-11-21): [estado y pendientes de cierre](cierre.md). Los requisitos y planes no equivalen a evidencia de implementación o despliegue.

Recepción, idempotencia, procesamiento y mantenimiento de lotes.

[Guía común](../../docs/guias/pruebas.md) · [Requisitos](spec.md) · [Tareas](tasks.md)

## Dónde buscar

| Alcance | Ubicación |
|---|---|
| Reglas y casos de uso | [KillaUru.Ingesta.Tests](../../backend/tests/KillaUru.Ingesta.Tests/) |
| Arquitectura y legibilidad | [Suite compartida](../../backend/tests/KillaUru.Arquitectura.Tests/) |
| Integración por spec | [Carpetas por propósito](../../backend/tests/KillaUru.Integracion.Tests/specs/005-modulo-ingesta/) |
| Contrato HTTP | [Contratos](../../backend/tests/KillaUru.Integracion.Tests/specs/005-modulo-ingesta/contratos/) |
| Carga y resiliencia | [Escenarios k6 compartidos](../../backend/tests/carga/README.md) |
| E2E entre módulos | [Recorrido compartido, pendiente](../../backend/tests/KillaUru.Integracion.Tests/sistema/e2e/FlujoPuntaAPuntaTests.cs) |

## Cómo ejecutar

Desde `KillaUru-main/backend/`:

```powershell
dotnet build KillaUru.sln
dotnet test --project tests/KillaUru.Ingesta.Tests --no-build
dotnet test --project tests/KillaUru.Arquitectura.Tests --no-build
dotnet test --project tests/KillaUru.Integracion.Tests --no-build -- --filter-class "*ContratoIngestaTests"
dotnet test --project tests/KillaUru.Integracion.Tests --no-build -- --filter-namespace "KillaUru.Integracion.Tests.Ingesta"
```

La última línea requiere Docker Desktop encendido (Testcontainers levanta PostgreSQL).
Las pruebas con dobles no certifican PostgreSQL real. El rendimiento local no acredita carga.

## Estado y límites

Contrato HTTP (T034): 4/4 aprobados el 2026-11-21. Comprueba `operationId`, los 7 códigos
(200, 400, 401, 403, 413, 429, 503), los esquemas `Lote`, `Evento` y `RespuestaLote`, y `Retry-After` en el 503.
Compara el OpenAPI **publicado** con el contrato; no envía lotes reales.

Recepción HTTP (T012, cerrada): 17/17 escenarios del quickstart aprobados el 2026-11-21, un archivo por bloque:

| Bloque | Archivo | Escenarios | Estado |
|---|---|---|---|
| A. Recepción e idempotencia | `RecepcionApiTests.cs` | #1, #3, #4, #21 | ✅ 4/4 |
| B. Rechazo por evento | `RecepcionRechazosApiTests.cs` | #2, #5, #6, #17 | ✅ 7/7 casos (el #5 prueba ambos lados de cada límite) |
| C. Lote inválido y límites | `RecepcionLimitesApiTests.cs` | #7, #8, #12 | ✅ 6/6 casos (el #8 sobre Kestrel real) |
| D. Seguridad y seudónimo | `RecepcionSeguridadApiTests.cs` | #9, #10, #11, #18, #19, #20 | ✅ 7/7 casos |

**2026-11-23:** `Lote_de_50_buenos_se_guarda_entero_y_pasa_a_procesado` fallaba de vez en cuando en la suite completa.
Ya sondeaba la base cada 200 ms; el problema era el tope de 15 s: un lote lo procesa un solo trabajador, evento tras
evento, a ~186 ms (DT-01), y se midieron 11,3 s en la suite y 17 s con el equipo cargado. El tope pasa a 60 s (no
alarga la prueba: el sondeo sale en cuanto termina) y el mensaje de falla dice cuántos eventos siguen sin procesar.

Envían lotes por HTTP a la API en memoria con PostgreSQL real y comprueban la respuesta **y** las filas de
`ingesta.evento_recibido`. El #1 espera, con un plazo de 15 s, a que el procesamiento real deje las filas en Procesado.
Se verificó por mutación que detectan errores: anular la marca de duplicados entre envíos hace fallar el #3,
anular la de dentro del lote hace fallar el #4, y volver exclusivos los límites de RN-11 (`>=` 50 m, `<=` 0,75)
hace fallar justo los dos casos de borde del #5.
El bloque C empezó en rojo (0/6) y halló errores reales, ya corregidos y descritos en [cierre.md](cierre.md):
el 300 KB respondía 500 en Kestrel, un cuerpo no JSON salía `datos_invalidos` y los errores no eran `problem+json`.
El #12 usa la cola real con `CapacidadCola = 1` y sin procesador ni barredor, para que nadie la vacíe.
En el bloque D, quitar la comprobación de `verificado` hace fallar el #20, y quitar el bloqueo del
administrador hace fallar el #11. Los tokens llevan el claim `verificado` igual que `EmisorJwt` de Identidad.
Siguen pendientes las pruebas con PostgreSQL y HTTP de T011, T020, T024, T027, T030 y T032.
Las pruebas de otros módulos y el código implementado no sustituyen esa evidencia.
El E2E compartido conserva dos casos omitidos con pasos sin implementar.
La ejecución de k6 y las validaciones operacionales requieren evidencia del entorno de pruebas.
Consulta [la revisión actual](../../docs/seguimiento/estado-implementacion.md) antes de cerrar tareas.

## Al añadir un caso

Citar `005/FR-...`, nombrar escenario y resultado, y separar Arrange / Act / Assert.
Asignar un propietario; enlazar desde otras specs cuando el flujo sea compartido.
Registrar comando, fecha, commit, entorno, resultado y omitidos.

## Inventario de archivos de prueba

Abre una categoría para ir al archivo exacto. Se listan archivos con pruebas, no
helpers ni resultados de ejecución. Los comandos anteriores indican cómo ejecutarlos.
Las suites de arquitectura, carga y E2E compartidas se detallan en [pruebas de plataforma](../001-plataforma-backend/pruebas.md#inventario-de-archivos-de-prueba).

<details>
<summary>Casos de uso (7 archivos)</summary>

- [BarrerPendientesUseCaseTests.cs](../../backend/tests/KillaUru.Ingesta.Tests/Application/BarrerPendientesUseCaseTests.cs)
- [LectorEventoTests.cs](../../backend/tests/KillaUru.Ingesta.Tests/Application/LectorEventoTests.cs)
- [LoteAceptadoTests.cs](../../backend/tests/KillaUru.Ingesta.Tests/Application/LoteAceptadoTests.cs)
- [ProcesarLoteUseCaseTests.cs](../../backend/tests/KillaUru.Ingesta.Tests/Application/ProcesarLoteUseCaseTests.cs)
- [RecibirLoteEventosUseCaseTests.cs](../../backend/tests/KillaUru.Ingesta.Tests/Application/RecibirLoteEventosUseCaseTests.cs)
- [RecibirLoteEventosUseCaseTests_Procesamiento.cs](../../backend/tests/KillaUru.Ingesta.Tests/Application/RecibirLoteEventosUseCaseTests_Procesamiento.cs)
- [SeudonimoDelLoteTests.cs](../../backend/tests/KillaUru.Ingesta.Tests/Application/SeudonimoDelLoteTests.cs)

</details>

<details>
<summary>Reglas de dominio (1 archivo)</summary>

- [HoraDeDeteccionTests.cs](../../backend/tests/KillaUru.Ingesta.Tests/Domain/HoraDeDeteccionTests.cs)

</details>

<details>
<summary>Integración HTTP y PostgreSQL (4 archivos)</summary>

- [RecepcionApiTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/005-modulo-ingesta/integracion/RecepcionApiTests.cs)
- [RecepcionRechazosApiTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/005-modulo-ingesta/integracion/RecepcionRechazosApiTests.cs)
- [RecepcionLimitesApiTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/005-modulo-ingesta/integracion/RecepcionLimitesApiTests.cs)
- [RecepcionSeguridadApiTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/005-modulo-ingesta/integracion/RecepcionSeguridadApiTests.cs)

La preparación está en [soporte/](../../backend/tests/KillaUru.Integracion.Tests/specs/005-modulo-ingesta/soporte/):
`LoteDePrueba` arma lotes válidos, `EnvioLote` los envía, `RecepcionEnBd` lee las filas guardadas
y `ProblemaLeido` exige `problem+json` y devuelve el `codigo` del error.

</details>

<details>
<summary>Adaptadores sin host (1 archivo)</summary>

- [LectorCuerpoLoteTests.cs](../../backend/tests/KillaUru.Ingesta.Tests/Adapters/LectorCuerpoLoteTests.cs): tope de bytes inclusivo, `Content-Length`, corte de Kestrel y cuerpos inválidos.

</details>

<details>
<summary>Contratos HTTP (1 archivo)</summary>

- [ContratoIngestaTests.cs](../../backend/tests/KillaUru.Integracion.Tests/specs/005-modulo-ingesta/contratos/ContratoIngestaTests.cs)

</details>

<details>
<summary>Adaptadores con dobles o componentes locales (1 archivo)</summary>

- [TareasPorRelojTests.cs](../../backend/tests/KillaUru.Ingesta.Tests/Infrastructure/TareasPorRelojTests.cs)

</details>
