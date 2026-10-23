# Revisión de implementación · 001-plataforma-backend

Fecha local: 2026-10-22. Alcance: constitución 1.3.0, documentos de referencia, spec, plan, research, data-model, contratos, tareas, backend, despliegue y workflows de la raíz del repositorio.

**Dictamen actualizado (2026-10-23): la base está bien encaminada y respeta la estructura general prevista, pero 001 aún no está completamente cerrado.** La solución compila y su suite actual pasa; quedan diferencias funcionales y verificaciones incompletas en tareas marcadas como terminadas. Los seis módulos vacíos son esperables en esta etapa. Esta revisión modifica comentarios y documentación; los defectos funcionales descritos abajo siguen pendientes.

## Lectura integral y alcance del dictamen

Se leyeron completos los 21 documentos de `docs/`: análisis (actores, historias, requisitos, calidad, restricciones, drivers, ADR, escenarios, reglas y glosario), arquitectura, diseño y los cuatro niveles C4. Sus copias en `KillaUru-main/referencia/docs/` coinciden byte a byte. La evaluación también considera la constitución, el mapa de specs y las decisiones de revisión.

El proyecto es un MVP para una persona: Android detecta y opera sin conexión; el backend recibe eventos durables, aplica reglas y sirve datos por rol. El monolito de seis módulos, sus contratos, el kernel mínimo y la ausencia de brokers adicionales son coherentes con ese propósito. No corresponde exigir a 001 la lógica de Ingesta, confirmación de Anomalías, mapa, identidad o auditoría de los siguientes specs.

El checkpoint de las fases 1–6 permite iniciar los módulos posteriores; es distinto del cierre completo de 001. Las observaciones de HTTP, contratos y arquitectura deben corregirse en esa base. Las de observabilidad y operación pertenecen a sus fases posteriores. Las pruebas de carga y recorrido completo necesitan 004–007: su ejecución pendiente no invalida por sí sola la estructura inicial, aunque sus verificadores deben corregirse antes de usarlos como evidencia. Las prioridades de abajo no significan que todos los hallazgos bloqueen empezar el siguiente módulo.

## Evidencia ejecutada

Desde `KillaUru-main/backend`:

| Comprobación | Resultado |
|---|---|
| `dotnet build KillaUru.sln --no-restore` | Correcta: 0 advertencias, 0 errores |
| `dotnet test --project tests/KillaUru.Shared.Tests --no-build -- --coverage --coverage-output-format cobertura` | 62 pruebas correctas |
| `dotnet test --project tests/KillaUru.Arquitectura.Tests --no-build` | 56 pruebas correctas |
| `dotnet test --project tests/KillaUru.Integracion.Tests --no-build` | 50 pruebas correctas, con Testcontainers |
| `python deploy/ci/cobertura_dominio.py 90` | 100 % del dominio medido: 42/42 líneas |
| Inspección de fuentes `.cs`, `.sh`, `.js`, `.py` | Ninguna alcanza 200 líneas; no se encontraron clases `partial` en fuentes propias |
| API local en Production, con `Api:DocumentacionPublica=true` | `/docs` redirige a `/docs/`, que responde 200; `/openapi/v1.json` responde 200 |
| Petición local a una ruta inexistente | 404 con cuerpo vacío, sin `application/problem+json` |

Docker no respondió al primer diagnóstico, pero estuvo disponible al ejecutar integración y las 50 pruebas finalizaron sin omisiones. La comprobación HTTP usó una clave ficticia, un puerto de loopback y no ejecutó migraciones ni despliegues. El servidor local se detuvo al terminar.

Los 168 casos aprobados acreditan los escenarios existentes, no los ausentes. No se ejecutaron despliegues en Oracle, restauraciones, servicios de Grafana/Cloudflare/GitHub ni pruebas k6. La cobertura actual corresponde al kernel: los seis módulos todavía son esqueletos. No se puede acreditar retrospectivamente que las pruebas se escribieran antes que las reglas.

## Hallazgos prioritarios

P1 = resolver antes de usar la plataforma con datos reales o automatizar producción. P2 = corregir para cerrar los requisitos y evitar regresiones. P3 = completar documentación y coherencia de artefactos.

### R01 · P1 · La protección de datos personales no cubre todas las salidas

**Evidencia:** [FiltroDatosPersonales.cs](../../backend/src/KillaUru.Api/Observabilidad/FiltroDatosPersonales.cs), [ManejadorErrores.cs](../../backend/src/KillaUru.Api/Errores/ManejadorErrores.cs), [TelemetriaExtensions.cs](../../backend/src/KillaUru.Api/Observabilidad/TelemetriaExtensions.cs) y [FiltroDatosPersonalesTests.cs](../../backend/tests/KillaUru.Integracion.Tests/Observabilidad/FiltroDatosPersonalesTests.cs).

El filtro elimina propiedades de primer nivel por nombre. Una propiedad `Detalle` que contenga un objeto con `Correo`, un mensaje con texto interpolado o una excepción con información personal no se sanea. El manejador registra la excepción completa. La instrumentación de trazas solo excluye `/health`; no tiene un procesador que elimine atributos personales. Las pruebas construyen eventos Serilog sintéticos y no capturan logs y trazas de peticiones reales.

**Impacto:** FR-034 y constitución V no quedan garantizados aunque T067 esté marcada como completa. **Corrección:** controlar lo que se registra, sanear estructuras/excepciones y atributos de trazas antes de exportar, y verificar los canales reales con datos sintéticos. Reabrir T047, T048 y T067.

### R02 · P1 · El CD cambia la imagen, pero no lleva los SQL de ese commit a la VM

**Evidencia:** [cd.yml](../../../.github/workflows/cd.yml), [desplegar.sh](../../backend/deploy/desplegar.sh) y [docker-compose.yml](../../backend/deploy/docker-compose.yml).

El job de imagen hace checkout del SHA y lo publica. El job remoto solo llama al `desplegar.sh` ya presente en `/opt/killauru`. El migrador monta `../db` y `deploy` del checkout local; no hay transferencia ni actualización al SHA publicado. Al añadir V0002 a una nueva imagen, la VM puede seguir aplicando únicamente V0000/V0001. La nueva API espera V0002 como recurso y queda en 503 de readiness. Los cambios de Compose y scripts tampoco llegan por este flujo.

**Corrección:** desplegar un paquete de SQL/configuración/scripts del mismo SHA antes de migrar, conservando la versión previa para rollback; probar un despliegue que añada una migración. FR-002/FR-038; reabrir T064/T071.

### R03 · P2 · La cabecera pública no se restringe a peticiones anónimas

**Evidencia:** [CabecerasMiddleware.cs](../../backend/src/KillaUru.Api/Seguridad/CabecerasMiddleware.cs), condición `Contains("public")`.

Si un endpoint escribe `Cache-Control: public`, el middleware lo conserva incluso con usuario autenticado. No consulta `IsAuthenticated`. Todavía no hay endpoints de negocio productivos que exploten ese camino, pero la protección común prometida por FR-013/constitución VI falta antes de implementar Mapa.

**Corrección:** forzar `private, no-store` para respuestas autenticadas y aceptar caché pública únicamente en el caso anónimo explícito; probar ambas identidades sobre la misma ruta. Reabrir T034/T039.

### R04 · P2 · No todos los errores pasan por ProblemDetails

**Evidencia:** [Program.cs](../../backend/src/KillaUru.Api/Program.cs), [ProblemDetailsExtensions.cs](../../backend/src/KillaUru.Api/Errores/ProblemDetailsExtensions.cs). Reproducción local: una ruta inexistente devuelve 404 y cero bytes.

`AddProblemDetails` personaliza los errores que usan su servicio; no convierte automáticamente cualquier respuesta vacía ni todos los resultados MVC. Faltan la integración para estados sin cuerpo y la verificación de errores de validación MVC. Los casos de seguridad y excepción actuales sí están cubiertos.

**Corrección:** unificar 404/405 y errores MVC con el contrato común, preservando los cuerpos de salud definidos en el YAML; añadir casos de validación y rutas inexistentes. FR-012; reabrir T034/T040.

### R05 · P2 · La configuración puede publicar documentación en Production

**Evidencia:** [DocumentacionExtensions.cs](../../backend/src/KillaUru.Api/Documentacion/DocumentacionExtensions.cs), `MapearDocumentacion`. La reproducción con `--environment=Production --Api:DocumentacionPublica=true` devuelve la UI y OpenAPI.

La opción es la única condición. La prueba existente usa el valor false por defecto, por lo que no detecta una variable de entorno que lo sobrescriba. **Corrección:** comprobar también el entorno y probar Production con la opción activada. FR-015; reabrir T041/T043.

### R06 · P2 · OpenAPI y su prueba de contrato están incompletos

**Evidencia:** [TransformadorSeguridad.cs](../../backend/src/KillaUru.Api/Documentacion/TransformadorSeguridad.cs), [ContratoOpenApiTests.cs](../../backend/tests/KillaUru.Integracion.Tests/Contratos/ContratoOpenApiTests.cs) y [ContratoYaml.cs](../../backend/tests/KillaUru.Integracion.Tests/Contratos/ContratoYaml.cs).

El transformador agrega texto y códigos, pero no genera los esquemas `tokenAcceso`/`cookieAcceso`, sus requisitos, el esquema común de errores ni la cabecera Retry-After. La prueba enumera manualmente un solo contrato y compara rutas, métodos y códigos; no comprueba propiedades requeridas, esquemas, ejemplos ni seguridad. La aserción `Superset` sí detecta códigos requeridos ausentes; no es el problema.

**Corrección:** completar T044 y comparar la estructura contractual, con descubrimiento o un registro verificable de features implementadas. No exigir ahora rutas de los módulos 002–007 aún vacíos. FR-016/constitución IX; reabrir T042/T044.

### R07 · P2 · La ventana fija difiere de la decisión de research

**Evidencia:** [LimiteTasaExtensions.cs](../../backend/src/KillaUru.Api/Seguridad/LimiteTasaExtensions.cs), `PorHora`, usa `GetFixedWindowLimiter`. [research.md](research.md), R-004, define una ventana deslizante.

Dos ráfagas a ambos lados de la renovación pueden consumir dos cuotas en un intervalo corto. Las pruebas actuales cuentan peticiones dentro de una sola ventana. RNF-11 fija cuotas por hora, pero no prescribe el algoritmo: esta es una discrepancia con research, no evidencia de que los valores de las cuotas sean incorrectos. **Corrección:** implementar la decisión acordada o reconciliarla explícitamente en los artefactos, y comprobar el borde temporal. FR-010/FR-030/FR-031; revisar T033/T038.

### R08 · P2 · La puerta de cobertura acepta falta de informes y agrega todos los dominios

**Evidencia:** [cobertura_dominio.py](../../backend/deploy/ci/cobertura_dominio.py), rama `if not totales` y cálculo único de porcentaje.

Si no encuentra informes o clases Domain, sale con 0. Cuando existan varios dominios, uno bien cubierto puede compensar otro por debajo del 90 %. Que ahora arroje 100 % no demuestra que la puerta falle ante esos casos.

**Corrección:** fallar cuando falten informes de dominios existentes y evaluar cada dominio; permitir explícitamente módulos todavía vacíos. Hay además una sustitución de coverlet por Microsoft.Testing.Extensions.CodeCoverage documentada en research, pero la constitución y el plan aún dicen coverlet: reconciliar esos artefactos, sin asumir que Cobertura es el mismo recolector. FR-037/constitución VII; revisar T005/T069.

### R09 · P2 · Las pruebas de arquitectura no verifican todas las reglas de legibilidad y dependencias

**Evidencia:** [ReglasDeDominioTests.cs](../../backend/tests/KillaUru.Arquitectura.Tests/ReglasDeDominioTests.cs), [ReglasDeCapasTests.cs](../../backend/tests/KillaUru.Arquitectura.Tests/ReglasDeCapasTests.cs), [LimiteDeLineasTests.cs](../../backend/tests/KillaUru.Arquitectura.Tests/LimiteDeLineasTests.cs).

Las prohibiciones comprueban combinaciones concretas, pero no toda la tabla permitida: Domain podría usar `Shared/Eventos`, `Shared/Persistencia` o su propio Contracts; Adapters podría consumir directamente Contracts ajenos. Tampoco se comprueban visibilidad pública, métodos ≤ 20 líneas, tipos por archivo o clases partial. El script Python de cobertura no entra en la prueba de tamaño. `AutenticacionExtensions.Configurar`, `ValidadorApiOptions.Validate` y `TelemetriaExtensions.AgregarTelemetria` exceden 20 líneas; hay tipos adicionales anidados en `ChequeoConCache` y `ContratoYaml`.

**Corrección:** extraer colaboradores donde corresponda y ampliar las pruebas a las reglas constitucionales, verificando que fallen con infracciones controladas. No se encontraron archivos de 200 líneas ni clases partial actuales. Constitución II/VIII, FR-029/FR-035; revisar T007–T009/T059.

### R10 · Observación para 004 · Separar la definición de severidad alta de la política configurable

**Evidencia:** [Severidad.cs](../../backend/src/KillaUru.Shared/Domain/Severidad.cs), `AltaDesde = 4`, frente a `SeveridadAltaDesde` de RN-06 y de AnomaliasOptions en los documentos de referencia.

El glosario y RN-02 definen severidad alta como 4–5, por lo que esta propiedad coincide con esa definición. Se retira su clasificación anterior como defecto de 001. El diseño interno, sección 6.2, resuelve la confirmación con `PoliticaConfirmacion.EsAlta`, que consulta `AnomaliasOptions.SeveridadAltaDesde`. Al implementar 004 debe mantenerse esa distinción: no sustituir la política configurable por la propiedad fija del kernel. No corresponde reabrir T015 por este motivo.

### R11 · P2 · El primer respaldo diario se omite al iniciar el contenedor

**Evidencia:** [programar.sh](../../backend/deploy/backup/programar.sh). El bucle calcula la próxima hora y duerme antes de llamar a `respaldar.sh`.

Un arranque después de las 02:00 espera hasta el día siguiente. No cumple la ejecución inicial de FR-042/research R-016. Además `desplegar.sh` solo obtiene respaldo cuando hay SQL pendiente, aunque T064 pide respaldo antes del despliegue.

**Corrección:** ejecutar al iniciar, conservar la programación por reloj y verificar el respaldo del flujo de despliegue según su requisito. Reabrir T051/T064.

### R12 · P2 · Un fallo al consultar migraciones pendientes puede aparentar éxito

**Evidencia:** [migrar.sh](../../backend/deploy/migrar.sh), `mapfile -t lista < <(pendientes)`.

La sustitución de proceso no transmite al `mapfile` el código de salida de `psql`. Si falla la consulta, la lista puede quedar vacía y el script sale con «Sin migraciones pendientes». Las pruebas de migración ejecutan SQL mediante otro helper, por lo que no ejercitan este script Bash.

**Corrección:** capturar y comprobar primero el resultado de la consulta; después construir la lista. Probar credenciales incorrectas, servidor inaccesible y lista realmente vacía. FR-002/FR-032; reabrir T031.

### R13 · P2 · Caos y carga pueden aprobar sin demostrar los criterios exigidos

**Evidencia:** [caos.sh](../../backend/tests/carga/caos.sh), [comun.js](../../backend/tests/carga/comun.js), [picos.k6.js](../../backend/tests/carga/picos.k6.js), [estres.k6.js](../../backend/tests/carga/estres.k6.js).

Caos cuenta pendientes viejos y duplicados existentes, pero nunca conserva el conjunto de `eventoId` aceptados por HTTP para compararlo con recepción/procesamiento/descarte. Una BD vacía da ambos conteos en cero. Los umbrales comunes toleran errores inferiores al 1 %, incluidos algunos 429, y Picos los reutiliza aunque exige cero 5xx. Estrés no tiene umbral que haga fallar los checks de respuestas inesperadas y su métrica principal solo cuenta 5xx; 429 puede pasar sin cortar la prueba. El generador usa saltos entre vértices sin medir su distancia, reinicia los tiempos por lote y solo comprueba HTTP 200, que puede contener eventos rechazados.

**Corrección:** conciliación por IDs aceptados, umbrales específicos de cero 429/cero 5xx donde aplique, recorrido plausible y comprobación de aceptados/rechazados. Requiere completar 004–006 para ejecutarlo, pero el defecto del verificador ya es visible. FR-039; reabrir T073/T075/T077; T079 sigue pendiente.

### R14 · P2 · Faltan healthchecks de servicios permanentes

**Evidencia:** [docker-compose.yml](../../backend/deploy/docker-compose.yml) y los Dockerfiles locales.

Postgres define healthcheck y API lo hereda de su imagen. El servicio backup y su Dockerfile no lo definen; cloudflared y Alloy tampoco tienen comprobaciones explícitas en Compose. No se validaron metadatos de esas imágenes remotas en esta revisión.

**Corrección:** definir/verificar comprobaciones útiles de cada servicio permanente y probar sus estados. Mantener la distinción entre healthcheck y reinicio: Compose no reinicia por el mero estado unhealthy. FR-041; revisar T029/T051/T068.

### R15 · P3 · La guía de ejecución y el estado de tareas necesitan reconciliación

**Evidencia:** [quickstart.md](quickstart.md), [tasks.md](tasks.md), [trazabilidad.md](checklists/trazabilidad.md) y READMEs de módulos.

El quickstart usa `--collect:"XPlat Code Coverage"`, mientras la solución selecciona Microsoft.Testing.Platform. Sus comandos Bash aparecen en bloques PowerShell y ejecutan `migrar.sh` en el host sin preparar las herramientas, variables ni acceso a `postgres` de la red Docker; el runbook de despliegue sí usa el contenedor migrador. Los READMEs siguen como plantillas y sus comandos omiten `--project`.

Hay 72 tareas marcadas y 9 abiertas, pero completar un archivo no equivale a cumplir sus criterios. T042, T067 y T077 son ejemplos de verificación parcial marcada como terminada. Se conserva el estado original para que esta revisión no certifique cumplimiento sin las correcciones y pruebas correspondientes.

## Evaluación explícita de la constitución

| Principio | Evaluación |
|---|---|
| I · Fuente y trazabilidad | Artefactos trazados; faltan reconciliar decisiones de implementación y estados reales. R15 |
| II · Monolito y capas | Ocho proyectos productivos y grafo de seis módulos presentes; módulos vacíos. Pruebas de dependencias incompletas. R09 |
| III · Reglas calibrables | TimeProvider y BannedSymbols presentes. EsAlta coincide con el glosario; 004 debe usar su política configurable para confirmar. R10 |
| IV · Datos confiables | Contexto de commit y publicador probados; no se acredita ausencia de pérdidas con el script de caos. R13 |
| V · Privacidad | Protección parcial de logs y sin saneamiento de trazas. R01 |
| VI · Acceso y rastro | Casos actuales 401/403, cookie, cuotas y techos pasan; caché autenticada sin protección común. Auditoría de negocio corresponde a 002. R03/R07 |
| VII · Pruebas | 168 correctas; faltan escenarios de los hallazgos y una puerta de cobertura robusta. R08 |
| VIII · Código legible | Archivos < 200 líneas, sin partial, Nullable y advertencias como errores. Métodos largos y comprobaciones incompletas. R09 |
| IX · Contrato y documentación | XML público del kernel existente y ampliado; errores/OpenAPI/documentación requieren R04–R06 |
| X · Simplicidad y costo | No se añadieron brokers ni servicios externos nuevos. Costo real y operación del plan gratuito no verificados |

## Comentarios corregidos en esta revisión

Se precisaron responsabilidades y límites en transacciones, callbacks, eventos, cancelación, programación por reloj, celdas, severidad, autenticación, caché, tasa, logs, trazas, métricas, errores y OpenAPI. Las implementaciones de IMetricas heredan la documentación del puerto. Los comentarios de pruebas describen los casos que realmente verifican; los scripts ya no prometen por comentario velocidad plausible, rollback medido o conciliación inexistente.

El criterio es documentar propósito, decisiones, precondiciones, unidades, efectos, cancelación y límites. No añadir narraciones línea por línea ni presentar un requisito pendiente como una garantía actual. Una descripción honesta no sustituye la corrección funcional.

## Cierre pendiente

Para consolidar la base de las fases 1–6, resolver seguridad y contratos (R03–R06), reconciliar la decisión de tasa (R07) y reforzar las comprobaciones arquitectónicas (R09). Antes de operar con datos reales o automatizar producción, resolver R01/R02 y completar los controles de cobertura y operación (R08/R11/R12/R14). Corregir los verificadores de sistema (R13) antes de ejecutarlos con los módulos implementados. R10 es una observación para 004, no un defecto actual. Tras cada corrección, agregar el escenario que hoy falta y actualizar tasks/checklists con evidencia.

Las tareas ya abiertas T050, T054, T070, T072, T079, T055, T080, T056 y T057 siguen pendientes: servicios externos, simulacros, protección de main, SSH, carga, README de módulos, recorrido extremo a extremo, quickstart y costo. Las dependencias de 004–007 no son funcionalidades exigibles a los esqueletos de 001; sí deben mantenerse explícitas antes del cierre final.
