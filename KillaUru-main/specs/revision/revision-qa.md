# Revisión QA / rendimiento — KillaUru (specs 001…007 frente a `referencia/docs`)

> Revisor: QA lead / pruebas y rendimiento · Fecha: 2026-10-21 · Modo: **solo lectura** (no se editó ningún archivo del proyecto).
> Rutas relativas a `KillaUru-main/`. Abreviaturas: `04` = `referencia/docs/01-analisis-del-sistema/04-atributos-de-calidad.md`, `08` = `…/08-escenarios-y-atributos-de-calidad.md`, `05` = `…/05-restricciones.md`, `enfoque` = `referencia/docs/02-arquitectura-de-software/enfoque-arquitectonico.md`, `DIM` = `referencia/docs/03-diseño-de-software/diseño-interno-de-modulos.md`, `const` = `.specify/memory/constitution.md`.
> Leyenda de estado: ✅ cubierto y medible · ⚠️ parcial (falta parte del umbral, el entorno, la frecuencia o la medición) · ❌ sin dueño o no se mide · 📱 es de la app o del panel (sin spec todavía).

---

## 1. Matriz RNF → spec → tarea → cómo se mide

| RNF | Meta en docs (`04`) | Spec / SC | Tarea | Herramienta · entorno · frecuencia | ¿Umbral = docs? | Estado |
|---|---|---|---|---|---|---|
| RNF-01 | p95 < 250 ms en la API **y** < 300 ms de extremo a extremo desde Lima; ≥ 80 % de aciertos anónimos en el borde (`04:12`) | 006 SC-001, SC-002 | 006 T031 (k6), T030 (un `HIT`) | k6 300 VU · entorno no definido · una vez | 250 ms ✓ · **300 ms desde Lima: no aparece** · 80 %: se declara pero ninguna tarea mide la tasa | ⚠️ |
| RNF-02 | Lote de 50 < 300 ms p95 (`04:13`) | 005 SC-001 | 005 T035 | k6 `lotes.k6.js` · entorno no definido · una vez | ✓ | ⚠️ (entorno) |
| RNF-03 | 300 dispositivos sincronizando **y** 300 consultando el mapa, ≥ 10 min, error < 1 %, RNF-02 se mantiene (`04:14`) | 005 SC-002 | 005 T035 (solo lotes) · 006 T031 (solo mapa) | Dos scripts separados; nadie corre el perfil **combinado** | Falta la mitad del perfil (300 lectores a la vez) | ⚠️ |
| RNF-04 | Inferencia < 50 ms p95 en ≥ 3 celulares (`04:15`) | — | — | Benchmark en el celular | — | 📱 ❌ |
| RNF-35 | Precision/recall ≥ 0,90 por clase; severidad ± 1 en ≥ 80 % (`04:16`) | — | — | Matriz de confusión | — | 📱 ❌ |
| RNF-23 | ≤ 100 ms p95 por evento **y** retraso de cola ≤ 30 s p95 (`04:17`) | 004 SC-001 · 005 SC-002 | 004 T053 · 005 T035 | Histograma `killauru.anomalias.proceso_ms` (001 `data-model.md` §3.2) ✓ · retraso de cola: **solo hay *gauges*** (tamaño y antigüedad del más viejo), sin distribución | 100 ms ✓ · 30 s p95: **no se puede calcular** (QA-06) | ⚠️ |
| RNF-05 | ≥ 99 % mensual, UptimeRobot (`04:23`) | 001 SC-007 | 001 T050 (configurar) | UptimeRobot · producción · mensual: **sin tarea de registro mensual** | ✓ | ⚠️ |
| RNF-06 | 0 aceptados perdidos; conciliación; reanudación ≤ 2 min; prueba de reinicio (`04:24`) | 005 SC-003 · 004 SC-002 | 005 T027 · 004 T027 | Testcontainers en proceso, 500 eventos, «detener el *host*» | ✓ (0 y 2 min) · pero no es caída real ni hay carga (QA-08) | ⚠️ |
| RNF-07 | Backup diario cifrado en R2, 30 días, **restauración mensual** (`04:25`) | 001 FR-027 | 001 T051, T054 | Simulacro en «killauru-pruebas» · **una vez** | ✓ valores · frecuencia mensual sin plan | ⚠️ |
| RNF-24 | RPO ≤ 24 h, RTO ≤ 2 h (BD), ≤ 4 h (VM) (`04:26`) | 001 SC-006 | 001 T054 (solo vuelta atrás y RTO 2 h) | Simulacro · una vez | ✓ en SC · **T054 no mide RPO ni la recreación de la VM** | ⚠️ |
| RNF-33 | 5 h sin conexión, 7 días sin pérdida, sincroniza en ≤ 60 s (`04:27`) | — | — | Prueba de campo | — | 📱 ❌ |
| RNF-08 | TLS 1.3 (mín. 1.2), SSL Labs A+, HSTS en `api`, `panel`, `cdn` (`04:33`) | 001 FR-014 | 001 T034 (cabecera HSTS en proceso) | **Ningún escaneo SSL Labs** | — | ❌ |
| RNF-09 | HSTS, CSP, XFO, nosniff en el panel (`04:34`) | 001 FR-014 (HSTS y nosniff de la API); CSP/XFO → «feature del panel» (001 `spec.md:238`) | 001 T034 | `WebApplicationFactory` | Parte API ✓ · parte panel sin spec | ⚠️ 📱 |
| RNF-10 | Ningún vínculo persona ↔ eventos (`04:35`) | 003 SC-003 · 002 SC-002 · 004 FR-023 · 005 FR-019 · 007 SC-002 | 003 T056 · 004 T040 · 005 T030 · 007 T022 | Auditoría del esquema con Testcontainers | ✓ | ✅ |
| RNF-11 | 60 consultas/h + 120 lotes/h (anónimo, seudónimo + IP), 1 000 y 3 000 req/h, 429 + `Retry-After`, IP de `CF-Connecting-IP` solo por el túnel (`04:36`) | 001 FR-010, FR-011 | 001 T033 | `WebApplicationFactory` | 60 ✓ (61.ª → 429) · 120/1 000/3 000 **sin valor límite** · rotar el seudónimo no se prueba (QA-15) | ⚠️ |
| RNF-25 | Seudónimo purgado a 72 h en reportes y recepción; recepción ≤ 30 días (`04:37`) | 004 SC-005 · 005 SC-005 | 004 T039 · 005 T030 | Testcontainers + `FakeTimeProvider` + consulta de verificación | ✓ | ✅ |
| RNF-26 | Sin acceso a la API en ≤ 5 min (`04:38`) | 003 SC-001 | 003 T036 | `WebApplicationFactory` (de punta a punta) | ✓ · la parte sin conexión (RN-21) es de la app | ✅ / 📱 |
| RNF-27 | 100 % de respuestas anónimas recortadas (`04:39`) | 006 SC-003 | 006 T021, T018 | En proceso (caché en memoria) · **no pasa por Cloudflare** | ✓ | ⚠️ |
| RNF-29 | Notificación ≤ 48 h; alerta por exportación, cambio de rol y ≥ 5 bloqueos/h (`04:40`) | 002 SC-003, FR-014 | 002 T027, T031 | Simulacro con cronómetro en «killauru-pruebas» · una vez (docs: semestral, `08:198`) | ✓ | ⚠️ (frecuencia) |
| RNF-30 | Revisión mensual de administradores (`04:41`) | 003 FR-034 | 003 T047…T050 | Funcional ✓; el cumplimiento mensual es operación | ✓ | ✅ |
| RNF-34 | Inventario de secretos, copia cifrada fuera, rotación; **simulacro de recreación de la VM** (`04:42`) | 001 FR-007, FR-028 | 001 T030, T053 (runbooks) | Revisión · **sin simulacro** | — | ⚠️ |
| RNF-31 | Acción y auditoría en la misma transacción; prueba que fuerza la falla (`04:43`) | 002 SC-001 · 003 SC-006 | 002 T009 · 003 T016 · 004 T044 | Testcontainers | ✓ | ✅ |
| RNF-12 / 13 / 14 | Español y ≤ 3 toques; Chrome/Firefox/Edge; tiempo relativo (`04:49-51`) | — | — | Revisión manual | — | 📱 ❌ |
| RNF-15 | ≤ 5 %/h de batería, < 1 MB/h de datos, < 100 MB a 6 meses (`04:57`) | — | — | Perfilado en campo | — | 📱 ❌ |
| RNF-16 | ≤ US$ 10, alerta a US$ 8 (`04:58`) | 001 SC-008 | 001 T057 (configurar) | Facturación mensual · **sin prueba de la alerta** (EQ-15 pide probarla con un umbral bajo, `08:228`) | ✓ | ⚠️ |
| RNF-17 | `docker compose up -d` en entorno limpio (`04:64`) | 001 SC-001 | 001 T029; quickstart §2 (a mano) | Manual · sin automatizar | ✓ | ⚠️ |
| RNF-18 | Rollback < 5 min, migraciones explícitas en dos fases (`04:65`) | 001 SC-002 | 001 T031, T054 | Simulacro en «killauru-pruebas» | ✓ | ✅ |
| RNF-19 | README por módulo (`04:66`) | 001 FR-017 + cada módulo | 001 T006, T055 · 004 T051 · 005 T033 · 006 T028 … | Revisión | ✓ | ✅ |
| RNF-28 | Pruebas de arquitectura **en cada build**, medido en el **pipeline de CI** (`04:67`) | 001 FR-029, SC-005 | 001 T007…T009 | `dotnet test` local · **no hay CI** (QA-01) | — | ⚠️ |
| RNF-20 | Alerta de caída ≤ 10 min; alerta de degradación (p95 > 250 ms o 5xx > 1 % por 5 min); simulacro de caída **y de lentitud** (`04:73`) | 001 SC-003, US4 | 001 T050; quickstart §5 (solo caída) | UptimeRobot + Grafana · **no hay simulacro de lentitud** | ✓ | ⚠️ |
| RNF-21 | OpenTelemetry, Serilog JSON, correlación, `/health` (`04:74`) | 001 FR-018…FR-021 | 001 T045…T048 | Integración + revisión | ✓ | ✅ |
| RNF-22 | Paneles y alertas: disco 80 %, tarea > 26 h, plan gratis 80 %, acciones de riesgo (`04:75`) | 001 FR-022 · 005 FR-020…FR-022 | 001 T049, T050 · 005 T031 | Configuración · **ninguna alerta se dispara a propósito** | ✓ | ⚠️ |
| RNF-32 | Plan gratuito, muestreo ≤ 10 %, sin alta cardinalidad; revisión mensual (`04:76`) | 001 FR-020 | 001 T047…T049 (`MeterListener`) | Prueba unitaria ✓ · revisión mensual sin tarea | ✓ | ⚠️ |

## 2. Matriz EQ → spec → tarea → cómo se mide

| EQ | Medida en docs (`08`) | Spec / tarea | Cómo se mide en las specs | Estado |
|---|---|---|---|---|
| EQ-01 | 5 h sin conexión, 7 días, ≤ 60 s (`08:17-18`) | — | — | 📱 ❌ |
| EQ-02 | 0 perdidos, 0 duplicados; corte de red y pérdida del 200; conciliación app ↔ BD (`08:32-33`) | 005 T012 (escenario 3: reenvío → `duplicados`), T027 · 004 T027 | Mitad del servidor ✓; la conciliación con la app y el corte **antes** del *commit* no se prueban | ⚠️ |
| EQ-03 | < 50 ms, ≥ 0,90, ≤ 5 %/h (`08:47`) | — | — | 📱 ❌ |
| EQ-04 | 100 % de intentos rechazados; 0 reidentificación; SSL Labs A+; ≤ 5 min; `/health` sin datos internos (`08:62-63`) | 003 T005, T028, T036 · 001 T045 · auditorías de esquema | Todo menos SSL Labs | ⚠️ |
| EQ-05 | Cambio de umbral con 0 líneas; 1 módulo; arquitectura en el pipeline; rollback < 5 min (`08:75-78`) | 001 T007…T009, T054 | Sin CI; ninguna prueba demuestra «cambiar el umbral por configuración» | ⚠️ |
| EQ-06 | < 300 ms p95; error < 1 %; ≤ 100 ms y ≤ 30 s; CPU y memoria; ráfaga «al recuperar señal juntos» (`08:88-93`) | 005 SC-001, SC-002, T035 · 004 T053 · 503 funcional en 005 T012 (escenario 12) | Sin perfil de ráfaga, sin CPU/memoria en los criterios, retraso de cola no medible | ⚠️ |
| EQ-07 | Caída ≤ 10 min; paneles; **detener una tarea y comprobar su alerta** (`08:107-108`) | 001 T045, T050 · 007 (tasa de confirmación por API) | Solo el simulacro de caída | ⚠️ |
| EQ-08 | 0 toques; ≤ 3 toques (`08:122`) | — | — | 📱 ❌ |
| EQ-09 | Matar el proceso a mitad de un lote, con carga normal; conciliación; ≤ 2 min (`08:135-138`) | 005 SC-003, T027 | «Detener el *host*» en proceso, 500 eventos, sin carga | ⚠️ |
| EQ-10 | RTO ≤ 2 h, RPO ≤ 24 h, simulacro **mensual** (`08:152-153`) | 001 SC-006, T054 | Una vez; RPO no se anota | ⚠️ |
| EQ-11 | 100 % recortado, **también por la caché de borde**; límites con la IP real; mismo `bbox` con y sin token **a través de Cloudflare** (`08:167-168`) | 006 T021, T030 · 001 T033 | En proceso ✓; por Cloudflare solo se mira `HIT`/`BYPASS` | ⚠️ |
| EQ-12 | 0 severas sin sesión, 0 a > 50 m, 0 por tope; 100 % de una subida de 5 h; ~600 ev/h (`08:182-183`) | 004 SC-003, SC-004, T054 | Prueba de integración contra el servicio, no un script por HTTP; el recorrido real es de la app | ⚠️ |
| EQ-13 | Reconstrucción completa; ≤ 48 h; simulacro semestral (`08:197-198`) | 002 SC-003, T031 | Simulacro con cronómetro, una vez | ✅ (frecuencia sin plan) |
| EQ-14 | p95 < 250 ms (API) y < 300 ms (Lima); error < 1 %; aciertos ≥ 80 %; **300 consultando y 300 sincronizando** (`08:212-213`) | 006 SC-001, T031 | 300 VU solo de mapa; sin Lima; sin tasa de error | ⚠️ |
| EQ-15 | 0 meses > US$ 10; alerta probada con umbral bajo (`08:227-228`) | 001 SC-008, T057 | Se configura; no se prueba | ⚠️ |

**Sin dueño en ninguna spec:** RNF-04, RNF-35, RNF-33, RNF-12/13/14, RNF-15, la parte de panel de RNF-09, EQ-01, EQ-03 y EQ-08 (todo de la app o del panel), **y además, del backend:** SSL Labs (RNF-08), el perfil combinado de RNF-03/EQ-14, la prueba sintética desde Lima (RNF-01, RS-05), la tasa de aciertos del borde, el simulacro de recreación de la VM (RNF-24/RNF-34), los simulacros de lentitud y de tarea detenida (RNF-20, EQ-07), la prueba de la alerta de presupuesto (EQ-15) y el pipeline de CI (RNF-28).

---

## 3. Pirámide de pruebas: qué hay y qué falta

| Nivel | Qué piden docs y la constitución | Qué hay en las specs | Hueco |
|---|---|---|---|
| Unitarias (dominio) | Valores límite de `enfoque:307` y `DIM:707-719`; cobertura del dominio ≥ 90 % (`principios-de-diseño.md:81`, `const:53`) | Todos los límites de `enfoque:307` están en 004 (quickstart escenarios 1–18, T006…T014) y 001 T010; los de `DIM` §10 están en 004 T010, T012, T014 y 001 T010 | Faltan límites de RN-15b en 150 km/h, de la tolerancia al futuro de 5 min y de las cuotas 120/1 000/3 000 (QA-18). La cobertura ≥ 90 % solo aparece en los *checkpoints* de 001, 003 y 004 (QA-02) |
| Application | NSubstitute + `FakeTimeProvider` | En todas las features | — |
| Integración | Testcontainers PostGIS | 001 T021 (`FabricaApi`), en todos los módulos | Imagen de pruebas (x86 en CI) distinta de la de producción (arm64, R-008): fijar la misma versión en ambas (QA-01) |
| Contrato | OpenAPI = código (`const:63`) | 001 T042 compara con **todos** los `contracts/*.openapi.yaml`; cada módulo tiene su tarea de contrato | ✅ |
| Arquitectura | NetArchTest + límite de 200 líneas | 001 T007…T009 | `DateTime.UtcNow` no se detecta bien con NetArchTest (QA-16); el límite solo cubre `.cs` (QA-17) |
| API | `WebApplicationFactory`: 200/400/401/429/503, recorte por rol (`enfoque:310`) | Cubierto en 001, 005 y 006 | — |
| E2E entre módulos | Flujo de `enfoque` §6 | Cada módulo prueba con semillas o dobles; 005 T024 llega hasta Anomalías | Nadie prueba lote → anomalía → mapa → caducidad → `updatedSince` → indicador (QA-20) |
| Carga | Perfil de RNF-03 (`04:14`), EQ-06, EQ-14, RNF-23 | 005 T035 y 006 T031 por separado | Perfil combinado, tasa de llegada, datos, entorno, criterio de error (QA-03…QA-06, QA-10) |
| Estrés y ráfaga | No lo piden de forma explícita; EQ-06 habla de una «ventana corta» y `DIM:307` de subir trabajadores «si no alcanza» | Nada | QA-09 |
| Caos | EQ-09: matar el proceso (`08:138`) | 005 T027 (parada ordenada en proceso) | QA-08 |
| Resistencia (*soak*) | No lo piden | Nada | QA-09 |
| Seguridad | EQ-04, EQ-11, EQ-12, EQ-13 | 001 T032, T033 · 003 · 006 T021 · 004 T054 · 002 T031 | SSL Labs (QA-11), rotación del seudónimo (QA-15), ataque por HTTP (QA-22) |
| Campo | EQ-01, EQ-03, EQ-08, RNF-15 | Nada (es de la app) | §7 |
| Operación | RNF-05, RNF-07, RNF-16, RNF-20, RNF-22, RNF-24, RNF-32 | Configuración y simulacros únicos | Frecuencias (QA-14), simulacros que faltan (QA-12, QA-13) |

**Perfil de carga, datos sintéticos, aprobación, entorno y CI:** el **perfil** solo está en docs (`04:14`); las specs lo repiten a medias (005 `quickstart.md:154`, 006 `quickstart.md:127`). Los **datos sintéticos** se limitan a semillas pequeñas (005 T004, 006 T003); no hay un generador para el volumen de `DIM:225-234`. El **criterio de aprobación** existe para p95 y error, pero no dice qué cuenta como error (429 y 503). El **entorno** no está definido para la carga: R-11 limita «killauru-pruebas» a simulacros y ese entorno no tiene túnel (001 `research.md:72`). **No hay CI.**

---

## 4. Estrés, ráfaga y resistencia: no existen. Propuesta

Nada en 001…007 busca el punto de quiebre ni prueba horas de operación. Propuesta: guardarlas en `backend/tests/carga/` y anotar resultados en `deploy/runbooks/simulacros.md`:

| Prueba | Perfil | Aprueba si | Qué se observa |
|---|---|---|---|
| **Ráfaga** (EQ-06, «al recuperar señal juntos») | 0 → 300 clientes en ≤ 10 s, cada uno sube 10 lotes de 50 sin pausa | 0 aceptados perdidos; los 503 llevan `Retry-After`; todo se procesa en ≤ 2 min después de la ráfaga | % de 503, cola, retraso, `/health/ready` |
| **Estrés** (punto de quiebre) | Escalera 300 → 450 → 600 → 900 → … dispositivos (pasos de 5 min) con el mapa al 100 % | Registrar el primer escalón donde p95 de lotes > 300 ms, error > 1 %, retraso de cola > 30 s o CPU > 85 %. Meta: quiebre ≥ 1,5 × S-01 (450) — **la cifra la decide el autor** | CPU, memoria, conexiones de Npgsql, bloqueos consultivos, GC |
| **Resistencia (*soak*)** | 4–8 h al perfil nominal de S-01 (~48 000 eventos/día, `DIM:225`) más la lectura del mapa, con las tareas horarias activas (purgas, caducidad) | Memoria estable (sin subir tras el *warm-up*), p95 estable, disco y logs con rotación, telemetría dentro del plan gratuito | Fugas en HybridCache (límite de 64 MB), `Channel`, pool; choque de purgas de 5 000 filas con los trabajadores |
| **Reinicio bajo carga** (EQ-09 completo) | Durante la ráfaga: `docker kill -s KILL api` y luego levantarlo | Conciliación exacta (`005 quickstart §4`), reanudación ≤ 2 min, 0 duplicados | Tiempo del barredor con miles de pendientes (005 `spec.md:137`) |

---

## 5. Cobertura, CI y qué bloquea un merge

- **Cobertura:** solo el dominio ≥ 90 % (docs y constitución). No se nombra la herramienta (coverlet), ni el informe, ni el umbral de Application. 002, 005, 006 y 007 no la ponen en sus *checkpoints*.
- **CI:** no hay `.github/`, ni workflow, ni ninguna tarea que lo cree. Docs lo exigen para RNF-28 (`04:67`, «Pipeline de CI») y EQ-05 (`08:75`, «pruebas de arquitectura en el pipeline»).
- **Qué bloquea un merge:** no está definido.
- **Propuesta (QA-01):**
  - **Cada PR, bloqueante:** `dotnet build` (con *warnings* como error) → pruebas unitarias → arquitectura y límite de líneas → integración con Testcontainers → contrato → puerta de cobertura (dominio ≥ 90 %) → construir la imagen `linux/arm64` con buildx.
  - **Cada noche o a mano, no bloqueante:** *smoke* de k6 (30 VU, 1 min) contra un `docker compose` levantado en el runner.
  - **Antes de cada versión:** el perfil RNF-03 completo y la ráfaga.

---

## 6. Hallazgos

| ID | Sev. | Archivo:línea | Qué dice docs (cita) | Qué dice la spec | Corrección propuesta | ¿Decide el autor? |
|---|---|---|---|---|---|---|
| QA-01 | **Alta** | `specs/001-plataforma-backend/tasks.md:14-35` (ninguna tarea de CI); `plan.md:19` | `04:67`: «deben verificarse con pruebas de arquitectura **en cada build**. \| Pipeline de CI (NetArchTest)»; `08:75`: «con pruebas de arquitectura en el pipeline» | No hay pipeline ni reglas de merge. Las pruebas solo corren con `dotnet test` local (`quickstart.md:15`) | Agregar en 001 la Phase 1/2 «T00x `.github/workflows/ci.yml`» con las etapas de §5, más protección de la rama `main` (*checks* obligatorios). Fijar la misma versión de PostGIS para Testcontainers (amd64) y producción (arm64, R-008) | Sí: plataforma de CI y reglas de la rama |
| QA-02 | **Alta** | `001/tasks.md:58`, `003/tasks.md:47`, `004/tasks.md:50`; sin puerta en `002/005/006/007 tasks.md` | `principios-de-diseño.md:81`: «Cobertura del dominio ≥ 90 %»; `const:53` | Solo 3 *checkpoints* la piden; no hay herramienta ni forma de medirla | Agregar `coverlet.collector` + ReportGenerator a 001 T005; puerta en CI por ensamblado `*.Domain` (o por espacio de nombres `Domain`) ≥ 90 %; repetir el *checkpoint* en 002, 005, 006 y 007 | No (lo exige docs); sí, si se quiere umbral para Application |
| QA-03 | **Alta** | `005/spec.md:203` (SC-002); `005/tasks.md:94` (T035); `006/tasks.md:78` (T031); `001/plan.md:19` («k6 … en 005 y 006») | `04:14`: «300 clientes que suben cada uno 10 lotes de 50 … **y 300 que consultan el mapa**»; `08:213`: «300 usuarios virtuales consultando y 300 sincronizando» | Dos scripts que nunca corren juntos; SC-002 no menciona los lectores y SC-001 de 005 no dice «bajo carga» | Script único `tests/carga/rnf03.k6.js` con dos escenarios en paralelo (`lotes` y `mapa`), ≥ 10 min, umbrales: `http_req_duration{endpoint:lote} p(95)<300`, `{endpoint:mapa} p(95)<250`, `http_req_failed<0.01`, más el retraso de cola. Dueño: una tarea en 001 (Polish) o una feature transversal «verificación de sistema» | Sí: qué feature es dueña |
| QA-04 | **Alta** | `001/research.md:72` (R-012); ninguna spec dice dónde corre k6 | `05:21` (R-11): «Los simulacros (restauración, rollback, incidente) se hacen en un segundo `docker compose` en la misma VM … se apaga al terminar, **para no restar recursos a producción**»; `enfoque:312`: «Los simulacros van en el entorno de pruebas (R-11)» | El entorno de pruebas no tiene `cloudflared` (no se pueden medir los 300 ms desde Lima ni el borde). Si la carga corre en la misma VM, compite con producción y `/health/ready` puede pasar a 503 (cola > 5 min), lo que descuenta de RNF-05 | Definir en 001 un «protocolo de carga»: (a) antes de la puesta en marcha, sobre el *compose* de producción sin usuarios, o (b) en «killauru-pruebas» con producción detenida y anunciada en la página de estado; k6 **fuera** de la VM (una laptop en Lima o Ayacucho para la prueba sintética); nunca contra datos reales | **Sí** (afecta R-11 en docs) |
| QA-05 | **Alta** | `006/tasks.md:78` («300 VU»); `005/quickstart.md:154`; `001/spec.md:82, 141` | `04:36`: «60 req/h de consultas para el anónimo (clave compuesta seudónimo + IP …)»; `08:91-92`: «aplica contrapresión (503 con `Retry-After`) … error < 1 %» | Sin tasa de llegada, pausas ni reparto anónimo/registrado: 300 VU en bucle desde una sola IP reciben 429 a los 60 pedidos. No se define si 429 y 503 cuentan como error. El mapa usa `bbox` alineados (T031), pero la app anónima pide sin `bbox` (006 R-007) | Escribir el perfil en una tabla: llegada por VU (p. ej. mapa anónimo ≤ 1/min, registrado con token y `updatedSince`), un seudónimo por VU, cuántos con cuenta (tokens previos; Turnstile de prueba), limitador **activo**. Criterio: error = 5xx distintos de 503 + *timeouts*; los 503 se reportan aparte (≤ x %), y la aprobación exige que todo lote se acepte tras el `Retry-After` | **Sí**: si 503 y 429 cuentan como error |
| QA-06 | **Alta** | `001/data-model.md` §3.2 (`ObservarCola`: 2 *gauges*); `005/data-model.md:8-18` (sin `procesado_utc`); `005/spec.md:94, 203` | `04:17`: «retraso de cola ≤ 30 s (p95)» | Solo hay tamaño y antigüedad del más viejo: un p95 no sale de un *gauge* | Agregar a `IMetricas` `RegistrarRetrasoCola(TimeSpan)` → histograma `killauru.ingesta.retraso_cola_ms` (`TimeProvider.GetUtcNow() − recibido_utc` al marcar la fila), o bien una columna `procesado_utc`. Tarea de prueba con `MeterListener` | Sí (cambia 001 y 005; es menor) |
| QA-07 | Media | `006/spec.md:178-179` (SC-001/002); `006/tasks.md:77` (T030) | `04:12`: «< 300 ms de extremo a extremo desde Lima … al menos el 80 % … en la caché de borde»; `05:43` (RS-05): «Medir la latencia Lima → Santiago en la prueba sintética»; `08:212`: «tasa de error < 1 %» | SC-001 solo tiene 250 ms en la API; no hay prueba sintética ni medición de la tasa de aciertos (T030 mira un `HIT`) | Agregar SC «p95 < 300 ms desde Lima» y «error < 1 %» a 006; tarea: k6 o un *check* sintético desde Lima contra `https://api.killauru.pe` y lectura de la tasa de aciertos en Cloudflare Analytics (filtro por ruta) durante 7 días | No |
| QA-08 | Media | `005/tasks.md:77` (T027); `005/quickstart.md:130` | `08:135`: «Carga normal con lotes en curso»; `08:138`: «Prueba de caos: **matar el proceso** a mitad de un lote»; `DIM:717` | «Detener el *host*» en proceso (parada ordenada que cancela los `BackgroundService`), 500 eventos, sin carga | Conservar T027 como prueba de integración y agregar T027b de caos en el entorno de carga: k6 activo + `docker kill -s KILL` durante el `INSERT` y durante el procesamiento → conciliación SQL, reanudación ≤ 2 min, 0 duplicados, y ningún lote con 200 que no esté confirmado | No |
| QA-09 | Media | Todas las specs (no hay tarea) | `08:88`: «en una ventana corta (p. ej. al recuperar señal juntos)»; `DIM:307`: «Si no alcanza en la prueba de carga, se suben los trabajadores» | Ni ráfaga, ni estrés, ni resistencia | Las cuatro pruebas de §4 | Sí: la meta de quiebre (p. ej. 1,5 × S-01) y si el *soak* entra en el alcance |
| QA-10 | Media | `005/tasks.md:19` (T004), `006/tasks.md:18` (T003): solo semillas pequeñas | `DIM:225-234`: «~1,5 millones» de filas en `evento_recibido`, «~35 millones» en `reporte_anomalia` | La carga correría con la BD vacía: los planes de `ix_recibido_tope`, `MasCercanaAsync` e `ix_anomalia_mapa` no se validan | Generador `tests/carga/datos/` (SQL `generate_series` sobre la polilínea) que siembra 30 días de recepción y 3–6 meses de reportes; ejecutar `EXPLAIN (ANALYZE)` de las consultas de `DIM` §3.5 antes de la carga | No |
| QA-11 | Media | `001/checklists/trazabilidad.md:19` (CHK006 → T034, T039) | `04:33`: «Calificación objetivo en SSL Labs: A+, con HSTS en **todos** los subdominios (`api`, `panel`, `cdn`)»; `08:63`: «escaneo SSL Labs de todos los subdominios» | T034 prueba la cabecera en proceso; nadie escanea TLS | Tarea en 001 (Polish): escanear los 3 subdominios con SSL Labs (o `testssl.sh`), guardar el resultado y repetirlo después de cada cambio de Cloudflare | No |
| QA-12 | Media | `001/quickstart.md:46-48`; `001/tasks.md:122` (T050) | `04:73`: «Simulacro de caída **y de lentitud**»; `08:108`: «detener una tarea programada y comprobar su alerta» | Solo se simula la caída | Simulacros: retraso artificial (p. ej. `pg_sleep` en un *endpoint* de prueba solo en desarrollo, o `tc netem`) ≥ 5 min → alerta p95; detener la tarea de purga 26 h con el reloj → alerta; disco al 80 % con un archivo de relleno | No |
| QA-13 | Media | `001/tasks.md:131` (T054); `001/spec.md:229` (SC-006) | `04:26`: «≤ 4 h ante pérdida total de la VM»; `04:42`: «simulacro de recreación de la VM (RNF-24)» | T054 cronometra la vuelta atrás y RTO 2 h; no anota RPO ni recrea la VM | T054b: recrear la VM desde cero (repositorio + secretos externos + último backup) con cronómetro ≤ 4 h; anotar RPO (hora del backup frente a la del último dato) | No |
| QA-14 | Media | `001/tasks.md:131, 139`; `002/tasks.md:80` (tareas únicas) | `04:25`: «prueba de restauración **mensual**»; `08:198`: «Simulacro **semestral**»; `04:76`: «Revisión **mensual**»; `08:228` | Cada verificación periódica es una tarea que se hace una vez | Agregar `deploy/runbooks/calendario-verificaciones.md`: qué, cada cuánto, quién y dónde se anota (restauración mensual, revisión de privilegios mensual, uso de Grafana y facturación mensual, simulacro de incidente semestral, disponibilidad mensual), con un recordatorio | No |
| QA-15 | Media | `001/spec.md:82, 141`; `001/tasks.md:90` (T033) | `04:36`: «clave compuesta seudónimo + IP, para tolerar redes CGNAT»; `08:167`: «límites de 60, 1 000 y 3 000 req/h aplicados con la IP real» | Un script que cambia `X-KillaUru-Seudonimo` en cada petición obtiene una cuota nueva cada vez; ninguna prueba define qué debe pasar | Decidir un techo por IP (p. ej. N × 60/h) o una regla de *rate limiting* de Cloudflare, y probarlo: 200 peticiones con 200 seudónimos desde una IP → 429 | **Sí** (docs no lo cubre) |
| QA-16 | Media | `001/tasks.md:32` (T008) | `const:34`: «Ningún código usa `DateTime.UtcNow`»; `DIM:719` | Se pide a NetArchTest, que revisa dependencias **entre tipos**; `DateTime` es un tipo de `System` que se usa con razón, así que no distingue la llamada a `UtcNow` | Usar `Microsoft.CodeAnalysis.BannedApiAnalyzers` (RS0030) con `BannedSymbols.txt` (`DateTime.Now/UtcNow`, `DateTimeOffset.Now/UtcNow`), que con *warnings* como error rompe el build; dejar NetArchTest para las capas | No |
| QA-17 | Baja | `001/tasks.md:31` (T007: `src/**/*.cs`) | `const:57`: «Ningún archivo de código fuente supera 199 líneas (C#, SQL de consultas, scripts)» | Solo revisa `.cs` | Extender a `db/**/*.sql` (excepto `db/migraciones/`), `deploy/**/*.sh` y `tests/carga/**/*.js` | No |
| QA-18 | Baja | `004/quickstart.md:22, 27`; `001/tasks.md:90` | `const:53`: «Cada regla … con **valores límite**»; `09-reglas-de-negocio.md:61`: «> 150 km/h» | Escenario 9 solo prueba 9/10 s a 160 km/h; no hay 149/150/151 km/h ni 4:59/5:00/5:01 min de tolerancia al futuro; las cuotas 120/1 000/3 000 no tienen límite | Agregar esos casos a 004 T008/T009 y a 001 T033 (121.º lote, 1 001.ª y 3 001.ª petición → 429) | No |
| QA-19 | Baja | `003/spec.md:269` (SC-005), `003/tasks.md:120` (T055), `002/spec.md:162` (SC-004) | Docs no fija ninguna latencia para login ni auditoría; `const:20`: lo que no viene de docs se marca «Decisión de diseño SDD» | Metas de 500 ms y 50 ms (renovación, solo en T055 y `plan.md:21`) sin marcar; 300 inicios de sesión chocan con Turnstile y el bloqueo de RN-19 | Marcarlas como SDD en `research.md`; subir la renovación a un SC si se mantiene; definir el perfil de T055 (cuentas sembradas, Turnstile en modo de prueba) | Sí: confirmar las metas |
| QA-20 | Media | Ninguna tarea; 005 T024 y 006 T018 se detienen en su módulo | `enfoque` §6 (flujo de la petición a la anomalía persistida); `DIM:716` | No hay prueba de punta a punta entre módulos | `tests/KillaUru.Integracion.Tests/Flujos/FlujoCompletoTests.cs`: 2 lotes de vehículos distintos con sesión → `Activa` → visible en `GET /anomalies` con `ETag` nuevo → reloj + vencimiento → baja en `updatedSince` → conteo en el indicador del día | No |
| QA-21 | Baja | `005/quickstart.md:116-133` | `08:33`: «Interrupción simulada de red **a mitad del envío**, incluida la pérdida de la respuesta 200» | Se prueba el reenvío (escenario 3), no el corte antes del *commit* | Prueba con `HttpClient` cancelado durante el `INSERT`: o no hay filas o están todas; el reenvío las da como `aceptados` o `duplicados`, nunca a medias | No |
| QA-22 | Baja | `004/tasks.md:121` (T054 en `IntAn/`) | `08:183`: «Prueba con **un script que envía eventos** fuera de la vía, en ráfaga y con saltos de posición» | Se prueba contra el servicio, sin HTTP, sin el límite de tasa ni la recepción de 005 | Repetir el escenario de ataque por HTTP (k6 o script) en el entorno de carga, con seudónimos rotados (enlaza con QA-15) | No |
| QA-23 | Baja | `001/tasks.md:120` (T048); `001/research.md:72` | `04:76`: «dentro del plan gratuito … sin tarjeta» | La carga y el *soak* generan telemetría que puede gastar la cuota gratuita o ensuciar los paneles de producción | En el entorno de carga, exportar con `deployment.environment=pruebas` a otra *stack* gratuita o desactivar OTLP; medir el uso de Grafana después de cada corrida | No |
| QA-24 | Baja | `001/spec.md:224` (SC-001) | `04:64`: «Despliegue en entorno limpio» | Solo a mano (quickstart §2) | Etapa de CI: `docker compose up -d` + `migrar.sh` + `curl /health/ready` = 200 en el runner | No |
| QA-25 | Baja | `004/spec.md:271` | RF-26 (publicar la polilínea y el mapa base en R2 para la app) | «tarea de datos compartida, fuera del backend»: no tiene dueño ni prueba (integridad SHA-256 del manifiesto, DA-18 / ADR-011) | Darle dueño (feature de la app o una tarea de datos) con una prueba de manifiesto y *hash* | Sí |

**Inconsistencias numéricas entre specs y docs (punto 5):** no se encontró ningún umbral **distinto** al de docs. Valores revisados: 250/300 ms, 300 dispositivos, 10 lotes de 50, < 1 %, ≤ 100 ms, ≤ 30 s, ≤ 2 min, 72 h, 30 días, 2 años, 0.75, 20 m, 50 m, 168 h, 3 min, 150 km/h, 120/h, 60/120/1 000/3 000, 5 min de `/health/ready`, RTO 2 h/4 h, RPO 24 h, US$ 10/8, caducidad cada hora con 1 h máximo de atraso (004 SC-007 = `09-reglas-de-negocio.md:147`). Las diferencias son **omisiones**, no cifras distintas:

- 006 SC-001 deja fuera los 300 ms desde Lima y el error < 1 % (QA-07).
- 005 SC-002 y 006 SC-001 parten el perfil de RNF-03 (QA-03).
- 005 `quickstart.md:154` omite «≥ 10 min».
- 001 T054 omite RPO y las 4 h de la VM, que SC-006 sí pide (QA-13).
- Las metas de 003 SC-005 y 002 SC-004 no vienen de docs (QA-19).

---

## 7. Lo que queda sin probar hasta que existan las specs de la app y del panel

| Docs | Qué se quedaría sin verificar |
|---|---|
| RNF-04, RNF-35, EQ-03 | Latencia del modelo en ≥ 3 celulares, matriz de confusión y conjunto de prueba separado, por `modeloVersion` |
| RNF-33, EQ-01, DA-01, DA-14 | 5 h en modo avión, 7 días adelantando el reloj del emulador, reproductor de sensores de 5 h, reinicio de la sincronización ≤ 60 s, mapa base offline |
| RNF-15 | ≤ 5 %/h de batería con pantalla apagada, < 1 MB/h de datos, < 100 MB a 6 meses, ahorro de RN-22 |
| EQ-02 (lado de la app), RF-04…RF-07 | Estados locales, reintento con *backoff* y *jitter* ±25 % respetando `Retry-After`, mismos `eventoId`, conciliación de conteos app ↔ `evento_recibido` |
| RNF-06 (de punta a punta) | «Conciliación app → tabla de recepción → BD» |
| RNF-26 / RN-21 (sin conexión) | Fin del acceso local al conectarse o a los 30 días |
| EQ-08, RNF-12, RNF-14 | 0 toques para detectar, ≤ 3 toques, español, tiempo relativo |
| RN-16, RN-18, RN-22, RF-08, RF-10 | Distancia de aviso, solo vehículo liviano con soporte, ahorro de batería, sombreado fuera del demo, aviso una vez por viaje |
| DA-18, RF-33, ADR-011, RF-26 | Actualización del modelo (manifiesto + SHA-256 en R2), descarga del mapa base y de la polilínea (QA-25) |
| RNF-09 (panel), RNF-13 | CSP y `X-Frame-Options` en Cloudflare Pages (`_headers`), escáner de cabeceras; Chrome, Firefox y Edge (últimas 2 versiones) |
| ADR-009 punto 5 (panel) | Flujo real con cookie `HttpOnly; SameSite=Strict`, CORS con credenciales desde `panel.killauru.pe`, renovación a las 8 h (E2E con navegador, p. ej. Playwright) |
| EQ-07 (panel), RF-18 | Que el panel muestre la tasa de confirmación y el enlace a la página de estado |
| EQ-11 (cliente anónimo) | Que la app anónima pida sin `bbox` y sin `updatedSince` (006 R-007), condición del 80 % de aciertos |
| EQ-12 (parte legítima) | Recorrido real de 5 h en modo avión subido de golpe, en paralelo al ataque |

**Recomendación:** al crear las specs de la app y del panel, cada una debe incluir su matriz RNF/EQ como la de §1–§2, más un plan de pruebas de campo (dispositivos, ruta Ayacucho–Vinchos, cronómetro y registro).
