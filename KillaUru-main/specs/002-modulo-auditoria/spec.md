# Feature Specification: Módulo Auditoría

**Feature Branch**: `002-modulo-auditoria`

**Created**: 2026-10-20

**Status**: Implementado y verificado localmente; pendientes operativos T027/T031 y revisión de consumidores T038

**Input**: User description: "Módulo Auditoría de KillaUru: registro inmutable de toda acción sensible, escrito en la misma transacción que la acción, y consulta de solo lectura para el administrador, para reconstruir un incidente con datos personales en menos de 48 h."

---

## 0. Ficha del módulo

| Campo | Valor |
|---|---|
| Qué es | El **rastro inmutable** de las acciones sensibles del sistema: quién hizo qué, sobre qué y cuándo |
| Qué **no** es | No registra descartes de ingesta (son métricas, RNF-22); no es un log técnico (eso va a Grafana) |
| Componente C3 | Auditoría (`Nivel3-diagrama-de-componentes.md`) |
| Esquema de BD | `auditoria` (dueño único) |
| Depende de | Solo `Shared` (no usa a ningún otro módulo) |
| Lo usan | Identidad y Accesos y Anomalías (por `IAuditoriaService`); cada módulo dueño audita las lecturas sensibles de sus datos, el host no audita (D15) |
| Pantalla del prototipo | Panel → **Auditoría** (tabla con fecha UTC, actor, categoría, acción y detalle desplegable; «Solo lectura»; «retención permanente») |
| Prioridad | HU-15 *Should*, **no recortable** (RNF-29, RNF-30) |

## 1. Fuentes en `referencia/docs`

| Tipo | IDs | Documento |
|---|---|---|
| Historia | HU-15 (y los «se audita» de HU-11, HU-14, HU-19, HU-20, HU-23, HU-25) | `02-historias-de-usuario.md` |
| Requisitos funcionales | RF-15 (principal); RF-30 (sin correo) | `03-requisitos-funcionales.md` |
| Atributos de calidad | RNF-29, RNF-30, RNF-31 | `04-atributos-de-calidad.md` |
| Regla de negocio | RN-10 (retención permanente; repetir eliminaciones tras restaurar) | `09-reglas-de-negocio.md` |
| Restricción | R-06 (D.S. 016-2024-JUS: notificar en 48 h) | `05-restricciones.md` |
| Decisión | ADR-013 (misma transacción), ADR-012 (esquema propio) | `07-decisiones-arquitectonicas.md` |
| Escenario | EQ-13 (incidente con datos personales) | `08-escenarios-y-atributos-de-calidad.md` |
| Diseño | Tabla `auditoria.registro`, `REVOKE`, contrato `RegistrarAsync(acción, actor, objetivo, contexto)` y `RegistrarLecturaAsync` | `diseño-interno-de-modulos.md` §3.4 · `Nivel3` §2.2 · `componentes-arquitectonicos.md` §3.1 |
| Decisiones del autor | D9 (restauración), D15 (transacción de la auditoría y lecturas auditadas), 2026-10-21 | `specs/revision/decisiones.md` |

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Ninguna acción sensible queda sin rastro (Priority: P1)

Como responsable de la protección de datos, quiero que cada acción sensible se registre en la **misma** transacción que la acción, para que nunca exista una acción sin rastro ni un rastro de una acción que no ocurrió (ADR-013, RNF-31).

**Why this priority**: Es la base legal para responder un incidente en 48 h (R-06) y la usan Identidad y Anomalías desde su primer caso de uso.

**Independent Test**: Un módulo de prueba ejecuta una acción con auditoría; luego se fuerza una falla al insertar la auditoría y se comprueba que la acción se revirtió.

**Acceptance Scenarios**:

1. **Given** un módulo que abrió su transacción, **When** registra una acción sensible y confirma, **Then** la acción y su registro existen ambos, con la misma hora de confirmación.
2. **Given** que el registro de auditoría falla, **When** el módulo intenta confirmar, **Then** se revierten la acción y el registro (RNF-31).
3. **Given** un módulo que llama `RegistrarAsync` **sin** haber unido su transacción en `IContextoTransaccion`, **When** se registra, **Then** lanza `InvalidOperationException` (error de programación; falla en pruebas y desarrollo). Un inicio de sesión fallido entra en la transacción de Identidad, que escribe `intentos_fallidos` (D15).
5. **Given** una lectura sensible (lista o detalle de usuarios, consulta de la auditoría, exportación), **When** el módulo dueño la registra con `RegistrarLecturaAsync`, **Then** Auditoría usa su propia transacción y la confirma (D15).
4. **Given** el registro de la eliminación de una cuenta, **When** se guarda, **Then** solo contiene el **identificador interno**, nunca el correo (RF-30, ADR-013 punto 3).

---

### User Story 2 - Consultar la auditoría en modo solo lectura (Priority: P1)

Como administrador, quiero consultar y filtrar el registro de auditoría, para reconstruir quién accedió a qué y cuándo (HU-15, RNF-29).

**Why this priority**: Es el criterio de aceptación de HU-15 y la herramienta del simulacro EQ-13.

**Independent Test**: Con registros sembrados, consultar como administrador con filtros por fecha, categoría y actor; comprobar el orden, la paginación y que no hay forma de editar ni borrar.

**Acceptance Scenarios**:

1. **Given** una sesión de administrador, **When** consulto la auditoría, **Then** veo los registros más recientes primero, con hora UTC, actor (id interno y rol, o «sistema»), categoría, acción, objetivo y contexto.
2. **Given** filtros de fechas, categoría, acción o actor, **When** los aplico, **Then** solo veo los registros que cumplen todos los filtros, paginados.
3. **Given** un conductor registrado o un anónimo, **When** intenta consultar la auditoría, **Then** sin token recibe 401 y con token de conductor recibe 403 `sin_permiso` (regla de `convenciones-api.md`, T-SEG20).
4. **Given** cualquier usuario, incluido el administrador, **When** intenta modificar o borrar un registro, **Then** no existe ninguna operación para hacerlo y la base de datos lo impide (sin permisos de `UPDATE`, `DELETE` ni `TRUNCATE`).
5. **Given** que un administrador consulta la auditoría, **When** la consulta termina, **Then** esa consulta también queda auditada como `AccesoAdministrativoDatos` con `RegistrarLecturaAsync` (RF-15, D15).

---

### User Story 3 - Avisar de acciones de riesgo y responder un incidente (Priority: P2)

Como responsable, quiero una alerta cuando ocurre una exportación, un cambio de rol o una ola de bloqueos, y un procedimiento escrito de incidentes, para notificar a la Autoridad y a los afectados en ≤ 48 h (RNF-29, EQ-13).

**Why this priority**: Completa RNF-29; la alerta es configuración de Grafana sobre una métrica que emite este módulo.

**Independent Test**: Registrar una exportación y 5 bloqueos en una hora en el entorno de pruebas; comprobar que llegan las alertas. Hacer el simulacro EQ-13 con cronómetro.

**Acceptance Scenarios**:

1. **Given** que se registra una exportación o un cambio de rol, **When** se confirma la transacción, **Then** Auditoría (único emisor) emite la métrica de acción de riesgo con `AlConfirmar` y Grafana alerta; si la transacción se revierte, no se emite (D15).
2. **Given** 5 o más bloqueos de cuenta en 1 h, **When** se registra el 5.º, **Then** Grafana alerta (RNF-29).
3. **Given** el simulacro EQ-13 (exportación «no autorizada» en el entorno de pruebas), **When** se sigue el procedimiento, **Then** con la auditoría se reconstruye quién, qué, cuándo y qué datos, y el borrador de notificación queda listo en ≤ 48 h.

### Edge Cases

- Actor «sistema» (tareas programadas, bloqueo automático): `ActorAuditoria.Sistema = new(null, null)`; en la tabla `actor_id` y `actor_rol` quedan `NULL` y la consulta muestra `actorRol = "Sistema"`.
- `RegistrarAsync` sin transacción unida → `InvalidOperationException`; nunca se audita «por fuera» en silencio (D15).
- Acción de un usuario que después elimina su cuenta: el registro conserva solo su id interno; la consulta lo muestra igual (el panel no puede mostrar su correo).
- Contexto con una clave `correo`, `email` o `contrasena`: se rechaza al crear el registro (falla la transacción completa, por diseño).
- Consulta con `tamano` > 200 o fechas invertidas: 400 `datos_invalidos`.
- Restauración de un respaldo: si la BD anterior aún se puede leer, las eliminaciones de cuenta registradas después de la fecha del respaldo se vuelven a aplicar (RN-10) usando esta tabla; si se perdió, esas eliminaciones (y desactivaciones y cambios de rol) se pierden como riesgo aceptado acotado por el RPO de 24 h y el runbook pide revisar a mano las solicitudes de eliminación del último día (D9).

## Requirements *(mandatory)*

### Functional Requirements

**A. Registrar**

- **FR-001**: El módulo MUST ofrecer a otros módulos las operaciones `RegistrarAsync(acción, actor, objetivo, contexto, ct)` y `RegistrarLecturaAsync(acción, actor, objetivo, contexto, ct)` en su `Contracts/`. *(Nivel3 §2.2, `componentes` §3.1; D15)*
- **FR-002**: `RegistrarAsync` MUST escribirse en la **misma transacción** del módulo que actúa, unida en `IContextoTransaccion`; si no hay transacción unida, MUST lanzar `InvalidOperationException` (error de programación, no flujo de negocio). *(ADR-013, RNF-31; D15)*
- **FR-017**: `RegistrarLecturaAsync` MUST usar una transacción propia y confirmarla, y solo se usa para lecturas sensibles con datos personales o de seguridad: lista y detalle de usuarios y `GET /usuarios?ids=` (Identidad), consulta de la auditoría (Auditoría) y exportación (`DatasetExportado`, Anomalías). Mapa y Dashboard no se registran. *(RF-15 «accesos administrativos a datos»; D15)*
- **FR-003**: Si el registro falla, MUST fallar la operación completa del llamador. *(RNF-31)*
- **FR-004**: Cada registro MUST guardar: hora UTC (del reloj inyectado), id interno del actor (o vacío si es el sistema), rol del actor, acción, objetivo y contexto. *(RF-15, HU-15)*
- **FR-005**: El catálogo de acciones MUST cubrir todo RF-15 (ver tabla «Catálogo de acciones»); una acción fuera del catálogo no se puede registrar.
- **FR-006**: El contexto MUST NOT contener correos, contraseñas ni seudónimos; el registro se rechaza si trae las claves `correo`, `email`, `contrasena`, `password` o `seudonimo`. *(RF-30, R-06)*
- **FR-007**: Los descartes de ingesta MUST NOT registrarse aquí. *(RF-15, ADR-005)*

**B. Inmutabilidad y retención**

- **FR-008**: La base de datos MUST impedir `UPDATE`, `DELETE` y `TRUNCATE` sobre los registros al usuario de la aplicación. *(RNF-31, `diseño-interno` §3.4)*
- **FR-009**: Los registros MUST conservarse de forma permanente. *(RN-10)*
- **FR-010**: Los registros MUST referenciar a las cuentas solo por identificador, sin llaves foráneas, para que eliminar una cuenta no rompa la auditoría. *(`componentes-arquitectonicos.md` §6 punto 3)*

**C. Consultar**

- **FR-011**: El módulo MUST exponer una consulta de solo lectura **solo para el administrador**, ordenada del más reciente al más antiguo, con filtros por rango de fechas, categoría, acción y actor, y paginación (máximo 200 por página); sin token responde 401, con token de otro rol 403 `sin_permiso`, y declara 429 como toda operación. *(HU-15, RF-15; T-SEG19, T-SEG20)*
- **FR-012**: Cada consulta de la auditoría MUST quedar registrada como `AccesoAdministrativoDatos` con `RegistrarLecturaAsync`. *(RF-15; D15)*

**D. Alertas e incidentes**

- **FR-013**: Al registrar una exportación, un cambio de rol o un bloqueo de cuenta, el módulo MUST emitir la métrica de acción de riesgo **solo después del commit** (con `IContextoTransaccion.AlConfirmar`, o tras confirmar su transacción propia en `RegistrarLecturaAsync`), sin datos personales en la etiqueta; Auditoría es el **único** emisor de esa métrica. *(RNF-29, RNF-32; D15)*
- **FR-014**: MUST existir una regla de alerta en Grafana: cualquier exportación, cualquier cambio de rol y ≥ 5 bloqueos en 1 h. *(RNF-29)*
- **FR-015**: MUST existir un procedimiento escrito de incidentes con plantilla de notificación, canal de la Autoridad Nacional de Protección de Datos Personales, pasos de contención y una persona de reemplazo. *(RNF-29)*
- **FR-016**: MUST existir una consulta documentada para listar las eliminaciones de cuenta posteriores a una fecha, usada después de restaurar un respaldo si la BD anterior aún se puede leer; si no, lo perdido es riesgo aceptado y se revisan a mano las solicitudes de eliminación del último día. *(RN-10, RNF-07; D9)*
- **FR-018**: Las verificaciones periódicas de este módulo (simulacro EQ-13 semestral) MUST figurar en el calendario de `simulacros.md` de 001. *(RNF-29; T-QA14)*

### Catálogo de acciones (RF-15)

| Código | Acción | Categoría (pantalla del panel) | Quién la registra | ¿De riesgo? | Fuente |
|---|---|---|---|:---:|---|
| 1 | `UsuarioCreado` | Usuarios | Identidad | — | RF-15 «alta de usuario» |
| 2 | `RolCambiado` | Usuarios | Identidad | ✅ | RF-15, RNF-29 |
| 3 | `CuentaDesactivada` | Usuarios | Identidad | — | RF-15, HU-14 |
| 4 | `CuentaActivada` | Usuarios | Identidad | — | Derivado del prototipo (acción «Activar cuenta») |
| 5 | `CuentaEliminada` | Cuenta del titular | Identidad | — | RF-15, RF-30, HU-23 |
| 6 | `InicioSesionFallido` | Sesión | Identidad | — | RF-15, HU-11 |
| 7 | `CuentaBloqueada` | Sesión | Identidad | ✅ (≥ 5/h) | RF-15, RN-19, RNF-29 |
| 8 | `InicioSesionAdministrador` | Sesión | Identidad | — | RF-15 «accesos administrativos»; prototipo |
| 9 | `RecuperacionSolicitada` | Cuenta del titular | Identidad | — | RF-15, HU-25 |
| 10 | `ContrasenaCambiada` | Cuenta del titular | Identidad | — | RF-15, HU-25 |
| 11 | `RevisionPrivilegios` | Usuarios | Identidad | — | RF-15, RF-19, RNF-30 |
| 12 | `AnomaliaModerada` | Moderación | Anomalías | — | RF-15, RF-24, HU-19 |
| 13 | `DatasetExportado` | Exportación | Anomalías (`RegistrarLecturaAsync`, con `filas`) | ✅ | RF-15, RF-20, HU-20, RNF-29 |
| 14 | `AccesoAdministrativoDatos` | Acceso a datos | Auditoría, Identidad (`RegistrarLecturaAsync`) | — | RF-15 «accesos administrativos a datos»; D15 |

### Key Entities

- **Registro de auditoría**: hecho inmutable con hora UTC, actor (id interno + rol, o sistema), acción del catálogo, objetivo (id del recurso afectado: cuenta, anomalía, exportación) y contexto (datos no personales de la acción, por ejemplo `{desde: "Activa", hacia: "Reparada"}`).
- **Actor**: id interno de la cuenta y su rol, o `Sistema`.
- **Categoría**: agrupación de acciones para el filtro del panel.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 0 acciones sensibles sin registro en la prueba que fuerza la falla de la auditoría (RNF-31).
- **SC-002**: 0 correos de titulares eliminados en la tabla de auditoría (auditoría del esquema, EQ-04).
- **SC-003**: El simulacro EQ-13 reconstruye el incidente completo y deja el borrador de notificación en ≤ 48 h (RNF-29).
- **SC-004**: La consulta filtrada de un mes responde en < 500 ms (p95) con 50 000 registros.
- **SC-005**: El 100 % de las acciones de RF-15 tienen un código en el catálogo y una prueba que lo registra.
- **SC-006**: 0 métricas de acción de riesgo emitidas por una transacción revertida, y 0 emitidas fuera de Auditoría (D15).
- **SC-007**: Cobertura del `Domain` de Auditoría ≥ 90 % en el CI (D10).

## Fuera de alcance

- Mostrar el **correo** del actor: lo resuelve el panel llamando a Identidad (feature 003, `GET /api/v1/usuarios?ids=`), porque Auditoría no puede depender de Identidad (crearía un ciclo).
- Registrar las acciones: lo hace cada módulo dueño de la acción (003 y 004).

## Assumptions

- La plataforma (001) ya ofrece `IContextoTransaccion` (con `AlConfirmar`, `ConfirmarAsync` y `RevertirAsync`), `IMetricas`, `TimeProvider` y la seguridad por rol.
- Volumen: miles de registros al mes (`diseño-interno` §3.6); no se necesita particionar.
- El canal y los datos de contacto de la Autoridad se verifican al redactar el procedimiento (D.S. 016-2024-JUS).
