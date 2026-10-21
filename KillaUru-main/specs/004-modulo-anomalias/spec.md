# Feature Specification: Módulo Anomalías

**Feature Branch**: `004-modulo-anomalias`

**Created**: 2026-10-20

**Status**: Draft

**Input**: User description: "Módulo Anomalías de KillaUru: validar cada evento, agruparlo en una anomalía, confirmar las graves con dos vehículos, clasificarla como persistente o episódica, manejar su ciclo de vida y caducidad, purgar seudónimos, permitir al administrador moderar y exportar, y ofrecer lecturas al mapa y al dashboard."

---

## 0. Ficha del módulo

| Campo | Valor |
|---|---|
| Qué es | El **corazón del negocio**: dueño del agregado `Anomalia` y de todas las reglas de `09-reglas-de-negocio.md` que se aplican en el servidor (RN-05…RN-07, RN-09…RN-15) |
| Qué **no** es | No recibe lotes por HTTP (eso es Ingesta, 005); no arma el GeoJSON del mapa ni la caché (006); no calcula indicadores diarios (007, aunque le da los conteos) |
| Componente C3 | Anomalías (validar → confirmar → clasificar; ciclo de vida; exportación) |
| Esquema de BD | `anomalias`: `anomalia`, `reporte_anomalia`, `transicion`, `corredor_via` |
| Depende de | `Shared`; Auditoría (002) por `IAuditoriaService` (moderación y exportación) |
| Lo usan | Ingesta (005) por `IAnomaliasService`; Consulta de Mapa (006) y Dashboard (007) por `IAnomaliaLectura`; Mapa se suscribe a `AnomaliaCambiada` |
| Pantallas del prototipo | Panel → Mapa con detalle de la anomalía (días distintos, reportes, vence, visible, reportes por día de 15 días, historial), Moderación («Por revisar» y «Esperan confirmación»), Exportación, Resumen («Últimos cambios de estado») |
| Prioridad | *Must*: HU-05, HU-06, HU-21 · *Should*: HU-19 · *Could*: HU-20 (lo primero que se recorta) |

## 1. Fuentes en `referencia/docs`

| Tipo | IDs | Documento |
|---|---|---|
| Historias | HU-05, HU-06, HU-19, HU-20, HU-21 | `02-historias-de-usuario.md` |
| Requisitos funcionales | RF-12, RF-13, RF-20, RF-23, RF-24, RF-27 (purga), RF-18 (datos para indicadores) | `03-requisitos-funcionales.md` |
| Atributos de calidad | RNF-06, RNF-10, RNF-23, RNF-25 | `04-atributos-de-calidad.md` |
| Reglas de negocio | RN-01, RN-02, RN-05, RN-06, RN-07, RN-09, RN-10, RN-11, RN-12, RN-13, RN-14, RN-15 | `09-reglas-de-negocio.md` |
| Decisiones | ADR-005 (puntos 4 y 5), ADR-006 (publicar tras el commit), ADR-007, ADR-008, ADR-012, ADR-013, ADR-015 | `07-decisiones-arquitectonicas.md` |
| Escenarios | EQ-02, EQ-09, EQ-12 | `08-escenarios-y-atributos-de-calidad.md` |
| Diseño | Algoritmo de `ProcesarReporteUseCase`, `AnomaliasOptions`, `ReglasCalidad`, `ReglaPersistencia` | `enfoque-arquitectonico.md` §5, §6 |
| Diseño | Tablas, índices, caducidad masiva, agregado, `PoliticaConfirmacion`, caso de uso, indicadores | `diseño-interno-de-modulos.md` §3.3, §3.5, §3.8, §5, §6.2, §6.3, §10 |

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Solo los eventos de calidad cuentan (Priority: P1)

Como sistema, necesito evaluar cada evento con las reglas de calidad y antifraude, para que solo los datos confiables lleguen al mapa (HU-05, RN-11, RN-13, RN-15).

**Why this priority**: Es la primera barrera; Ingesta la necesita para responder el lote.

**Independent Test**: Evaluar eventos en los valores límite de cada regla y comprobar el motivo de descarte exacto.

**Acceptance Scenarios**:

1. **Given** un evento, **When** se evalúan las reglas de **recepción** (sin BD), **Then** se rechaza con su motivo si: falta un campo o tiene formato inválido (`EsquemaInvalido`), confidence < 0,75 (`ConfidenceBajo`), precisión GPS > 20 m (`PrecisionGpsInsuficiente`), detectado hace más de 168 h o más de 5 min en el futuro respecto de la **hora de recepción** (`TimestampFueraDeRango`), tipo fuera de RN-01 (`TipoDesconocido`), o a más de 50 m del eje de la PE-28A (`FueraDelCorredor`) (RN-11, RN-13).
2. **Given** un evento ya aceptado, **When** se procesa, **Then** se vuelven a aplicar las reglas de recepción (por seguridad) y se descarta como `VelocidadImposible` si, respecto del evento anterior del mismo seudónimo separado ≥ 10 s, la velocidad descontando las dos precisiones GPS supera 150 km/h; se descarta **solo el posterior** (RN-15b).
3. **Given** un descarte en el procesamiento, **When** ocurre, **Then** se cuenta como métrica por motivo y **no** se audita (RF-15).

---

### User Story 2 - Agrupar, confirmar y publicar anomalías (Priority: P1)

Como conductor, quiero que los reportes de un mismo bache formen **una** anomalía y que las graves se publiquen solo cuando dos vehículos distintos las confirmen, para confiar en el mapa (HU-05, RN-05, RN-06, RN-14, RN-15a).

**Why this priority**: Es la regla de negocio central del producto.

**Independent Test**: Procesar secuencias de reportes (mismo colectivo, otro auto a 2:59 y a 3:00 min, con y sin sesión, dispositivo sobre el tope) y comprobar el estado resultante.

**Acceptance Scenarios**:

1. **Given** un reporte del mismo tipo a ≤ 100 m de la **posición fija** de una anomalía (la del reporte que la creó), **When** se procesa, **Then** se suma a esa anomalía; si hay dos a ≤ 100 m, a la más cercana; si no hay ninguna, nace una nueva en la posición del reporte (RN-05).
2. **Given** una anomalía nueva con severidades 1–3, **When** se procesa, **Then** queda `Activa` de inmediato; salvo que el dispositivo superara su tope por hora de detección (RN-15a), en cuyo caso nace `PendienteConfirmacion` (RN-06).
3. **Given** un reporte de severidad 4 o 5 en una anomalía todavía no publicada, **When** se procesa, **Then** queda `PendienteConfirmacion` hasta que ≥ 2 dispositivos distintos la detecten (con cualquier severidad) dentro de 72 h, con detecciones separadas ≥ 3 min y al menos un reporte con sesión (RN-06, RN-14).
4. **Given** 4 celulares de un mismo colectivo que reportan severidad 4 en el mismo segundo, **When** se procesan, **Then** cuentan como **un** vehículo y la anomalía sigue pendiente; **When** otro auto con sesión la detecta 3:00 min después, **Then** pasa a `Activa`; a 2:59 min, sigue pendiente (RN-06).
5. **Given** una anomalía creada por un dispositivo sobre el tope y sin gravedad, **When** otro vehículo la confirma, **Then** pasa a `Activa` **sin** exigir sesión (RN-15a).
6. **Given** una anomalía `Activa`, **When** llega un reporte de severidad 5, **Then** no vuelve a pendiente; solo cambia la mediana (RN-06).
7. **Given** una anomalía con reportes de los últimos 15 días, **When** se registra un reporte, **Then** su severidad mostrada es la **mediana redondeada hacia arriba** (2, 3, 3, 5 → 3; 3, 4 → 4) (RN-05).
8. **Given** el mismo evento procesado dos veces (reintento tras una caída), **When** se procesa la segunda vez, **Then** no se duplica nada y devuelve el mismo resultado (idempotencia por `eventoId`, ADR-005).
9. **Given** dos reportes simultáneos del mismo bache, **When** se procesan en paralelo, **Then** se crea **una sola** anomalía (bloqueo por celda, ADR-005).
10. **Given** un cambio confirmado en una anomalía, **When** termina el commit, **Then** se publica `AnomaliaCambiada` para que el mapa invalide su celda; nunca antes del commit (ADR-006).

---

### User Story 3 - Clasificar y dejar que el mapa refleje la realidad en el tiempo (Priority: P1)

Como administrador, quiero que el sistema distinga baches permanentes de obstáculos temporales y retire del mapa lo que ya no existe, para priorizar las alertas (HU-06, RN-07, RN-12).

**Why this priority**: *Must*; sin caducidad el mapa se llena de falsas alarmas (ADR-008).

**Independent Test**: Con `FakeTimeProvider`, crear anomalías y avanzar el reloj: comprobar persistencia, caducidad y «vuelve a nacer».

**Acceptance Scenarios**:

1. **Given** reportes en ≥ 3 **días locales** distintos (UTC−5) dentro de 15 días, **When** se registra un reporte, **Then** la anomalía es `Persistente`; si no, `Episodica`. Un reporte a las 22:00 de Ayacucho cuenta para ese día, no para el siguiente en UTC (RN-12).
2. **Given** una `Activa` episódica sin reportes durante 72 h, una persistente sin reportes durante 7 días o una `PendienteConfirmacion` sin segundo dispositivo durante 72 h, **When** corre la tarea horaria, **Then** pasa a `Expirada` (como máximo 1 h después de su plazo) y se publica `AnomaliaCambiada` (RN-07, RF-23).
3. **Given** una `Expirada` o `Reparada`, **When** llega un reporte **detectado después** de su vencimiento, **Then** vuelve a nacer y pasa otra vez por la confirmación usando solo los reportes de las últimas 72 h; con un 5 queda `PendienteConfirmacion`, no `Activa` (RN-06, RN-07).
4. **Given** una `Expirada`, **When** llega un reporte detectado **antes** de su vencimiento, **Then** no la reactiva (RN-07).
5. **Given** cualquier ventana de tiempo de estas reglas, **When** se evalúa, **Then** usa la **hora de detección**, nunca la de llegada (RN-05).

---

### User Story 4 - Proteger la privacidad y respetar la retención (Priority: P1)

Como conductor, quiero que mi seudónimo se borre a las 72 h y que nada vincule mi identidad con lo que detecto, para proteger mi privacidad (HU-21, RN-09, RN-10, RNF-25).

**Why this priority**: *Must* y obligación legal (R-06).

**Independent Test**: Sembrar reportes con distintas horas de recepción, correr la purga y comprobar que ningún seudónimo con más de 72 h queda en `reporte_anomalia`.

**Acceptance Scenarios**:

1. **Given** reportes recibidos hace más de 72 h, **When** corre la purga horaria, **Then** su seudónimo queda vacío; los recibidos hace menos, lo conservan (aunque se hayan detectado hace días) (RN-09).
2. **Given** reportes con más de 2 años desde su detección y anomalías que llevan 2 años en estado final, **When** corre la purga diaria, **Then** se borran (RN-10).
3. **Given** el esquema del módulo, **When** se audita, **Then** ninguna tabla guarda una cuenta, correo ni token: solo `con_sesion` sí/no (RNF-10, RN-14).

---

### User Story 5 - Moderar anomalías (Priority: P2)

Como administrador, quiero marcar una anomalía como reparada, descartarla como falso positivo, reabrirla o mantenerla descartada, y ver las que necesitan una decisión, para que el mapa refleje la realidad (HU-19, RF-24, RN-07).

**Why this priority**: *Should* (se recorta después de HU-13).

**Independent Test**: Probar cada acción desde cada estado según la tabla de RN-07; comprobar auditoría, transición, invalidación del mapa y la lista «Por revisar».

**Acceptance Scenarios**:

1. **Given** una anomalía, **When** la modero, **Then** solo se permiten: marcar reparada desde `Activa` o `PendienteConfirmacion`; descartar desde `Activa`, `PendienteConfirmacion` o `Expirada`; reabrir desde `Reparada` o `Descartada` (pasa a `Activa` sin confirmación); mantener descartada desde `Descartada`. Otra combinación → 409 `transicion_no_permitida` (RN-07, RF-24).
2. **Given** una moderación válida, **When** se confirma, **Then** deja de mostrarse a los conductores (o vuelve, si se reabrió), queda auditada en la misma transacción y el mapa se invalida (HU-19).
3. **Given** una `Descartada`, **When** siguen llegando reportes, **Then** los absorbe para siempre; si llegan de ≥ 3 vehículos distintos en 7 días (contados como en RN-06), aparece en «Por revisar» (RN-07).
4. **Given** una anomalía en «Por revisar», **When** el administrador la reabre o la mantiene descartada, **Then** el contador de vehículos se reinicia (RN-07).
5. **Given** el panel, **When** pido las listas, **Then** obtengo «Por revisar» (vehículos en 7 días, reportes, último reporte) y «Esperan confirmación» (reportes, primer reporte, vence) (prototipo).
6. **Given** el panel, **When** abro una anomalía, **Then** veo tipo, severidad mostrada, estado, persistencia, km sobre la vía, días distintos en 15 días, número de reportes, último reporte, vencimiento, si es visible para conductores, reportes por día de los últimos 15 días y su historial de estados (prototipo).

---

### User Story 6 - Exportar el dataset sin seudónimos (Priority: P3)

Como administrador, quiero exportar las anomalías en CSV y GeoJSON con filtros, para analizarlas fuera de la plataforma (HU-20, RF-20).

**Why this priority**: *Could*: es lo primero que se recorta.

**Independent Test**: Exportar con filtros en ambos formatos; comprobar columnas, ausencia de seudónimos y el registro de auditoría.

**Acceptance Scenarios**:

1. **Given** filtros de fechas, tipo y estado, **When** exporto en CSV o GeoJSON, **Then** recibo un archivo con las anomalías que cumplen los filtros y **ninguna** columna o propiedad con seudónimos (RF-20).
2. **Given** una exportación, **When** termina, **Then** queda auditada con formato, filtros y número de filas (`DatasetExportado`, acción de riesgo, RNF-29).

---

### User Story 7 - Dar lecturas al mapa y al dashboard (Priority: P1)

Como módulos Consulta de Mapa y Dashboard, queremos leer anomalías y conteos por contrato, sin tocar las tablas de Anomalías, para respetar la regla «un dueño por tabla» (regla 3).

**Why this priority**: 006 (*Must*) no puede funcionar sin estas lecturas.

**Independent Test**: Con datos sembrados, cada operación de `IAnomaliaLectura` devuelve exactamente lo esperado.

**Acceptance Scenarios**:

1. **Given** una lista de celdas, **When** se piden las anomalías activas, **Then** se devuelven solo las `Activa` de esas celdas (`ix_anomalia_mapa`).
2. **Given** un rectángulo y filtros, **When** se filtra, **Then** se devuelven las que cumplen todos los filtros, de cualquier estado pedido.
3. **Given** una fecha, **When** se piden los cambios desde ella, **Then** se devuelven todas las anomalías modificadas desde entonces, con su estado actual (Mapa decide si son altas o bajas para cada rol).
4. **Given** un día local, **When** se pide su resumen, **Then** se devuelven: eventos (reportes) recibidos, dispositivos distintos, tramos de 1 km con reportes en 7 días y el total, confirmaciones, pendientes que expiraron, expiraciones y renacimientos desde `Expirada` (RF-18).
5. **Given** el corredor cargado, **When** se piden sus celdas, **Then** se devuelven todas las celdas que toca la franja de 50 m alrededor de la polilínea, y ninguna anomalía tiene una celda fuera de ese conjunto (006).

### Edge Cases

- Un evento en el borde de dos celdas: se bloquean todas las celdas que toca el radio de 100 m (hasta 4), en orden ascendente; nunca hay interbloqueo.
- Dos anomalías del mismo tipo a ≤ 100 m entre sí (pudo pasar antes de un cambio de radio): el reporte va a la más cercana.
- Tipos distintos en el mismo punto (bache y hundimiento): son anomalías distintas (RN-05 exige mismo tipo).
- Seudónimo ya purgado en reportes viejos: esos reportes no cuentan como «dispositivo distinto» para confirmar ni para «Por revisar» (solo cuentan los que conservan seudónimo; la ventana de 72 h coincide con la purga).
- Reporte con hora de detección **anterior** a la última detección de la anomalía (llegó tarde): se guarda, cuenta para la mediana y los días, pero `ultima_deteccion_utc` no retrocede.
- Reloj del servidor adelantado o atrasado: no afecta las reglas, porque todas usan la hora de detección y la hora de recepción que entrega Ingesta.
- Se reabre una anomalía vieja: vence contando desde el momento de la reapertura (ver `research.md` R-007).
- El corredor no está cargado al arrancar: el módulo no está listo y `/health/ready` responde 503.

## Requirements *(mandatory)*

### Functional Requirements

**A. Calidad del dato (RN-11, RN-13, RN-15)**

- **FR-001**: El módulo MUST ofrecer una evaluación **pura y sin BD** de las reglas de recepción por evento, que devuelva el motivo de descarte o nada. *(RN-11, ADR-005 punto 1)*
- **FR-002**: La antigüedad MUST calcularse como hora de **recepción** − hora de detección, tanto al recibir como al revalidar en el procesamiento. *(RN-11)*
- **FR-003**: La distancia a la vía MUST medirse contra la polilínea de la PE-28A cargada en memoria desde `anomalias.corredor_via`. *(RN-13, ADR-015)*
- **FR-004**: Al procesar, MUST descartarse por `VelocidadImposible` el evento posterior cuya velocidad respecto del anterior del mismo seudónimo (separados ≥ 10 s, descontando la suma de precisiones) supere 150 km/h. *(RN-15b)*
- **FR-005**: Los descartes MUST contarse como métrica por motivo y NO auditarse. *(RF-15, ADR-005)*

**B. Procesamiento (RN-05, RN-06, RN-14, RN-15a; ADR-005)**

- **FR-006**: El procesamiento MUST ser idempotente por `eventoId`: un evento ya procesado devuelve el resultado anterior sin efectos. *(ADR-005 punto 5)*
- **FR-007**: El procesamiento MUST tomar un bloqueo por cada celda que toca el radio de agrupación alrededor del evento, en orden ascendente, dentro de **su propia** transacción. *(ADR-005 punto 4)*
- **FR-008**: Un reporte MUST asignarse a la anomalía del mismo tipo más cercana a ≤ 100 m de su posición fija, o crear una nueva. *(RN-05)*
- **FR-009**: La severidad mostrada MUST ser la mediana de los reportes de los últimos 15 días, redondeada hacia arriba, recalculada con cada reporte. *(RN-05)*
- **FR-010**: Una anomalía no publicada MUST exigir confirmación si algún reporte de las últimas 72 h es de severidad 4–5, si su severidad mostrada es 4–5, o si la creó un dispositivo sobre el tope. *(RN-06, RN-15a)*
- **FR-011**: La confirmación MUST requerir ≥ 2 vehículos distintos (seudónimos distintos con detecciones separadas ≥ 3 min) con reportes de cualquier severidad en 72 h; si es por gravedad, además ≥ 1 reporte con sesión. *(RN-06, RN-14)*
- **FR-012**: Una anomalía `Activa` MUST NOT volver a `PendienteConfirmacion` por un reporte grave. *(RN-06)*
- **FR-013**: El tope por hora de detección MUST considerarse superado cuando el conteo de eventos recibidos de ese seudónimo en ese bloque de reloj (que entrega Ingesta) supera 120; solo afecta a las anomalías **nuevas** que cree ese evento. *(RN-15a)*
- **FR-014**: `AnomaliaCambiada(anomaliaId, celda)` MUST publicarse solo después del commit. *(ADR-006)*
- **FR-015**: Cada cambio de estado MUST registrarse como **una** transición (desde, hacia, momento y, si fue el administrador, su id). *(RF-18; `diseño-interno` §3.3)*

**C. Clasificación y ciclo de vida (RN-07, RN-12; ADR-008)**

- **FR-016**: La persistencia MUST calcularse con días locales UTC−5 distintos (≥ 3 en 15 días). *(RN-12)*
- **FR-017**: El dominio MUST calcular el vencimiento (`vence_utc`) con cada reporte: episódica +72 h, persistente +7 días, pendiente +72 h desde la última detección. *(RN-07, RF-23)*
- **FR-018**: Una tarea cada hora MUST pasar a `Expirada` las `Activa` y `PendienteConfirmacion` vencidas, registrar sus transiciones y publicar `AnomaliaCambiada` tras el commit. *(RN-07, RF-23)*
- **FR-019**: Una `Expirada` o `Reparada` que recibe un reporte detectado después de su vencimiento MUST volver a nacer y pasar por la confirmación con los reportes de las últimas 72 h. *(RN-06, RN-07)*
- **FR-020**: Una `Descartada` MUST absorber sus reportes sin reactivarse y marcarse «Por revisar» al sumar ≥ 3 vehículos en 7 días, guardando solo la hora de cada vehículo nuevo. *(RN-07)*

**D. Privacidad y retención (RN-09, RN-10)**

- **FR-021**: Una tarea cada hora MUST poner en `NULL` el seudónimo de los reportes recibidos hace > 72 h. *(RN-09, RF-27, RNF-25)*
- **FR-022**: Una tarea diaria MUST borrar los reportes con más de 2 años desde su detección y las anomalías con 2 años en estado final. *(RN-10)*
- **FR-023**: Ninguna tabla del módulo MUST guardar cuentas, correos o tokens; solo `con_sesion`. *(RNF-10, RN-14)*
- **FR-024**: Cada tarea programada MUST registrar su última ejecución correcta como métrica. *(RNF-22)*

**E. Moderación (RF-24, RN-07)**

- **FR-025**: El administrador MUST poder ejecutar `MarcarReparada`, `Descartar`, `Reabrir` y `MantenerDescartada` solo desde los estados permitidos; cada una se audita en la misma transacción, registra su transición (si cambia el estado) y publica `AnomaliaCambiada`. *(RF-24, RN-07, ADR-013)*
- **FR-026**: `Reabrir` y `MantenerDescartada` MUST reiniciar el contador de «Por revisar». *(RN-07)*
- **FR-027**: El administrador MUST poder listar «Por revisar» y «Esperan confirmación», ver el detalle de una anomalía y los últimos cambios de estado. *(RF-24; prototipo)*

**F. Exportación (RF-20)**

- **FR-028**: El administrador MUST poder exportar anomalías en CSV y GeoJSON filtradas por fechas, tipo y estado, sin seudónimos, con auditoría `DatasetExportado`. *(RF-20, HU-20)*

**G. Lecturas para otros módulos**

- **FR-029**: El módulo MUST ofrecer `IAnomaliaLectura` con: activas por celdas, filtrado, cambios desde una fecha, resumen de un día y las celdas del corredor. *(`componentes-arquitectonicos.md` §3.1; celdas del corredor: 006 R-001)*

### Reglas y parámetros (`AnomaliasOptions`, sección `Anomalias`)

Valores de `enfoque-arquitectonico.md` §5 (sin cambios):

| Regla | Parámetro | Valor | Regla | Parámetro | Valor |
|---|---|---|---|---|---|
| RN-11 | `ConfidenceMinimo` | 0.75 | RN-15a | `MaxEventosPorHoraDeteccion` | 120 |
| RN-11 | `PrecisionGpsMaximaMetros` | 20 | RN-15b | `VelocidadMaximaPlausibleKmh` | 150 |
| RN-11 | `AntiguedadMaximaDias` | 7 | RN-15b | `SeparacionMinimaPlausibilidadSegundos` | 10 |
| RN-11 | `ToleranciaFuturoMinutos` | 5 | RN-12 | `DiasDistintosMinimos` / `VentanaDias` / `DesfaseHorarioHoras` | 3 / 15 / −5 |
| RN-05 | `RadioAgrupacionMetros` | 100 | RN-07 | `CaducidadEpisodicaHoras` | 72 |
| RN-05 | `VentanaSeveridadDias` | 15 | RN-07 | `CaducidadPersistenteDias` | 7 |
| RN-06 | `SeveridadAltaDesde` | 4 | RN-07 | `CaducidadPendienteHoras` | 72 |
| RN-06 | `DispositivosRequeridos` | 2 | RN-07 | `VehiculosRevisionDescartada` / `VentanaRevisionDescartadaDias` | 3 / 7 |
| RN-06 | `VentanaConfirmacionHoras` | 72 | RN-09 | `PurgaSeudonimoHoras` | 72 |
| RN-06 | `SeparacionMinimaMinutos` | 3 | RN-10 | `RetencionHistoricaDias` | 730 |
| RN-14 | `ReportesConSesionRequeridos` | 1 | RN-13 | `DistanciaMaximaViaMetros` | 50 |

### Key Entities

- **Anomalía** (agregado): tipo, posición fija, celda, estado, severidad mostrada, persistencia, última detección, vencimiento, desde cuándo está en su estado, creada por tope, marcas de revisión, por revisar.
- **Reporte**: evento aceptado y asociado a una anomalía; único por `eventoId`; con seudónimo (hasta 72 h), `con_sesion`, severidad, confidence, precisión, posición, horas de detección y recepción, versión del modelo.
- **Transición**: cambio de estado de una anomalía (desde, hacia, cuándo, administrador o sistema).
- **Corredor vía**: polilínea de la PE-28A (333 km).
- **Evento reportado / contexto del reporte** (contratos): lo que entrega Ingesta para cada evento.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Procesar un evento toma ≤ 100 ms p95 con 300 dispositivos (RNF-23).
- **SC-002**: 0 anomalías duplicadas y 0 reportes duplicados en la prueba de concurrencia y en la de reinicio (EQ-02, EQ-09).
- **SC-003**: En el escenario de ataque EQ-12: 0 anomalías de severidad 4–5 confirmadas solo con seudónimos anónimos; 0 eventos aceptados a más de 50 m de la vía; 0 anomalías publicadas solo por un dispositivo sobre el tope.
- **SC-004**: En el escenario legítimo de EQ-12: 100 % de una subida acumulada de 5 h aceptada y 0 eventos reales perdidos con ~600 eventos por hora.
- **SC-005**: 0 seudónimos con más de 72 h en `reporte_anomalia` tras la purga (EQ-04).
- **SC-006**: Cobertura de pruebas del dominio ≥ 90 %, con todos los valores límite de `enfoque-arquitectonico.md` §8.
- **SC-007**: Una anomalía vencida deja de estar `Activa` como máximo 1 h después de su plazo (RN-07).

## Fuera de alcance

- Recibir lotes por HTTP, la tabla `evento_recibido` y su purga: Ingesta (005).
- Recorte por rol, zona demo, caché y GeoJSON del mapa: Consulta de Mapa (006).
- Publicar la polilínea del corredor en R2 para la app (RF-26): tarea de datos compartida, fuera del backend (aquí solo se carga en la BD).

## Assumptions

- La polilínea de la PE-28A se obtiene de OpenStreetMap (ref `PE-28A`, Pisco–Ayacucho) y se revisa a mano antes de cargarla.
- Ingesta entrega en el contexto la hora de recepción, `con_sesion` y el conteo de la hora de detección (ver `research.md` R-001).
