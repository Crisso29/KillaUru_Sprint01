# Data Model: Módulo Auditoría

## 1. Tabla `auditoria.registro` (`V0002__auditoria.sql`)

Copiada de `diseño-interno-de-modulos.md` §3.4, con el `CHECK` del catálogo y el índice por acción.

```sql
CREATE TABLE auditoria.registro (
    id            bigint      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ocurrido_utc  timestamptz NOT NULL,
    actor_id      uuid        NULL,                          -- id interno; NULL = sistema. Nunca el correo (RF-30)
    actor_rol     smallint    NULL CHECK (actor_rol IN (1, 2)),   -- 1 ConductorRegistrado · 2 Administrador · NULL sistema
    accion        smallint    NOT NULL CHECK (accion BETWEEN 1 AND 14),   -- catálogo de RF-15
    objetivo      text        NULL,                          -- id del recurso afectado (cuenta, anomalía, exportación…)
    contexto      jsonb       NOT NULL DEFAULT '{}'          -- datos no personales de la acción
);
CREATE INDEX ix_registro_fecha  ON auditoria.registro (ocurrido_utc);
CREATE INDEX ix_registro_actor  ON auditoria.registro (actor_id, ocurrido_utc);
CREATE INDEX ix_registro_accion ON auditoria.registro (accion, ocurrido_utc);

REVOKE UPDATE, DELETE, TRUNCATE ON auditoria.registro FROM killauru_app;   -- append-only (RNF-31)
```

Sin llaves foráneas hacia `identidad` (FR-010). Retención permanente (RN-10).

## 2. Entidad `RegistroAuditoria` (Domain)

| Campo | Tipo | Regla |
|---|---|---|
| `Id` | `long` | Lo asigna la BD |
| `OcurridoUtc` | `DateTimeOffset` | Del `TimeProvider` (nunca `DateTime.UtcNow`) |
| `ActorId` | `Guid?` | `null` si es el sistema |
| `ActorRol` | `string?` | `ConductorRegistrado`, `Administrador` o `null` (sistema) |
| `Accion` | `short` | Código validado del catálogo; Application traduce AccionAuditada (R-011) |
| `Objetivo` | `string?` | Máx. 100 caracteres |
| `Contexto` | `string` (JSON inmutable) | Instantánea del diccionario del contrato, sin claves prohibidas incluso anidadas; mapeada a jsonb |

**Fábrica:** `RegistroAuditoria.Crear(codigo, actorId, actorRol, objetivo, contexto, ahora)` → `ResultadoRegistro(Registro, Error)`.
Rechaza acción desconocida, contexto prohibido, actor incoherente u objetivo largo; Error no contiene los valores recibidos.
No hay métodos que cambien un registro creado (R-011).

## 3. Catálogo (`CatalogoAcciones`)

| `AccionAuditada` | Código | `CategoriaAuditoria` | `EsDeRiesgo` |
|---|:---:|---|:---:|
| `UsuarioCreado` | 1 | `Usuarios` | no |
| `RolCambiado` | 2 | `Usuarios` | **sí** |
| `CuentaDesactivada` | 3 | `Usuarios` | no |
| `CuentaActivada` | 4 | `Usuarios` | no |
| `CuentaEliminada` | 5 | `CuentaDelTitular` | no |
| `InicioSesionFallido` | 6 | `Sesion` | no |
| `CuentaBloqueada` | 7 | `Sesion` | **sí** |
| `InicioSesionAdministrador` | 8 | `Sesion` | no |
| `RecuperacionSolicitada` | 9 | `CuentaDelTitular` | no |
| `ContrasenaCambiada` | 10 | `CuentaDelTitular` | no |
| `RevisionPrivilegios` | 11 | `Usuarios` | no |
| `AnomaliaModerada` | 12 | `Moderacion` | no |
| `DatasetExportado` | 13 | `Exportacion` | **sí** |
| `AccesoAdministrativoDatos` | 14 | `AccesoADatos` | no |

## 4. Contexto esperado por acción (lo llena el módulo que actúa)

| Acción | Objetivo | Contexto (ejemplo) |
|---|---|---|
| `RolCambiado` | id de la cuenta | `{ "desde": "ConductorRegistrado", "hacia": "Administrador" }` |
| `CuentaDesactivada` / `CuentaActivada` | id de la cuenta | `{ "motivo": "Solicitud del titular" }` |
| `CuentaEliminada` | id de la cuenta | `{ "sesionesRevocadas": 2 }` |
| `InicioSesionFallido` | id de la cuenta (o `null` si no existe) | `{ "intento": 3, "maximo": 3, "canal": "app" }` |
| `CuentaBloqueada` | id de la cuenta | `{ "duracionMinutos": 5, "nivel": 1 }` |
| `InicioSesionAdministrador` | id de la cuenta | `{ "canal": "panel", "ip": "190.xxx.xxx.21", "venceUtc": "…" }` |
| `ContrasenaCambiada` | id de la cuenta | `{ "via": "enlace de recuperación", "sesionesCerradas": true }` |
| `RevisionPrivilegios` | `null` | `{ "confirmados": ["…"], "retirados": [] }` |
| `AnomaliaModerada` | id de la anomalía | `{ "accion": "MarcarReparada", "desde": "Activa", "hacia": "Reparada" }` |
| `DatasetExportado` | id de la exportación | `{ "formato": "CSV", "desde": "…", "hasta": "…", "tipo": null, "estado": "Activa", "filas": 312 }` |
| `AccesoAdministrativoDatos` | recurso consultado (`auditoria`, `usuarios`, id del usuario en el detalle) | `{ "filtros": { … } }` |

**Cómo se registra (D15):** las acciones que escriben usan `RegistrarAsync` (transacción del llamador obligatoria); `AccesoAdministrativoDatos` y `DatasetExportado` usan `RegistrarLecturaAsync` (transacción propia). Las acciones de riesgo (`RolCambiado`, `CuentaBloqueada`, `DatasetExportado`) emiten `killauru.seguridad.acciones_riesgo{accion}` solo después del commit; Auditoría es el único emisor.

## 5. Lectura (`FiltrosAuditoria` → `PaginaAuditoria`)

| Filtro | Regla |
|---|---|
| `desde`, `hasta` | Opcionales; si vienen ambos, `desde ≤ hasta` |
| `categoria` | Opcional; uno de `CategoriaAuditoria` |
| `accion` | Opcional; uno de `AccionAuditada` |
| `actorId` | Opcional; `uuid` |
| `pagina`, `tamano` | `pagina ≥ 1`; `1 ≤ tamano ≤ 200`; por defecto 1 y 50 |

Orden: `ocurrido_utc DESC, id DESC`. Lectura con `AsNoTracking` y proyección directa al DTO (`principios-de-diseño.md` regla 9).

## 6. Consulta para después de restaurar (RN-10)

```sql
-- Cuentas eliminadas después del momento del respaldo: se vuelven a eliminar antes de reabrir el servicio
SELECT objetivo AS usuario_id, ocurrido_utc
FROM auditoria.registro
WHERE accion = 5 AND ocurrido_utc > @momento_del_respaldo
ORDER BY ocurrido_utc;
```

**Límite detectado (ver `research.md` R-009):** esta consulta se ejecuta sobre la BD **anterior** a la restauración. Si esa BD ya no se puede leer (pérdida total), las eliminaciones posteriores al respaldo también se perdieron con ella: **riesgo aceptado** acotado por el RPO de 24 h (D9); `restaurar.md` pide revisar a mano las solicitudes de eliminación del último día.
