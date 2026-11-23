# Calidad del código y de las pruebas (app Android y herramientas de la CDN)

> Medido sobre el código, no declarado. Alcance: lo hecho del 2026-11-22 al 2026-11-23 (cuenta, CDN, mapa) y las
> reglas de la constitución (principio VIII y AGENTS.md). [Volver a evidencias](../../README.md).

## 1. Resultado de las pruebas

| Suite | Resultado | Cómo repetirlo |
|---|---|---|
| App, pruebas JVM (JUnit + Robolectric + MockWebServer) | **406 aprobadas, 0 fallidas, 0 omitidas** en 98 archivos | `cd app; ./gradlew testDebugUnitTest` · informe HTML en `app/build/reports/tests/testDebugUnitTest/index.html` |
| App, cobertura del dominio (Kover) | **97,2 % de líneas, 86,6 % de ramas** (mínimo exigido 90 %) | `./gradlew koverXmlReport koverVerify` · `app/build/reports/kover/` |
| App, análisis estático | lint **sin hallazgos** (avisos = errores); detekt **sin hallazgos** | `./gradlew lintDebug detekt` |
| Herramientas de la CDN (Python) | **6 aprobadas**, validadas contra el YAML del contrato | `python -m unittest discover -s herramientas/cdn` |
| Polígono del tramo demo (Python, backend) | **5 aprobadas** | desde `backend`: `python -m unittest discover -s db/datos -p "test_*.py"` |
| Backend, módulo Mapa (.NET) | **52 aprobadas** con el polígono nuevo | `dotnet test tests/KillaUru.Mapa.Tests` |

La cobertura de Kover mide **solo el dominio** (reglas de negocio). Datos e interfaz tienen pruebas, pero ese
porcentaje no las incluye.

## 2. Reglas comprobadas con herramientas

| Regla | Cómo se comprobó | Resultado |
|---|---|---|
| Patrón AAA en cada prueba | script que busca `Arrange`/`Assert` en el cuerpo de cada `@Test` | **406 de 406**. Había 15 sin AAA en pruebas antiguas: 5 probaban varias cosas a la vez y se partieron en 14 pruebas de un solo comportamiento; una era tautológica (`assertEquals(15, 3 * 5)`) y se borró |
| Métodos ≤ 20 líneas | detekt `LongMethod` (Kotlin); `ast` de Python | Sin excepciones. Había 19 funciones largas (17 de la sesión anterior) y se partieron |
| Archivos < 200 líneas | `TamanoArchivosTest` (Kotlin); script (Python) | Sin excepciones |
| Nombres claros | búsqueda de variables de una letra en el código de estas sesiones | Corregidos `r`, `m`, `s`, `p`, `a`, `c`, `i`. Se dejan `x`, `y`, `t`, `dx` en fórmulas geométricas (`Corredor`, `Geodesia`), donde son la convención |
| Pruebas registradas | todo `*Test.kt` aparece en `specs/008-app-android/pruebas.md` | Sí, con el conteo por carpeta recalculado |

## 3. TDD: lo que se hizo, contado con honestidad

| Cómo se escribió | Pruebas |
|---|---|
| **Rojo visto** (la prueba se ejecutó y falló antes del código) | `test_construir_tramo_demo`, `test_datos_cdn`, `ContratoCuentaTest`, `CasosCuentaNuevosTest`, `CuentaViewModelsTest`, `MapaSinConexionPantallaTest`, `DescargaViewModelTest`, `MapaViewModelTest` (la carrera v1/v2: falló con «v1» y luego se corrigió), `EstadoTrabajoMapaTest` |
| **Prueba escrita antes**, pero el rojo fue solo de compilación (no se ejecutó aparte) | `ContratoManifiestoMapaTest`, `ActualizarMapaUseCaseTest`, `ArchivosMapaLocalTest`, `CerrarSesionUseCaseTest`, `EnviarRevocacionPendienteTest`, `IniciarAppTest`, `CargadoresMapaTest`, `EnDemoAnomaliasTest`, `EstiloMapaLocalTest`, `NavGrafoCuentaTest`, `CuentaUiTest` |
| **Prueba en el mismo paso o después del código** | `FormatoNumeroTest` (después), `RedSeguraTest`, `CampoFormularioTest` (mismo paso) |

**Refactor** después de cada verde: funciones partidas por detekt, clases separadas por archivo (AGENTS.md), cliente
de la CDN separado del de la API, traducción de WorkManager convertida en función pura, nombres cortos renombrados,
API de prueba simplificada (`proyectar` en lugar de una función con tres tipos de retorno).

## 4. Diseño

- **Arquitectura limpia (puertos y adaptadores):** el dominio define interfaces y no conoce Android ni HTTP —
  `CdnMapa`, `ArchivosMapa`, `DescargaMapa`, `FuenteZonaDemo`, `FuenteCorredor`, `FuenteEstiloMapa`,
  `RevocacionPendiente`, `ProgramadorSincronizacion`. Los datos las implementan (`CdnMapaHttp`, `ArchivosMapaLocal`,
  `DescargaMapaWorkManager`…) y Hilt las une. Por eso los casos de uso se prueban con dobles en memoria.
- **Patrones con un problema concreto:** caso de uso (`ActualizarMapaUseCase`, `CerrarSesionUseCase`, `IniciarApp`),
  repositorio, mapeador (`MapeadorManifiesto`), adaptador (`CdnMapaHttp`), estrategia (`PoliticaDescarga`), observador
  (`Flow`/`StateFlow`), inyección de dependencias (Hilt) y estados como tipos sellados (`ResultadoMapa`,
  `EstadoDescarga`, `RespuestaManifiesto`) en lugar de banderas sueltas.
- **KISS:** funciones puras para lo que no necesita Android (`TextosDescarga`, `FormatoNumero`,
  `EstiloMapaLocal.adaptar`, `estadoDescarga`); un archivo por responsabilidad; sin capas que no resuelvan nada.
- **Comentarios:** cada clase pública lleva un KDoc o docstring que dice **para qué** sirve y qué requisito cumple (FR,
  RN, R-xxx). Los comentarios dentro del código explican decisiones no obvias (p. ej. por qué se guarda el token
  cifrado y no en WorkManager, o por qué la CDN tiene su propio cliente).
- **Contrato como fuente única:** generadores Python y app Kotlin se validan contra el mismo `cdn.openapi.yaml`.

## 5. Lo que todavía no tiene prueba automática (y por qué)

| Qué | Por qué | Cómo se verificó |
|---|---|---|
| Dibujo del mapa con MapLibre (`MapaPantalla`, `CapasMapa`) | MapLibre necesita OpenGL; no corre en la JVM | En el emulador (capturas en [2026-11-23-cdn](../2026-11-23-cdn/README.md)) |
| `DescargarMapaWorker`, `RevocacionPendienteWorkManager` | requieren `work-testing` (pendiente) | Su lógica está fuera: `ActualizarMapaUseCase`, `EnviarRevocacionPendiente`, `estadoDescarga`, todas probadas |
| `AlmacenRevocacionCifrado` | usa el Keystore de Android | Indirecto: el cifrado lo cubre `SerializadorCifradoTest` |
| `generar_pmtiles.sh` | descarga de Internet | Se ejecutó a mano (2,3 MB, zoom 0–14) y en el flujo `app-datos-r2` |
| Pruebas instrumentadas (`androidTest`) | la carpeta aún no existe en el proyecto | Pendiente del plan (T066, T122) |

## 6. Fallos que las pruebas y la revisión encontraron (todos corregidos)

Ver [cuenta](../2026-11-22-app-cuenta/README.md) y [CDN](../2026-11-23-cdn/README.md). Los más graves: la app enviaba
el token y el seudónimo a la CDN pública; la sincronización y la purga nunca se programaban; cerrar sesión no
revocaba en el servidor ni borraba el historial; `enDemo` nunca se calculaba; una carga vieja del mapa podía pisar a la
nueva. En las pruebas: hilos de Room y la `Application` real se colaban entre pruebas.
