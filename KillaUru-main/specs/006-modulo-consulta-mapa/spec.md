# Feature Specification: Módulo Consulta de Mapa

**Feature Branch**: `006-modulo-consulta-mapa`

**Created**: 2026-10-21

**Status**: Draft

**Input**: User description: "Módulo Consulta de Mapa de KillaUru: responder el mapa de anomalías en GeoJSON recortado por rol (anónimo solo el tramo demo, registrado el corredor completo, administrador todos los estados con filtros), con caché por celdas y alcance, ETag, descarga incremental con updatedSince que incluye las bajas, y cabeceras de caché que solo dejan cachear en el borde lo anónimo."

---

## 0. Ficha del módulo

| Campo | Valor |
|---|---|
| Qué es | La **lectura rápida** del mapa con alcance por rol: el endpoint más llamado del sistema (RNF-01) y la frontera que impide ver el corredor completo sin cuenta (RNF-27) |
| Qué **no** es | No tiene tablas ni `DbContext` ni escribe SQL: todo lo lee por `IAnomaliaLectura` (004). No decide estados ni caducidades (004). No sirve el mapa base ni la polilínea (R2, RF-26) |
| Componente C3 | Consulta de Mapa (recorte por rol · caché por celda · ETag) |
| Esquema de BD | Ninguno |
| Depende de | `Shared`; Anomalías (004) por `IAnomaliaLectura` y el evento `AnomaliaCambiada` |
| Lo usan | Nadie por contrato. La app (mapa y copia offline) y el panel (mapa con filtros) por HTTP |
| Pantallas del prototipo | App → Mapa (tramo demo sombreado para el anónimo, «Actualizado hace…»); Panel → Mapa con filtros (tipo, severidad, fechas, estado) y franja «Anomalías activas a lo largo del corredor» del Resumen |
| Prioridad | *Must*: HU-07, HU-08, HU-12 |

## 1. Fuentes en `referencia/docs`

| Tipo | IDs | Documento |
|---|---|---|
| Historias | HU-07, HU-08, HU-12 | `02-historias-de-usuario.md` |
| Requisitos funcionales | RF-14, RF-17, RF-08 y RF-26 (parte servidor: recorte, `venceUtc`, bajas) | `03-requisitos-funcionales.md` |
| Atributos de calidad | RNF-01, RNF-11 (política `consultas`), RNF-27 | `04-atributos-de-calidad.md` |
| Reglas de negocio | RN-08 | `09-reglas-de-negocio.md` |
| Decisiones | ADR-006, ADR-011 (mapa base fuera de la API) | `07-decisiones-arquitectonicas.md` |
| Escenarios | EQ-11, EQ-14 | `08-escenarios-y-atributos-de-calidad.md` |
| Diseño | `ConsultarMapaUseCase`, `AnomaliasController`, `MapaOptions`, flujo 4.2 | `enfoque-arquitectonico.md` §4.1, §5; `arquitectura-inicial.md` §4.2 |
| Diseño | Caché por celdas, `ZonaDemo`, ETag, filtros sin caché, `updatedSince`, *Decorator* y *Observer* | `diseño-interno-de-modulos.md` §4, §6.5; `patrones-de-diseño.md` §2.1 |

## User Scenarios & Testing *(mandatory)*

### User Story 1 - El anónimo ve solo el tramo demo (Priority: P1)

Como conductor no registrado, quiero ver las anomalías del tramo Ayacucho–Vinchos aunque pida un área mayor, para conocer la vía antes de registrarme, sin que nadie pueda sacar el corredor completo sin cuenta (HU-07, RN-08, RNF-27).

**Why this priority**: *Must* y frontera de seguridad del modelo de negocio (EQ-11).

**Independent Test**: Sembrar anomalías dentro, en el borde y fuera del polígono demo; pedir el `bbox` del corredor sin token y comprobar que solo vuelven las de adentro.

**Acceptance Scenarios**:

1. **Given** una petición sin token con un `bbox` que cubre los 333 km, **When** se responde, **Then** el GeoJSON trae solo anomalías `Activa` **dentro** del polígono demo (RF-14, RN-08).
2. **Given** una anomalía en una celda del borde del polígono pero fuera de él, **When** se responde al anónimo, **Then** no aparece (filtro punto por punto, `diseño-interno` §4).
3. **Given** un `bbox` que no toca el tramo demo, **When** lo pide el anónimo, **Then** recibe 200 con una colección vacía.
4. **Given** una respuesta anónima, **When** sale, **Then** lleva `Cache-Control: public, max-age=60` y `ETag` (ADR-006).
5. **Given** el mismo `bbox` pedido primero con token y luego sin token, **When** el segundo pasa por la caché, **Then** el anónimo recibe solo el tramo demo (EQ-11, `diseño-interno` §10).

---

### User Story 2 - El conductor registrado ve el corredor completo (Priority: P1)

Como conductor registrado, quiero ver los 333 km con las anomalías activas, su severidad, cuándo se detectaron y cuándo vencen, para planificar mi viaje y alertarme sin señal (HU-08, RF-14, RF-26).

**Why this priority**: *Must*; es el valor principal de registrarse.

**Independent Test**: Con token de conductor, pedir el corredor; repetir con `If-None-Match`; cambiar una anomalía y repetir.

**Acceptance Scenarios**:

1. **Given** un token de conductor, **When** pide cualquier `bbox`, **Then** recibe todas las `Activa` del corredor dentro del `bbox`, con `id`, posición, `tipo`, `severidad`, `persistencia`, `ultimaDeteccionUtc` y `venceUtc` (RF-14, RF-26).
2. **Given** una respuesta con token, **When** sale, **Then** lleva `Cache-Control: private, no-store` (ADR-006).
3. **Given** el `ETag` de una respuesta anterior, **When** repite la petición con `If-None-Match` y ninguna de sus celdas cambió, **Then** recibe 304 sin cuerpo y sin consultar la BD (`diseño-interno` §4).
4. **Given** que una anomalía de una de sus celdas cambió (nueva, confirmada, expirada, moderada), **When** repite la petición, **Then** recibe 200 con un `ETag` nuevo y los datos al día (invalidación por `AnomaliaCambiada`).
5. **Given** anomalías `PendienteConfirmacion`, `Expirada`, `Reparada` o `Descartada`, **When** pide el mapa, **Then** no aparecen (RN-08).

---

### User Story 3 - Copia offline al día con `updatedSince` (Priority: P1)

Como conductor, quiero descargar solo lo que cambió desde mi última actualización, incluidas las anomalías que dejaron de verse, para que mi copia offline no alerte de baches que ya no están (RF-14, RF-26).

**Why this priority**: *Must*; sin las bajas la copia offline acumula falsas alarmas.

**Independent Test**: Tomar una fecha, cambiar anomalías (alta, confirmación, expiración, moderación) y pedir `updatedSince` con cada rol.

**Acceptance Scenarios**:

1. **Given** `updatedSince = T`, **When** se responde, **Then** trae como *feature* normal cada anomalía modificada desde T que hoy es visible para el rol, y como `{id, eliminada: true}` (sin posición ni estado) cada una modificada desde T que hoy **no** es visible (RF-14).
2. **Given** una anomalía `Activa` fuera del tramo demo, **When** el anónimo pide `updatedSince`, **Then** no aparece ni como alta ni como baja.
3. **Given** cualquier respuesta, **When** sale, **Then** trae `generadoUtc` (hora del servidor) para usarlo como el próximo `updatedSince` (R-006).
4. **Given** `updatedSince` en el futuro o con formato inválido, **When** se pide, **Then** 400 `datos_invalidos`.

---

### User Story 4 - El administrador ve todo y filtra (Priority: P1)

Como administrador, quiero ver todas las anomalías, incluidas las no confirmadas, y filtrarlas por tipo, severidad, fechas y estado, para monitorear la vía (HU-12, RF-17).

**Why this priority**: *Must*.

**Independent Test**: Con cookie de administrador, pedir con y sin cada filtro; con token de conductor, intentar el filtro `estado`.

**Acceptance Scenarios**:

1. **Given** una sesión de administrador sin filtro de estado, **When** pide el mapa, **Then** recibe `Activa` y `PendienteConfirmacion` del `bbox`, cada una con su `estado` (R-004).
2. **Given** filtros `tipo`, `severidad`, `desde`, `hasta` y `estado`, **When** el administrador los usa, **Then** recibe solo las que cumplen todos, de cualquier estado pedido (RF-17).
3. **Given** un conductor o un anónimo, **When** usa `tipo`, `severidad`, `desde` o `hasta`, **Then** se aplican **sobre su alcance** (solo `Activa`; el anónimo, solo dentro del demo) (RF-14).
4. **Given** un conductor o un anónimo, **When** usa `estado`, **Then** recibe 403 `sin_permiso` (RF-14: `estado` solo para el administrador).
5. **Given** una petición con filtros, **When** se responde, **Then** no usa la caché por celdas ni devuelve `ETag` (`diseño-interno` §4).

### Edge Cases

- `bbox` invertido, fuera de rango o mal escrito: 400 `datos_invalidos`. Sin `bbox`: el corredor completo (recortado por rol) (R-003).
- `updatedSince` junto con filtros: 400 `parametros_incompatibles` (R-006).
- Anomalía que cambia de celda: imposible; la posición es fija (RN-05), así que `AnomaliaCambiada` lleva una sola celda.
- Dos procesos de la API tras un reinicio: el `ETag` incluye el instante de arranque, así que nunca hay un 304 falso (`diseño-interno` §4).
- 300 peticiones simultáneas a una celda vacía en caché: una sola consulta a Anomalías (protección contra estampida de HybridCache).
- Corredor aún no cargado: `CeldasDelCorredor()` vacío → colección vacía; `/health/ready` ya está en 503 por Anomalías.
- Token presente pero vencido: 401 (middleware de 001); la app renueva o vuelve a pedir sin token.

## Requirements *(mandatory)*

### Functional Requirements

**A. Endpoint y recorte por rol (RF-14, RN-08, RNF-27)**

- **FR-001**: El sistema MUST exponer `GET /api/v1/anomalies` en GeoJSON (`application/geo+json`) para los tres roles, con la política de límite `consultas` (RNF-11).
- **FR-002**: El sistema MUST decidir **en el servidor** qué ve cada rol: anónimo → `Activa` dentro del polígono demo; conductor → `Activa` en el corredor; administrador → los estados pedidos (por defecto `Activa` y `PendienteConfirmacion`). *(RN-08; R-004)*
- **FR-003**: El sistema MUST ajustar el `bbox` a las celdas del corredor que toca (`IAnomaliaLectura.CeldasDelCorredor()`), y para el anónimo además a las celdas dentro o en el borde del polígono demo. *(`diseño-interno` §4; R-001, R-002)*
- **FR-004**: Para el anónimo, las anomalías de las celdas del borde MUST filtrarse punto por punto contra el polígono **antes** de guardarse en la caché con alcance `Anonimo`. *(`diseño-interno` §4)*
- **FR-005**: Cada *feature* MUST llevar `id`, posición `[longitud, latitud]`, `tipo`, `severidad` (mostrada), `persistencia`, `ultimaDeteccionUtc` y `venceUtc`; para el administrador, además `estado`. *(RF-14, RF-26)*

**B. Caché, ETag y cabeceras (ADR-006, RNF-01)**

- **FR-006**: Las peticiones sin filtros de anónimos y conductores MUST resolverse por celdas con una caché en memoria (HybridCache) de clave `celda:{id}:{alcance}`, valor ya serializado y vigencia de `VigenciaCacheMinutos`. *(`diseño-interno` §4, §6.5)*
- **FR-007**: Al recibir `AnomaliaCambiada(anomaliaId, celda)`, el módulo MUST incrementar la versión de esa celda y borrar sus entradas de ambos alcances. *(ADR-006)*
- **FR-008**: El `ETag` MUST calcularse en memoria con (instante de arranque, alcance, versiones de las celdas pedidas); si coincide con `If-None-Match`, responder 304 sin tocar la BD. *(`diseño-interno` §4)*
- **FR-009**: Las respuestas a anónimos MUST llevar `Cache-Control: public, max-age=MaxAgeBordeSegundos`; las respuestas con token, `private, no-store`. *(ADR-006, 001 FR-013)*
- **FR-010**: Las peticiones del administrador y las que traen filtros MUST ir a `IAnomaliaLectura.FiltrarAsync`, con el mismo recorte por rol y sin `ETag`. *(`diseño-interno` §4)*

**C. Filtros (RF-14, RF-17)**

- **FR-011**: El sistema MUST aceptar `tipo`, `severidad` (lista de 1–5), `desde` y `hasta` (sobre la última detección) para todos los roles, y `estado` (lista) solo para el administrador; `estado` de otro rol → 403. *(RF-14)*

**D. `updatedSince` (RF-14, RF-26)**

- **FR-012**: Con `updatedSince`, el sistema MUST devolver las altas visibles para el rol y las bajas como `{id, eliminada: true}` sin revelar su estado, usando `IAnomaliaLectura.CambiosDesdeAsync`, sin caché por celdas. *(RF-14)*
- **FR-013**: Toda respuesta MUST incluir `generadoUtc` (hora del servidor al armarla). *(R-006)*

**E. Operación**

- **FR-014**: El módulo MUST NOT tener esquema, `DbContext` ni SQL; solo lee por `IAnomaliaLectura`. *(regla 3; `componentes-arquitectonicos.md` §6)*
- **FR-015**: La caché MUST limitarse a `TamanoCacheMb`. *(`diseño-interno` §4)*

### Reglas y parámetros (`MapaOptions`, sección `Mapa`)

| Regla | Parámetro | Valor | Fuente |
|---|---|---|---|
| RN-08 | `PoligonoDemo` | polígono Ayacucho–Vinchos (~65 km, km 268–333) | `10-glosario.md`; coordenadas: tarea T002 |
| ADR-006 | `VigenciaCacheMinutos` | 10 | `diseño-interno` §4 |
| ADR-006 | `MaxAgeBordeSegundos` | 60 | `diseño-interno` §4 |
| ADR-006 | `TamanoCacheMb` | 64 | `diseño-interno` §4 |
| RN-08 | `EstadosAdministradorPorDefecto` | `Activa`, `PendienteConfirmacion` | SDD (R-004) |

El tamaño de celda **no** es configurable aquí: es la constante de `Celda` en Shared, la misma que usa Anomalías para `anomalia.celda` (R-002).

### Key Entities

- **Alcance**: `Anonimo` o `Registrado` (claves de caché); el administrador no usa la caché.
- **Zona demo**: polígono y clasificación precalculada de las celdas del corredor (`Dentro`, `Borde`, `Fuera`).
- **Celda GeoJSON**: *features* de una celda y un alcance, ya serializadas (`byte[]`).
- **Versiones de celda**: contador en memoria por celda, base del `ETag`.
- **Consulta de mapa**: rol, `bbox`, filtros, `updatedSince`, `If-None-Match`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: p95 < 250 ms medido en la API con 300 clientes consultando el mapa (RNF-01, EQ-14).
- **SC-002**: ≥ 80 % de las consultas anónimas del tramo demo resueltas en la caché de borde (RNF-01).
- **SC-003**: 100 % de las respuestas a anónimos recortadas al tramo demo, también a través de la caché y con `bbox` del corredor completo (RNF-27, EQ-11).
- **SC-004**: Un cambio en una anomalía se ve en la siguiente petición sin filtros (0 respuestas viejas tras `AnomaliaCambiada`).
- **SC-005**: Un 304 se responde sin ninguna consulta a la BD.

## Fuera de alcance

- Mapa base PMTiles, polilínea y manifiestos (R2, ADR-011, RF-26 parte app).
- Sombreado fuera del demo, aviso una vez por viaje y alertas (app: RF-08, RF-10, RN-16).
- Estados, caducidad y `venceUtc`: los calcula Anomalías (004).

## Assumptions

- La app anónima pide el demo completo (sin `bbox` ni `updatedSince`) para que las URL se repitan y Cloudflare acierte; el conductor registrado usa `updatedSince` (R-007).
- La regla de caché de Cloudflare excluye las peticiones con `Authorization` o con la cookie `ku_acceso` (ADR-006); se documenta en el *runbook* de monitoreo (T030).
