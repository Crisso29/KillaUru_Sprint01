# Data Model: Módulo Identidad y Accesos

## 1. Tablas (`V0003__identidad.sql`)

Base: `diseño-interno-de-modulos.md` §3.4. Lo **agregado** está marcado con `-- (+)` y justificado en `plan.md` → Complexity Tracking.

```sql
CREATE TABLE identidad.usuario (
    id                        uuid        PRIMARY KEY,                 -- UUIDv7
    correo                    citext      NOT NULL UNIQUE,
    hash_contrasena           text        NULL,                        -- (+) NULL = invitación pendiente. PBKDF2, nunca en claro
    rol                       smallint    NOT NULL CHECK (rol IN (1, 2)),   -- 1 ConductorRegistrado · 2 Administrador
    activo                    boolean     NOT NULL DEFAULT true,
    correo_verificado         boolean     NOT NULL DEFAULT false,
    intentos_fallidos         smallint    NOT NULL DEFAULT 0,          -- RN-19
    ventana_fallos_desde_utc  timestamptz NULL,                        -- (+) inicio de la ventana de 15 min
    nivel_bloqueo             smallint    NOT NULL DEFAULT 0,
    bloqueado_hasta           timestamptz NULL,
    creado_utc                timestamptz NOT NULL                     -- truncado al día (D3)
);

CREATE TABLE identidad.sesion_refresh (
    id                   uuid        PRIMARY KEY,
    usuario_id           uuid        NOT NULL REFERENCES identidad.usuario (id) ON DELETE CASCADE,
    familia_id           uuid        NOT NULL,                         -- todos los tokens de una misma sesión
    hash_token           bytea       NOT NULL UNIQUE,                  -- SHA-256 del token
    creado_utc           timestamptz NOT NULL,                         -- (+) para RN-17 (renovó en los últimos N días); truncado a la hora (D3)
    expira_utc           timestamptz NOT NULL,                         -- truncado hacia abajo a la hora (D3)
    limite_absoluto_utc  timestamptz NULL,                             -- (+) inicio + 8 h (admin); NULL (conductor); exacto
    usado_utc            timestamptz NULL,                             -- exacto (gracia de 60 s); la purga lo trunca a la hora pasados 2 min (D3)
    revocado_utc         timestamptz NULL                              -- truncado a la hora (D3)
);
CREATE INDEX ix_sesion_usuario_fk ON identidad.sesion_refresh (usuario_id);       -- completo: sirve al ON DELETE CASCADE (BD-05)
CREATE INDEX ix_sesion_familia ON identidad.sesion_refresh (familia_id);
CREATE INDEX ix_sesion_creado  ON identidad.sesion_refresh (creado_utc);          -- (+) RN-17
CREATE INDEX ix_sesion_expira  ON identidad.sesion_refresh (expira_utc);          -- (+) purga (D3)

CREATE TABLE identidad.consentimiento (
    usuario_id   uuid        NOT NULL REFERENCES identidad.usuario (id) ON DELETE CASCADE,
    documento    smallint    NOT NULL CHECK (documento IN (1, 2)),     -- 1 Política de privacidad · 2 Términos de uso
    version      text        NOT NULL,
    aceptado_utc timestamptz NOT NULL,                 -- truncado al día (D3): no revela la hora del registro
    PRIMARY KEY (usuario_id, documento, version)                       -- RF-29, RF-37
);

CREATE TABLE identidad.token_correo (
    hash_token  bytea       PRIMARY KEY,
    usuario_id  uuid        NOT NULL REFERENCES identidad.usuario (id) ON DELETE CASCADE,
    proposito   smallint    NOT NULL CHECK (proposito IN (1, 2, 3)),   -- 1 Verificación 24 h · 2 Recuperación 30 min · (+) 3 Invitación 24 h
    expira_utc  timestamptz NOT NULL,
    usado_utc   timestamptz NULL
);
CREATE INDEX ix_token_usuario ON identidad.token_correo (usuario_id, proposito) WHERE usado_utc IS NULL;
CREATE INDEX ix_token_usuario_fk ON identidad.token_correo (usuario_id);          -- completo: sirve al ON DELETE CASCADE (BD-05)

CREATE TABLE identidad.revision_privilegios (                          -- (+) RNF-30
    id             bigint      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    realizada_utc  timestamptz NOT NULL,
    actor_id       uuid        NOT NULL,                               -- sin FK: el actor puede ser eliminado después
    confirmados    smallint    NOT NULL,
    retirados      smallint    NOT NULL
);
```

**Privacidad (RNF-10):** ninguna tabla tiene seudónimos, ubicaciones, eventos ni trayectorias, y las horas de sesión no permiten unir una renovación con un lote (D3, `research.md` R-013). Eliminar una cuenta borra en cascada sus sesiones, consentimientos y tokens (todo dentro del esquema `identidad`), con índices completos por `usuario_id`.

### 1.1 Purga de Identidad (`PurgaIdentidadRepositorio`, tarea `purga-identidad`, 03:45 local y al arrancar)

```sql
-- @ahora del TimeProvider; @dias = RetencionIdentidadDias (30); @minutos = TruncarUsoTrasMinutos (2). En lotes de 5 000.
DELETE FROM identidad.sesion_refresh
 WHERE expira_utc < @ahora - make_interval(days => @dias)
    OR revocado_utc < @ahora - make_interval(days => @dias);
DELETE FROM identidad.token_correo
 WHERE expira_utc < @ahora - make_interval(days => @dias)
    OR usado_utc  < @ahora - make_interval(days => @dias);
UPDATE identidad.sesion_refresh SET usado_utc = date_trunc('hour', usado_utc)
 WHERE usado_utc < @ahora - make_interval(mins => @minutos)
   AND usado_utc <> date_trunc('hour', usado_utc);
```

### 1.2 Restauración de un respaldo (sección «Identidad» de `restaurar.md`, D9)

```sql
UPDATE identidad.sesion_refresh SET revocado_utc = date_trunc('hour', @ahora) WHERE revocado_utc IS NULL;
-- y rotar KU_JWT_CLAVE_FIRMA antes de arrancar la API
```

## 2. Entidades del dominio

### 2.1 `Usuario`

| Campo | Tipo | Regla |
|---|---|---|
| `Id` | `Guid` | `Guid.CreateVersion7()` |
| `Correo` | `Correo` (value object) | Recortado, minúsculas, formato básico `algo@dominio.tld`, ≤ 254 caracteres |
| `HashContrasena` | `string?` | `null` solo si la invitación está pendiente |
| `Rol` | `RolUsuario` | `ConductorRegistrado` o `Administrador` |
| `Activo`, `CorreoVerificado` | `bool` | — |
| Bloqueo | `IntentosFallidos`, `VentanaFallosDesdeUtc`, `NivelBloqueo`, `BloqueadoHasta` | RN-19 |
| `CreadoUtc` | `DateTimeOffset` | Del `TimeProvider`, truncado al día local (`HorasGuardadas.AlDia`, D3) |

**Estado mostrado** (derivado): `InvitacionPendiente` si `HashContrasena == null`; `Desactivada` si `!Activo`; si no, `Activa`.

**Métodos:** `RegistrarConductor(correo, hash, ahora)`, `Invitar(correo, rol, ahora)`, `FijarContrasena(hash)`, `MarcarCorreoVerificado()`, `CambiarRol(rol)`, `Desactivar()`, `Activar()`, `RegistrarFallo(ahora, parametros)` → `bool seBloqueo`, `RegistrarExito()`, `EstaBloqueado(ahora)`, `PuedeIniciarSesion(ahora)`.

### 2.2 Bloqueo (RN-19) — tabla de verdad

| Situación | Resultado |
|---|---|
| Fallo con ventana iniciada hace > 15 min o sin ventana | `intentos = 1`, ventana = ahora |
| Fallo que deja `intentos = 3` dentro de la ventana | `bloqueado_hasta = ahora + Duraciones[min(nivel, 2)]`, `nivel + 1`, `intentos = 0`, ventana = null |
| Inicio correcto | `intentos = 0`, `nivel = 0`, ventana = null |
| Contraseña restablecida | Igual que inicio correcto y `bloqueado_hasta = null` |

Ejemplo: 1.º bloqueo 5 min, 2.º 15 min, 3.º y siguientes 60 min.

### 2.3 `SesionRenovacion` y `PoliticaSesion`

| Rol | `expira_utc` de cada token nuevo | `limite_absoluto_utc` |
|---|---|---|
| ConductorRegistrado | `ahora + 30 días` | `null` |
| Administrador | `min(ahora + 8 h, limite_absoluto)` | `inicio de sesión + 8 h` (exacto) |

`expira_utc`, `creado_utc` y `revocado_utc` se truncan hacia abajo a la hora al guardarse (`HorasGuardadas.ALaHora`, D3); `usado_utc` queda exacto hasta la purga. El token de acceso lleva `sub`, `rol`, `verificado` y `jti` (D8).

**Renovar (tabla de decisión):**

| Token recibido | Cuenta | Resultado |
|---|---|---|
| No existe (hash desconocido) | — | 401 `sesion_invalida` |
| Revocado o vencido | — | 401 `sesion_invalida` |
| Válido, no usado | Desactivada | revoca familia → 401 `cuenta_desactivada` |
| Válido, no usado | Activa | marca usado; emite derivado; 200 |
| Usado hace ≤ 60 s | Activa | recalcula el mismo derivado; emite acceso nuevo; 200 |
| Usado hace > 60 s | — | revoca familia → 401 `sesion_invalida` |

### 2.4 `TokenCorreo`

| Propósito | Vigencia | Al usarlo |
|---|---|---|
| `Verificacion` (1) | 24 h | `CorreoVerificado = true` |
| `Recuperacion` (2) | 30 min | fija contraseña, desbloquea, revoca sesiones |
| `Invitacion` (3) | 24 h | fija contraseña y `CorreoVerificado = true` |

Un solo uso; emitir uno nuevo del mismo propósito invalida los anteriores sin usar.

### 2.5 `RevisionPrivilegios`

`Vence(ultima)` = primer día del mes siguiente al de `ultima.RealizadaUtc` (hora de Lima, UTC−5). Sin revisiones previas: vence de inmediato.

## 3. `IdentidadOptions` (sección `Identidad` de `appsettings.json`)

```text
TokenAccesoMinutos = 5            SesionAdministradorHoras = 8      SesionConductorDias = 30
GraciaRotacionSegundos = 60       IntentosMaximos = 3               VentanaIntentosMinutos = 15
DuracionesBloqueoMinutos = [5, 15, 60]
VerificacionCorreoHoras = 24      RecuperacionMinutos = 30          InvitacionHoras = 24
RecuperacionesPorCorreoPorHora = 3                RecuperacionesPorIpPorHora = 10
ContrasenaMinima = 10             ContrasenaMaxima = 128                      (D13)
RetencionIdentidadDias = 30       TruncarUsoTrasMinutos = 2         HoraPurgaIdentidad = 03:45   (D3, T-OPS05)
VersionPolitica = "1.2"           VersionTerminos = "1.2"
UrlPolitica, UrlTerminos          (páginas públicas)
UrlBaseEnlaces = "https://panel.killauru.pe/cuenta"
Jwt: Emisor, Audiencia, ClaveFirma (secreto)      ClaveRotacion (secreto, R-003)
Turnstile: ClaveSecreta (secreto)
```

El tope de registros por IP no vive aquí: `LimiteTasa:RegistrosPorHoraPorIp = 5` está en `ApiOptions` (001) y lo aplica la política de límite de tasa del host (D8).

## 4. Secretos que agrega este módulo (RNF-34)

`KU_JWT_CLAVE_FIRMA` (ya en 001; se rota al restaurar un respaldo, D9), `KU_IDENTIDAD_CLAVE_ROTACION` (nuevo), `KU_CORREO_API_KEY` (ya en 001), `KU_TURNSTILE_SECRETO` (nuevo).
