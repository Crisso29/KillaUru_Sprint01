# Research: Módulo Auditoría

## R-001 · Cómo unirse a la transacción del llamador (ADR-013)

- **Decision**: `FabricaContextoAuditoria` lee `IContextoTransaccion` (scoped, de `Shared`). Si tiene conexión y transacción, crea `AuditoriaDbContext` con `UseNpgsql(conexion)` y llama `Database.UseTransaction(transaccion)`; `SaveChangesAsync` inserta sin confirmar (confirma el llamador con `IContextoTransaccion.ConfirmarAsync`). Si no hay transacción unida, `RegistrarAsync` lanza `InvalidOperationException` (D15): un módulo que olvide `Unir(...)` falla en pruebas y desarrollo en vez de auditar fuera de su transacción. Solo `RegistrarLecturaAsync` abre su propio `DbContext` con su conexión y confirma él (R-010).
- **Rationale**: `componentes-arquitectonicos.md` §6 punto 5: «el `DbContext` de Auditoría se une a la transacción del módulo que actúa usando la misma conexión (`UseTransaction`), sin compartir el `DbContext`».
- **Alternatives considered**: `TransactionScope` distribuido (Npgsql lo promovería a transacción distribuida si hay dos conexiones: no); *Outbox* (ADR-013 lo descarta).

## R-002 · Inmutabilidad en la base de datos

- **Decision**: `GRANT SELECT, INSERT` a `killauru_app` sobre `auditoria.registro` (los privilegios por defecto de V0001 ya lo hacen) y además `REVOKE UPDATE, DELETE, TRUNCATE … FROM killauru_app` explícito en V0002. La identidad `GENERATED ALWAYS` impide forzar ids.
- **Rationale**: `diseño-interno` §3.4 (append-only en la BD) y RNF-31. Defensa en profundidad: aunque haya un error en el código, la BD no deja borrar.
- **Alternatives considered**: *trigger* que lance excepción ante `UPDATE` (redundante con el `REVOKE`).

## R-003 · Catálogo de acciones como `smallint`

- **Decision**: `enum AccionAuditada : short` en `Contracts/` con valores fijos 1–14 que **nunca se renumeran**; la columna `accion smallint` tiene `CHECK (accion BETWEEN 1 AND 14)` que se amplía con cada migración que agrega acciones.
- **Rationale**: «Enumeraciones como `smallint` con `CHECK`» (`diseño-interno` §3.1). El enum vive en `Contracts/` porque los otros módulos lo usan al registrar.

## R-004 · Reglas de privacidad del contexto

- **Decision**: `RegistroAuditoria.Crear` rechaza un contexto que tenga alguna clave de `ClavesProhibidas` (comparación sin distinguir mayúsculas). Resultado: `ResultadoRegistro.ContextoNoPermitido` → el caso de uso lanza una excepción de programación (es un error del módulo llamador, no un caso de negocio) que revierte la transacción.
- **Rationale**: RF-30 y ADR-013 punto 3. Es un **error de programación** (no un resultado esperado), por eso aquí sí es excepción (`principios-de-diseño.md` regla 3: excepciones para lo inesperado).
- **Nota sobre la IP**: el prototipo muestra la IP **enmascarada** en el inicio de sesión del administrador (`190.xxx.xxx.21`). Identidad (003) guarda solo la IP enmascarada en el contexto; Auditoría no la valida.

## R-005 · Actor «sistema»

- **Decision**: `ActorAuditoria.Sistema = new(null, null)` (igual en `contratos-internos.md` y `data-model.md`). `actor_id` y `actor_rol` quedan `NULL` en la tabla; solo el DTO de la consulta traduce `null` a `actorRol = "Sistema"`.
- **Rationale**: `diseño-interno` §3.4 ya permite `actor_id NULL` y `actor_rol NULL`.

## R-006 · Correo del actor en el panel (evitar un ciclo)

- **Decision**: la consulta devuelve `actorId` y `actorRol`; el panel pide los correos a Identidad (`GET /api/v1/usuarios?ids=…`, feature 003). Si la cuenta ya no existe, muestra el id (prototipo: «Las cuentas eliminadas se muestran solo con su id interno»).
- **Rationale**: Identidad → Auditoría ya existe; Auditoría → Identidad crearía un ciclo (prohibido por la regla 4).

## R-007 · Alertas de riesgo

- **Decision**: `CatalogoAcciones.EsDeRiesgo(accion)` es verdadero para `RolCambiado`, `DatasetExportado` y `CuentaBloqueada`; el caso de uso **no** emite la métrica al guardar: registra `IContextoTransaccion.AlConfirmar(() => IMetricas.RegistrarAccionRiesgo(nombre))`, que corre solo después del commit del llamador (en `RegistrarLecturaAsync`, después de confirmar su transacción propia). Auditoría es el **único** emisor; Identidad y Anomalías no la llaman (D15, SEG-18, OPS-16). Reglas en Grafana: `increase(...{accion="DatasetExportado"}[5m]) > 0`, igual para `RolCambiado`, y `increase(...{accion="CuentaBloqueada"}[1h]) >= 5`.
- **Rationale**: RNF-29; la etiqueta `accion` tiene 3 valores posibles (baja cardinalidad, RNF-32).

## R-008 · Paginación y orden

- **Decision**: orden `ocurrido_utc DESC, id DESC`; paginación por número de página (el panel muestra «N registros»); `tamano` 1–200 (convención de la API). Índices: `(ocurrido_utc)`, `(actor_id, ocurrido_utc)` y `(accion, ocurrido_utc)`.
- **Alternatives considered**: paginación por cursor (más eficiente, pero el volumen es bajo: miles al mes).

## R-009 · Eliminaciones de cuenta después de restaurar (RN-10) — hueco detectado en docs

- **Problema**: RN-10 dice «después de restaurar un backup se vuelven a ejecutar las eliminaciones de cuenta registradas en la auditoría». Pero la auditoría vive en la **misma** BD que se restaura: si la BD se perdió por completo, también se perdieron los registros de las eliminaciones posteriores al respaldo.
- **Decision (D9)**: (1) si la BD dañada todavía se puede leer, la consulta de `data-model.md` §6 lista las eliminaciones posteriores al respaldo y se repiten antes de reabrir; (2) si se perdió, las eliminaciones, desactivaciones y cambios de rol posteriores al backup se pierden: **riesgo aceptado**, acotado por el RPO de 24 h y anotado en RN-10; el runbook `restaurar.md` (001 FR-033) pide revisar a mano las solicitudes de eliminación del último día. Además, antes de reabrir se revocan todas las sesiones y se rota la clave JWT (001). Ya no se depende de logs de Grafana (su retención gratuita es menor que 30 días).

## R-010 · Lecturas auditadas con transacción propia (D15)

- **Decision**: `RegistrarLecturaAsync(acción, actor, objetivo, contexto, ct)` abre su propio `DbContext`, inserta y confirma; si la acción es de riesgo emite la métrica después de su commit. Solo para lecturas sensibles: lista y detalle de usuarios y `GET /usuarios?ids=` (Identidad, `AccesoAdministrativoDatos`), consulta de la auditoría (Auditoría, `AccesoAdministrativoDatos`) y exportación (Anomalías, `DatasetExportado`: cuenta las filas primero, audita con `filas` y luego escribe, PO-12). Mapa y Dashboard no se registran. El host no audita: lo hace cada módulo dueño.
- **Rationale**: separa las dos intenciones (ARQ-09): una escritura sensible nunca se audita fuera de su transacción; una lectura no tiene transacción propia que unir.
- **Alternatives considered**: filtro de acción en el host para todo acceso de administrador (ARQ-11; D15 lo descartó: Mapa y Dashboard no tienen datos personales).

## Confirmados por el autor el 2026-10-21

## R-011 · Ajustes de implementación (2026-10-23)

- Domain recibe el código short y los campos simples del actor; Application traduce Contracts.
  Así Domain solo depende de BCL y Shared/Domain, conforme a la tabla de enfoque §3.
- El contexto se conserva como una instantánea JSON inmutable (BCL), validada recursivamente antes de persistir;
  el contrato público sigue recibiendo IReadOnlyDictionary. Se validan también objetivo ≤ 100 y coherencia id/rol.
- ResultadoRegistro contiene Registro/Error, sin tipos anidados. Los errores internos inválidos lanzan
  InvalidOperationException en Application, sin incluir datos personales.
- ConsultaAuditoriaRepositorio e IConsultaAuditoria separan lectura de escritura (ISP);
  COUNT y página usan RepeatableRead para evitar totales incoherentes ante inserciones concurrentes.
- RegistrarLecturaAsync restringe su catálogo a accesos administrativos y exportaciones, conforme a D15.
- Los números del DTO usan serialización estricta y los filtros documentan sus catálogos y límites.
- CRISOLOGO coordina incidentes; CRISSO lo reemplaza (confirmado por el autor durante la implementación).

- R-009 (eliminaciones tras restaurar): confirmado con ajuste por **D9** — riesgo aceptado acotado por el RPO de 24 h y revisión manual; se quita la dependencia de logs de Grafana.
- R-001, R-007 y R-010 (transacción exigida, `RegistrarLecturaAsync`, métrica con `AlConfirmar` y único emisor): **D15**.
