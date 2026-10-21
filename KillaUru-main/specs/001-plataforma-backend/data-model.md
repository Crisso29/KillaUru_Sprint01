# Data Model: Plataforma base del backend

Esta feature no tiene datos de negocio. Define la **base de datos vacía y segura**, el **kernel compartido** y los **puertos técnicos** que usan los módulos.

## 1. Base de datos: `V0001__esquemas_y_roles.sql`

```sql
-- Roles (contraseñas desde el inventario de secretos, RNF-34)
CREATE ROLE killauru_migrador LOGIN;          -- DDL, solo durante el paso de migración (RNF-18)
CREATE ROLE killauru_app      LOGIN;          -- solo DML

CREATE EXTENSION IF NOT EXISTS postgis;
CREATE EXTENSION IF NOT EXISTS citext;        -- correo sin distinguir mayúsculas (feature 003)

-- Un esquema por módulo (ADR-012). Consulta de Mapa no tiene esquema.
CREATE SCHEMA identidad AUTHORIZATION killauru_migrador;
CREATE SCHEMA ingesta   AUTHORIZATION killauru_migrador;
CREATE SCHEMA anomalias AUTHORIZATION killauru_migrador;
CREATE SCHEMA auditoria AUTHORIZATION killauru_migrador;
CREATE SCHEMA dashboard AUTHORIZATION killauru_migrador;

-- La app usa los esquemas, pero no los crea ni los altera
GRANT USAGE ON SCHEMA identidad, ingesta, anomalias, auditoria, dashboard TO killauru_app;
ALTER DEFAULT PRIVILEGES FOR ROLE killauru_migrador IN SCHEMA identidad, ingesta, anomalias, dashboard
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO killauru_app;
ALTER DEFAULT PRIVILEGES FOR ROLE killauru_migrador IN SCHEMA auditoria
    GRANT SELECT, INSERT ON TABLES TO killauru_app;          -- append-only (RNF-31); detalle en 002
ALTER DEFAULT PRIVILEGES FOR ROLE killauru_migrador IN SCHEMA identidad, ingesta, anomalias, auditoria, dashboard
    GRANT USAGE, SELECT ON SEQUENCES TO killauru_app;

-- Registro de migraciones aplicadas (R-009)
CREATE TABLE public.migracion_aplicada (
    version      text        PRIMARY KEY,                    -- 'V0001'
    descripcion  text        NOT NULL,
    aplicada_utc timestamptz NOT NULL DEFAULT now(),
    fase         smallint    NOT NULL CHECK (fase IN (1, 2)) -- 1 agregar · 2 borrar (RNF-18)
);
```

**Regla de migraciones (RNF-18):** cada cambio que **borra** algo (columna, tabla, índice) va en una migración de fase 2, en un despliegue posterior a la versión que dejó de usarlo.

## 2. Kernel compartido (`KillaUru.Shared/Domain`)

| Value object | Forma | Validación | Fuente |
|---|---|---|---|
| `Coordenada` | `readonly record struct (double Latitud, double Longitud)` | `Crear(lat, lon)` → `null` si lat ∉ [-90, 90] o lon ∉ [-180, 180] | `diseño-interno` §6.1 |
| `Severidad` | `readonly record struct` con `int Valor` y constructor privado | `Crear(v)` → `null` si v ∉ [1, 5]; `MedianaHaciaArriba(valores)`: con número par, promedio de los dos del medio; luego techo | RN-02, RN-05 |
| `Confidence` | `readonly record struct` con `double Valor` | `Crear(v)` → `null` si v ∉ [0.0, 1.0] | RN-04 |
| `SeudonimoInstalacion` | `readonly record struct (Guid Valor)` | `Crear(texto)` → `null` si no es un UUID o es `Guid.Empty` | RF-27 |
| `Celda` | `readonly record struct (long Id)`; paso 0,005° (≈ 550 m ≥ 5 × 100 m) | `De(Coordenada)`: `((floor(lat/0,005) + 18 000) × 100 000) + floor(lon/0,005) + 36 000`; `Alrededor(c, metros)`: celdas de las 4 esquinas de un cuadro de ±metros, sin repetir, **orden ascendente**; `Limites()`: (minLat, minLon, maxLat, maxLon) de la celda, inverso de `De` (lo usa Mapa, 006 R-002) | `diseño-interno` §3.3, §6.1 |
| `Roles` | clase estática con 3 constantes de texto | — | `10-glosario.md` §4 |

**Ejemplos que deben cumplir las pruebas:** `MedianaHaciaArriba({2,3,3,5}) = 3`; `MedianaHaciaArriba({3,4}) = 4`; `Severidad.Crear(0) = null`; `Severidad.Crear(6) = null`; un punto a 10 m de un borde de celda devuelve 2 celdas en `Alrededor(c, 100)`; cerca de una esquina, 4.

## 3. Puertos técnicos compartidos

### 3.1 Eventos de dominio (`Shared/Eventos`)

```csharp
public interface IEventoDominio { }
public interface IManejador<in TEvento> where TEvento : IEventoDominio
{
    Task ManejarAsync(TEvento evento, CancellationToken ct);
}
public interface IPublicadorEventos
{
    // El llamador lo invoca DESPUÉS de su commit (ADR-006). Ejecuta los manejadores en orden de registro;
    // la falla de un manejador se registra y no impide a los demás.
    Task PublicarAsync<TEvento>(TEvento evento, CancellationToken ct) where TEvento : IEventoDominio;
}
```

### 3.2 Métricas (`Shared/Metricas/IMetricas.cs`)

Sin tipos de ningún módulo y **sin etiquetas de alta cardinalidad** (RNF-32). El motivo es un texto corto de un catálogo cerrado (`MotivoDescarte`).

| Método | Instrumento OpenTelemetry | Lo usa |
|---|---|---|
| `RegistrarLote(int aceptados, int duplicados, int rechazados)` | 3 contadores `killauru.ingesta.eventos{resultado}` | Ingesta |
| `RegistrarRechazo(string motivo)` | contador `killauru.ingesta.rechazos{motivo}` | Ingesta (recepción, RN-11) |
| `RegistrarDescarte(string motivo)` | contador `killauru.anomalias.descartes{motivo}` | Anomalías (procesamiento) |
| `RegistrarRetenidoPorTope()` | contador `killauru.anomalias.retenidos_tope` | Anomalías (RN-15a) |
| `RegistrarDuracionProceso(TimeSpan d)` | histograma `killauru.anomalias.proceso_ms` | Anomalías (RNF-23) |
| `ObservarCola(Func<(int Tamano, double AntiguedadSegundos)> lectura)` | 2 *gauges* observables | Ingesta (RNF-22) |
| `RegistrarTareaCorrecta(string tarea)` | *gauge* `killauru.tarea.ultima_ok{tarea}` (segundos Unix) | Todas las tareas programadas |
| `RegistrarAccionRiesgo(string accion)` | contador `killauru.seguridad.acciones_riesgo{accion}` | Identidad, Anomalías (RNF-29) |

### 3.3 Contexto de transacción (`Shared/Persistencia`)

```csharp
public interface IContextoTransaccion          // scoped: uno por petición o por trabajo
{
    DbConnection? Conexion { get; }
    DbTransaction? Transaccion { get; }
    void Unir(DbConnection conexion, DbTransaction transaccion);   // lo llama el módulo que abre la transacción
}
```

Uso (ADR-013): el módulo que actúa abre su transacción y llama `Unir(...)`; `IAuditoriaService.RegistrarAsync` crea su `DbContext` sobre **esa** conexión y llama `UseTransaction`. Si no hay transacción activa, Auditoría abre la suya (acciones sin escritura propia, como un login fallido).

## 4. Configuración del host (`ApiOptions`, sección `Api`)

| Propiedad | Valor inicial | Fuente |
|---|---|---|
| `LimiteTasa.AnonimoConsultasPorHora` | 60 | RNF-11 |
| `LimiteTasa.AnonimoLotesPorHora` | 120 | RNF-11 |
| `LimiteTasa.RegistradoPorHora` | 1 000 | RNF-11 |
| `LimiteTasa.AdministradorPorHora` | 3 000 | RNF-11 |
| `Telemetria.MuestreoTrazas` | 0,10 | RNF-32 |
| `Telemetria.OtlpEndpoint` | (secreto/entorno) | ADR-010 |
| `DocumentacionPublica` | `false` (`true` en Development y pruebas) | Constitución IX |
| `OrigenPanel` | `https://panel.killauru.pe` | RNF-09 |
| `RedTunel` | subred interna de Docker donde vive `cloudflared` | RNF-11 |
| `Jwt.Emisor`, `Jwt.Audiencia` | `https://api.killauru.pe`, `killauru` | ADR-009 |
| `Jwt.ClaveFirma` | (secreto) | RNF-34 |

## 5. Variables de entorno (`deploy/.env.example`, sin valores)

`KU_DB_APP_PASSWORD`, `KU_DB_MIGRADOR_PASSWORD`, `KU_JWT_CLAVE_FIRMA`, `KU_CORREO_API_KEY`, `KU_R2_ACCESS_KEY_ID`, `KU_R2_SECRET_ACCESS_KEY`, `KU_GRAFANA_OTLP_TOKEN`, `KU_TUNEL_TOKEN`, `KU_BACKUP_CLAVE_PUBLICA` (la privada **nunca** en la VM).
