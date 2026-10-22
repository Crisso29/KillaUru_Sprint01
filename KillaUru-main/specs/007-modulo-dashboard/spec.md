# Feature Specification: Módulo Dashboard

**Feature Branch**: `007-modulo-dashboard`

**Created**: 2026-10-21

**Status**: Draft

**Input**: User description: "Módulo Dashboard de KillaUru: calcular cada día, antes de la purga de seudónimos, los indicadores de negocio (eventos válidos por día, conductores registrados activos, dispositivos que aportaron datos, cobertura, tasa de confirmación y tasa de reaparición), guardar solo números sin vincular identidades y mostrarlos al administrador con comparación contra el periodo anterior y un enlace a los paneles técnicos."

---

## 0. Ficha del módulo

| Campo | Valor |
|---|---|
| Qué es | Los **indicadores de negocio** del panel (RF-18): un número por indicador y por día, calculado una vez y guardado para siempre como número |
| Qué **no** es | No calcula nada técnico (latencia, errores, descartes por motivo, CPU: eso está en Grafana, ADR-010); no lee tablas ajenas: pide los conteos a Anomalías e Identidad por contrato; nunca guarda ids, seudónimos ni correos (RN-17) |
| Componente C3 | Dashboard (indicadores de negocio) |
| Esquema de BD | `dashboard`: `indicador_diario` |
| Depende de | `Shared`; Anomalías (004) por `IAnomaliaLectura.ResumenDelDiaAsync`; Identidad (003) por `IIdentidadConsultas.ContarConductoresActivosAsync` |
| Lo usan | Nadie por contrato. El panel por HTTP |
| Pantallas del prototipo | Panel → Resumen: franja de 6 indicadores (valor, variación «vs. 7 d previos», mini serie de 7 días, ayuda; el primero, «Eventos válidos (ayer)», D13), gráfico de eventos válidos por día con el periodo anterior, selector de periodo y botón «Paneles técnicos» |
| Prioridad | *Should*: HU-13 (4.º en el orden de recorte, después de HU-20, HU-10 y RF-33) |

## 1. Fuentes en `referencia/docs`

| Tipo | IDs | Documento |
|---|---|---|
| Historias | HU-13 | `02-historias-de-usuario.md` |
| Requisitos funcionales | RF-18 | `03-requisitos-funcionales.md` |
| Atributos de calidad | RNF-10, RNF-22 (última ejecución de la tarea) | `04-atributos-de-calidad.md` |
| Reglas de negocio | RN-17, RN-09 (por qué la hora del cálculo), RN-07 (reaparición = vuelve a nacer después de expirar, D14) | `09-reglas-de-negocio.md` |
| Decisiones | ADR-007 (privacidad), ADR-010 (técnicos en Grafana) | `07-decisiones-arquitectonicas.md` |
| Diseño | Capas del módulo, `DashboardOptions` | `enfoque-arquitectonico.md` §4.1, §5 |
| Diseño | Tabla `indicador_diario`, tarea de las 00:30, de dónde sale cada indicador | `diseño-interno-de-modulos.md` §3.4, §3.8 |
| Diseño | Retención de 2 años; procesos en segundo plano por reloj (`ProgramaReloj`, T-OPS05) | `componentes-arquitectonicos.md` §4, §6 |

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Calcular y guardar los indicadores de cada día (Priority: P1)

Como sistema, necesito calcular cada madrugada los conteos del día anterior, antes de que la purga de 72 h borre los seudónimos, y guardar solo los números, para tener la historia sin vincular a nadie (RF-18, RN-17).

**Why this priority**: sin el cálculo diario no hay nada que mostrar; y «dispositivos que aportaron datos» ya no se puede calcular después de la purga.

**Independent Test**: Con `FakeTimeProvider` y dobles de los contratos, arrancar la tarea (corre al arrancar), avanzar el reloj hasta las 00:30 de Lima y comprobar las filas guardadas.

**Acceptance Scenarios**:

1. **Given** las 00:30 hora de Lima (05:30 UTC), **When** corre la tarea, **Then** pide `ResumenDelDiaAsync(ayer)` a Anomalías y `ContarConductoresActivosAsync(7)` a Identidad, y guarda los 9 números del día en una sola transacción (`diseño-interno` §3.8).
2. **Given** el día ya guardado, **When** la tarea vuelve a correr, **Then** no lo recalcula ni lo duplica.
3. **Given** que la API estuvo apagada a las 00:30, **When** arranca, **Then** calcula los días faltantes de los últimos `DiasRecuperables` (2), que aún conservan sus seudónimos (R-002).
4. **Given** un día más viejo que eso sin calcular, **When** se consulta el resumen, **Then** aparece como hueco (`null`), nunca como 0.
5. **Given** una ejecución correcta, **When** termina, **Then** registra `RegistrarTareaCorrecta("indicadores")`; si pasan 26 h sin ella, Grafana alerta (RNF-22).
6. **Given** la tabla `indicador_diario`, **When** se audita, **Then** solo tiene día, indicador y valor: ninguna cuenta, correo, seudónimo ni id (RN-17, RNF-10).

---

### User Story 2 - Ver el resumen del sistema en el panel (Priority: P1)

Como administrador, quiero ver los 6 indicadores de negocio con su tendencia y compararlos con el periodo anterior, para decidir sobre la plataforma (HU-13, RF-18).

**Why this priority**: es la historia del módulo.

**Independent Test**: Sembrar 60 días de indicadores y pedir el resumen con 7, 14 y 30 días; comprobar cada cálculo.

**Acceptance Scenarios**:

1. **Given** una sesión de administrador, **When** pido el resumen, **Then** veo eventos válidos del último día cerrado («Eventos válidos (ayer)»), conductores registrados activos, dispositivos que aportaron datos, cobertura, tasa de confirmación y tasa de reaparición, cada uno con su valor, el valor de comparación, la variación y la serie de los últimos 7 días (prototipo).
2. **Given** los conteos diarios (eventos, conductores, dispositivos, cobertura), **When** se arma el indicador, **Then** el valor es el del último día cerrado y la comparación es el promedio de los 7 días anteriores a ese día (R-003).
3. **Given** las tasas, **When** se arman, **Then** confirmación = confirmaciones ÷ (confirmaciones + pendientes que expiraron) y reaparición = renacimientos (anomalías que vuelven a nacer después de expirar, hacia `Activa` o `PendienteConfirmacion`; D14) ÷ expiraciones, sumando los 7 últimos días contra los 7 anteriores; sin denominador → `null` (`diseño-interno` §3.8).
4. **Given** cobertura, **When** se arma, **Then** es tramos de 1 km con reportes en 7 días ÷ 333 × 100 (RF-18).
5. **Given** un periodo de 7, 14 o 30 días, **When** lo elijo, **Then** recibo los eventos válidos de cada día del periodo junto al valor del mismo día del periodo anterior (gráfico del prototipo).
6. **Given** la respuesta, **When** la veo, **Then** incluye el enlace a los paneles técnicos de Grafana (HU-13) y la fecha del último día cerrado.
7. **Given** un conductor con token, **When** pide el resumen, **Then** recibe 403 `sin_permiso`; **Given** una petición sin token, **Then** recibe 401 (regla única T-SEG20).

---

### User Story 3 - Conservar la historia 2 años (Priority: P3)

Como dueño de la plataforma, quiero que los indicadores se conserven 2 años y luego se borren, para cumplir la retención declarada (`componentes-arquitectonicos.md` §6).

**Why this priority**: bajo impacto; solo evita crecer sin límite.

**Independent Test**: Sembrar filas de hace 731 días y correr la tarea.

**Acceptance Scenarios**:

1. **Given** filas con más de `RetencionDias` (730), **When** corre la tarea diaria, **Then** se borran.

### Edge Cases

- Día sin actividad (0 eventos): se guardan ceros; las tasas de ese día son `null` (sin denominador).
- Día recuperado tarde (API caída a las 00:30): «conductores activos» se cuenta en el momento del cálculo, no a las 00:30 (R-002).
- Contrato de Identidad o Anomalías que falla: no se guarda nada de ese día (transacción) y se reintenta en la siguiente ejecución o al arrancar.
- Cambio de hora: Perú no tiene horario de verano; el desfase fijo −5 h es correcto (RN-12).
- Menos de 14 días de historia (recién desplegado): las comparaciones sin datos salen `null`.

## Requirements *(mandatory)*

### Functional Requirements

**A. Cálculo diario (RF-18, RN-17)**

- **FR-001**: Una tarea MUST correr al arrancar la API y luego cada día a `HoraCalculoLocal` (00:30, UTC−5), calculando el próximo instante con `Shared/Tareas/ProgramaReloj` (001), nunca con un `PeriodicTimer` de 24 h; su métrica se llama `indicadores`. *(`diseño-interno` §3.8; T-OPS05)*
- **FR-002**: La tarea MUST calcular cada día local faltante entre `hoy − DiasRecuperables` y ayer, pidiendo `ResumenDelDiaAsync(dia)` y `ContarConductoresActivosAsync(DiasConductorActivo)`. *(RF-18; R-002)*
- **FR-003**: Los 9 valores de un día MUST guardarse juntos en una transacción, sin sobrescribir un día ya guardado. *(R-004)*
- **FR-004**: El módulo MUST guardar solo números agregados por día; nunca ids, seudónimos ni correos. *(RN-17, RNF-10)*
- **FR-013**: Los eventos del día MUST ser los **eventos válidos** (reportes procesados por Anomalías) y el panel MUST rotularlos «Eventos válidos (ayer)», porque el valor es el del último día cerrado. *(RF-18; D13, D14; R-005)*
- **FR-005**: La tarea MUST borrar los indicadores con más de `RetencionDias` y registrar su última ejecución correcta como métrica. *(`componentes` §6; RNF-22)*

**B. Resumen (RF-18, HU-13)**

- **FR-006**: El sistema MUST exponer `GET /api/v1/dashboard/indicadores?dias=` solo para el administrador, con `dias` ∈ {7, 14, 30} (por defecto 14); sin token → 401, con token de otro rol → 403. *(HU-13; T-SEG20)*
- **FR-007**: Para eventos, conductores activos, dispositivos y cobertura, el valor MUST ser el del último día cerrado y la comparación, el promedio de los 7 días anteriores; la variación en porcentaje para eventos, en diferencia absoluta para conductores y dispositivos, y en puntos para la cobertura. *(prototipo; R-003)*
- **FR-008**: Las tasas de confirmación y de reaparición MUST calcularse sumando numeradores y denominadores de los últimos 7 días (valor) y de los 7 anteriores (comparación), con variación en puntos; `null` sin denominador. *(`diseño-interno` §3.8; R-003)*
- **FR-009**: Cada indicador MUST traer su serie de los últimos 7 días (para la tasa, la tasa diaria). *(prototipo)*
- **FR-010**: La respuesta MUST traer los eventos válidos de cada día del periodo y el del día equivalente del periodo anterior. *(prototipo)*
- **FR-011**: La respuesta MUST traer el último día cerrado, la hora del último cálculo correcto y el enlace `UrlPanelesTecnicos`. *(HU-13, ADR-010)*
- **FR-012**: Un día sin calcular MUST mostrarse como `null`, nunca como 0, y los promedios lo MUST ignorar.

### Reglas y parámetros (`DashboardOptions`, sección `Dashboard`)

| Regla | Parámetro | Valor | Fuente |
|---|---|---|---|
| RN-17 | `DiasConductorActivo` | 7 | `enfoque` §5 |
| RN-17, RN-09 | `HoraCalculoLocal` | `00:30` | `diseño-interno` §3.8 |
| RN-12 | `DesfaseHorarioHoras` | −5 | mismo valor que en Anomalías |
| RN-09 | `DiasRecuperables` | 2 | SDD (R-002) |
| — | `RetencionDias` | 730 | `componentes` §6 (2 años) |
| HU-13 | `UrlPanelesTecnicos` | URL del tablero de Grafana Cloud | SDD |
| — | `DiasComparacion` | 7 | prototipo («vs. 7 d previos») |

### Key Entities

- **Indicador diario**: (día local, indicador, valor). Indicadores guardados: `EventosRecibidos`, `ConductoresActivos`, `DispositivosDistintos`, `TramosConReportes7d`, `TramosTotales`, `Confirmaciones`, `PendientesExpiradas`, `Expiraciones`, `Renacimientos`.
- **Indicador del resumen** (calculado al leer; `IndicadorResumen` y `ResumenDashboard` viven en Application, D4): valor, comparación, variación, unidad y serie de 7 días. Indicadores mostrados: eventos válidos (ayer), conductores activos, dispositivos, cobertura, confirmación, reaparición.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: La tarea deja guardado el día anterior antes de las 01:00 hora de Lima el 100 % de los días con la API en marcha.
- **SC-002**: 0 columnas con identidades en el esquema `dashboard` (RN-17).
- **SC-003**: El resumen se arma con una sola consulta a `indicador_diario` (≤ 60 días × 9 filas), sin llamar a otros módulos.
- **SC-004**: Los 6 indicadores coinciden con un cálculo a mano sobre los datos sembrados (cobertura, confirmación y reaparición incluidas).

## Fuera de alcance

- Indicadores técnicos (latencia p95, errores, descartes por motivo, CPU, memoria): Grafana (ADR-010, RNF-22).
- «Anomalías activas a lo largo del corredor» del prototipo: el panel la arma con el mapa del administrador (006).
- «Requiere atención»: el panel la arma con las listas de moderación (004), la revisión de privilegios (003) y la página de estado de UptimeRobot.
- Analítica de tramos para clientes (v1.5).

## Assumptions

- Anomalías calcula `ResumenDelDiaAsync` con el día local UTC−5 (004 R-011).
- La API corre siempre en una sola instancia (S-03): la tarea no necesita un candado distribuido.
- 001 provee `Shared/Tareas/ProgramaReloj` (próximo instante con `TimeProvider`) y la regla 401/403 en `convenciones-api.md` (T-OPS05, T-SEG20).
