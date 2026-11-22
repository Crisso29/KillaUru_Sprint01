# Cuello de botella del procesamiento: trabajadores y pool · local · 2026-11-22

> Continúa la [carga progresiva del 2026-11-21](../2026-11-21-local-progresiva-2000/README.md). [Evidencias](../../README.md) · [Estado del proyecto](../../../estado-implementacion.md)

| Campo | Valor |
|---|---|
| Entorno | `killauru-local` (`backend/deploy/docker-compose.local.yml`), mismos recursos que `killauru-pruebas`. k6 en el mismo PC |
| Ráfaga | `progresiva-2000.k6.js` con `ESCALONES=1200 SUBIDA_S=10 MANTENER_S=60`: unos 50 000 eventos en 70 s |
| Método | Cada caso parte de una base vacía (`down -v`). Tras la ráfaga se mide el vaciado de la cola durante 60 s |
| Alcance | Evidencia **local, misma máquina**; el disco de Docker Desktop en Windows es lento para confirmar commits |

## 1. ¿En qué esperan los trabajadores? (40 muestras de `pg_stat_activity` con 2 trabajadores)

| Estado de la conexión | Muestras | Significado |
|---|---:|---|
| `IO:WalSync` + `LWLock:WALWrite` | 26 (38 %) | Esperando que el disco confirme un COMMIT |
| `Client:ClientRead` dentro de la transacción | 19 (28 %) | La base espera a la API entre consultas |
| CPU | 20 (29 %) | Consultas ejecutándose |
| `Lock:advisory` | 2 (3 %) | Bloqueo de celdas entre trabajadores |

Cada evento hace unas 10 idas y vueltas y varios commits (Anomalías, `MarcarAsync` y consultas sueltas).

## 2. Más trabajadores (`Ingesta__Trabajadores`)

| Trabajadores | Eventos/s procesados | CPU API (tope 125 %) | Lote p95 en la ráfaga | 5xx |
|---:|---:|---:|---:|---:|
| 2 (valor por defecto con 1,25 CPU) | **150** | 85 % | 78 ms | 22 |
| 4 | 101 | 96 % | 1 100 ms | 33 |
| 8 | 78 | 73 % | 466 ms | 15 |

**Más trabajadores empeoran:** compiten por la CPU (tope de 1,25) y por las conexiones con la recepción.
**Se descarta** subirlos. Con 2 trabajadores y la base vacía salen 150 eventos/s, frente a los 77 medidos tras
160 000 reportes: el procesamiento **se vuelve más lento a medida que crecen los datos**. La hipótesis es la carga
del historial de 15 días por evento; está pendiente de medir.

## 3. Los 5xx: pool de conexiones sin tope

Los 70 errores anteriores fueron `Npgsql.PostgresException 53300: remaining connection slots are reserved...`
en `POST /api/v1/anomalies/batch`. `diseño-interno-de-modulos.md` §3.7 fija `Maximum Pool Size=30`, pero **no estaba
configurado** (Npgsql usa 100 por defecto y PostgreSQL admite 97). Ya lo había señalado ARQ-22 en la revisión del 2026-10-21.

Corrección: `Maximum Pool Size=30` en `appsettings.json` y en los tres compose, vigilado por
`backend/tests/KillaUru.Arquitectura.Tests/PoolDeConexionesTests.cs` (en rojo 0/4 antes del cambio).

| Misma ráfaga, 2 trabajadores | Antes | Después |
|---|---:|---:|
| 5xx | 22 | **0** |
| Errores 53300 en el log | 22 | **0** |
| «Error no controlado» | 22 | **0** |
| Conexiones máximas de la API | ~97 | 35 |

**Queda una desviación:** 35 > 30 porque la API tiene **3 pools** (los módulos con la cadena simple, Anomalías con
NetTopologySuite y el `NpgsqlDataSource` de los chequeos de salud), cada uno con tope 30: techo real de 90.
Para cumplir «un pool de 30» habría que compartir un único `NpgsqlDataSource` entre todos los `DbContext`.
