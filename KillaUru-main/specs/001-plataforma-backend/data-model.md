# Data Model: Plataforma base del backend

Esta feature no tiene datos de negocio. Define la **base de datos vacía y segura**, el **kernel compartido** y los **puertos técnicos** que usan los módulos.

## 1. Base de datos: arranque y privilegios (T-BD01)

### 1.1 `V0000__arranque.sql` — una sola vez, como superusuario (`migrar.sh --arranque`, `KU_PG_SUPERUSUARIO`)

```sql
-- Roles (contraseñas por variables de psql desde el inventario de secretos, RNF-34)
CREATE ROLE killauru_migrador LOGIN PASSWORD :'pw_migrador';   -- DDL, solo durante el paso de migración (RNF-18)
CREATE ROLE killauru_app      LOGIN PASSWORD :'pw_app';        -- solo DML

CREATE EXTENSION IF NOT EXISTS postgis;
CREATE EXTENSION IF NOT EXISTS citext;        -- correo sin distinguir mayúsculas (feature 003)

REVOKE ALL ON DATABASE killauru FROM PUBLIC;
GRANT CONNECT ON DATABASE killauru TO killauru_app, killauru_migrador;

-- Un esquema por módulo (ADR-012). Consulta de Mapa no tiene esquema.
CREATE SCHEMA identidad   AUTHORIZATION killauru_migrador;
CREATE SCHEMA ingesta     AUTHORIZATION killauru_migrador;
CREATE SCHEMA anomalias   AUTHORIZATION killauru_migrador;
CREATE SCHEMA auditoria   AUTHORIZATION killauru_migrador;
CREATE SCHEMA dashboard   AUTHORIZATION killauru_migrador;
CREATE SCHEMA migraciones AUTHORIZATION killauru_migrador;

-- La app usa los esquemas, pero no los crea ni los altera
GRANT USAGE ON SCHEMA identidad, ingesta, anomalias, auditoria, dashboard TO killauru_app;
GRANT USAGE ON SCHEMA migraciones TO killauru_app;             -- solo lectura, para ChequeoMigraciones

-- Registro de migraciones aplicadas (R-009)
CREATE TABLE migraciones.aplicada (
    version      text        PRIMARY KEY,                    -- 'V0001'
    descripcion  text        NOT NULL,
    aplicada_utc timestamptz NOT NULL,
    fase         smallint    NOT NULL CHECK (fase IN (1, 2)) -- 1 agregar · 2 borrar (RNF-18)
);
ALTER TABLE migraciones.aplicada OWNER TO killauru_migrador;
GRANT SELECT ON migraciones.aplicada TO killauru_app;
INSERT INTO migraciones.aplicada VALUES ('V0000', 'arranque', clock_timestamp(), 1);
```

### 1.2 `V0001__privilegios.sql` — con `killauru_migrador` (`psql -v ON_ERROR_STOP=1 --single-transaction`)

```sql
ALTER DEFAULT PRIVILEGES FOR ROLE killauru_migrador IN SCHEMA identidad, ingesta, anomalias, dashboard
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO killauru_app;
ALTER DEFAULT PRIVILEGES FOR ROLE killauru_migrador IN SCHEMA auditoria
    GRANT SELECT, INSERT ON TABLES TO killauru_app;          -- append-only (RNF-31); detalle en 002
ALTER DEFAULT PRIVILEGES FOR ROLE killauru_migrador IN SCHEMA identidad, ingesta, anomalias, auditoria, dashboard
    GRANT USAGE, SELECT ON SEQUENCES TO killauru_app;
```

`migrar.sh` registra cada `V*.sql` en `migraciones.aplicada` dentro de la misma transacción del script. **Regla de migraciones (RNF-18):** cada cambio que **borra** algo (columna, tabla, índice) va en una migración de fase 2, en un despliegue posterior a la versión que dejó de usarlo. **Prueba:** ninguna FK cruza esquemas (`SinFkEntreEsquemasTests`).

**Respaldo:** `pg_dumpall --roles-only` + `pg_dump -Fc` (R-011). **Restauración:** roles → datos → pasos de D9 (R-017).

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
| `RegistrarRetrasoCola(TimeSpan retraso)` | histograma `killauru.ingesta.retraso_cola_s` (procesado − recibido) | Ingesta (RNF-23; D11) |
| `ObservarCola(Func<(int Tamano, double AntiguedadSegundos)> lectura)` | 2 *gauges* observables | Ingesta (RNF-22) |
| `RegistrarTareaCorrecta(string tarea)` | *gauge* `killauru.tarea.ultima_ok{tarea}` (segundos Unix) | Todas las tareas programadas de la API |
| `RegistrarAccionRiesgo(string accion)` | contador `killauru.seguridad.acciones_riesgo{accion}` | **Solo Auditoría (002)**, con `AlConfirmar` (RNF-29; D15) |

**Catálogo de `tarea`** (T-OPS05): `caducidad`, `purga-seudonimos`, `purga-seudonimos-recepcion`, `purga-recepcion`, `purga-historica`, `purga-identidad`, `indicadores`, `backup`. El backup es otro contenedor: no usa `IMetricas`, escribe `killauru_tarea_ultima_ok{tarea="backup"} <unix>` en un `.prom` que lee Alloy (D12).

### 3.3 Contexto de transacción (`Shared/Persistencia`, D15)

```csharp
public interface IContextoTransaccion          // scoped: uno por petición o por trabajo
{
    DbConnection? Conexion { get; }
    DbTransaction? Transaccion { get; }
    void Unir(DbConnection conexion, DbTransaction transaccion);   // lo llama el módulo que abre la transacción
    void AlConfirmar(Func<Task> accion);       // se ejecuta DESPUÉS del commit; se descarta si hay rollback
    Task ConfirmarAsync(CancellationToken ct); // commit → acciones de AlConfirmar → libera (Conexion/Transaccion = null)
    Task RevertirAsync(CancellationToken ct);  // rollback → descarta acciones → libera
}
```

Uso (ADR-013): el módulo que actúa abre su transacción, llama `Unir(...)`, hace la acción, llama `IAuditoriaService.RegistrarAsync` (que crea su `DbContext` sobre **esa** conexión con `UseTransaction`) y termina con `ConfirmarAsync` o `RevertirAsync` del contexto. Si no hay transacción unida, `RegistrarAsync` lanza `InvalidOperationException`; las lecturas sensibles usan `RegistrarLecturaAsync`, con transacción propia (002). `Unir` con una transacción ya unida y sin liberar también lanza `InvalidOperationException`.

### 3.4 Programa de tareas (`Shared/Tareas/ProgramaReloj.cs`, T-OPS05)

| Método | Resultado |
|---|---|
| `ProximaHoraria(DateTimeOffset ahora, int minuto)` | próximo instante con ese minuto (estrictamente después de `ahora`) |
| `ProximaDiaria(DateTimeOffset ahora, TimeOnly horaLocal)` | próximo instante con esa hora local (UTC−5 fijo) |

Calendario: al arrancar + caducidad `:05`, purgas de seudónimo `:10`; 00:30 indicadores, 02:00 backup, 03:00 purga de recepción, 03:30 purga histórica, 03:45 purga de Identidad.

## 4. Configuración del host (`ApiOptions`, sección `Api`)

| Propiedad | Valor inicial | Fuente |
|---|---|---|
| `LimiteTasa.AnonimoConsultasPorHora` | 60 | RNF-11 |
| `LimiteTasa.AnonimoLotesPorHora` | 120 | RNF-11 |
| `LimiteTasa.RegistradoPorHora` | 1 000 | RNF-11 |
| `LimiteTasa.AdministradorPorHora` | 3 000 | RNF-11 |
| `LimiteTasa.TechoIpLotesPorHora` | 600 (solo anónimos, clave IP real) | D2 |
| `LimiteTasa.TechoIpConsultasPorHora` | 300 (solo anónimos, clave IP real) | D2 |
| `LimiteTasa.RegistrosPorHoraPorIp` | 5 (política `registro`) | D8 |
| `Telemetria.MuestreoTrazas` | 0,10 | RNF-32 |
| `Telemetria.OtlpEndpoint` | (secreto/entorno) | ADR-010 |
| `DocumentacionPublica` | `false` (`true` en Development y Pruebas) | Constitución IX |
| `OrigenPanel` | `https://panel.killauru.pe` | RNF-09 |
| `RedTunel` | subred interna de Docker donde vive `cloudflared` | RNF-11 |
| `Jwt.Emisor`, `Jwt.Audiencia` | `https://api.killauru.pe`, `killauru` (sin tolerancia de reloj: `ClockSkew = 0`) | ADR-009, RNF-26 |
| `Jwt.ClaveFirma` | (secreto) | RNF-34 |

`appsettings.Pruebas.json` sube los límites de tasa para las pruebas de carga (D11).

## 5. Variables de entorno y secretos (sin valores)

- `deploy/.env.example`: `KU_PG_SUPERUSUARIO` (solo para `migrar.sh --arranque` y la imagen de PostgreSQL), `KU_DB_APP_PASSWORD`, `KU_DB_MIGRADOR_PASSWORD`, `KU_JWT_CLAVE_FIRMA`, `KU_CORREO_API_KEY`, `KU_R2_ACCESS_KEY_ID`, `KU_R2_SECRET_ACCESS_KEY`, `KU_GRAFANA_OTLP_TOKEN`, `KU_TUNEL_TOKEN`, `KU_BACKUP_CLAVE_PUBLICA` (la privada **nunca** en la VM).
- Secretos de GitHub (entorno `produccion`, D10): `KU_CF_ACCESS_CLIENT_ID`, `KU_CF_ACCESS_CLIENT_SECRET`, `KU_DEPLOY_SSH_KEY`.

## 6. Compose (límites y operación)

| Servicio | Producción | Pruebas (`killauru-pruebas`) |
|---|---|---|
| Todos | `restart: unless-stopped`; logs `json-file` con `max-size: 10m`, `max-file: 3` | igual |
| `api` | *healthcheck* `/health/live`; sin puertos publicados; límites de CPU y memoria | API + BD con `cpus: 2`, `mem_limit: 6g` en total; volumen aparte; `ASPNETCORE_ENVIRONMENT=Pruebas` |
| `alloy` | `grafana/alloy:<versión fija>`; lee `/var/lib/docker/containers` (logs), métricas del host y el volumen `metricas-backup` (`.prom`) | no se levanta |
| `backup` | cron 02:00 local; escribe el `.prom` en `metricas-backup` | no se levanta |
