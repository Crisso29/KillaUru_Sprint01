# Data Model: Módulo Ingesta

## 1. Tabla (`V0006__ingesta.sql`)

Base: `diseño-interno-de-modulos.md` §3.2. Lo agregado está marcado con `-- (+)`.

```sql
CREATE TABLE ingesta.evento_recibido (
    evento_id      uuid        PRIMARY KEY,                                   -- idempotencia (DA-02)
    lote_id        uuid        NULL,                                          -- (+) lo genera el servidor; NULL a las 72 h (D3)
    recibido_utc   timestamptz NOT NULL,                                      -- una sola hora por lote; redondeada a la hora a las 72 h (D3)
    detectado_utc  timestamptz NOT NULL,                                      -- hora GPS del celular (RF-03)
    seudonimo      uuid        NULL,                                          -- NULL a las 72 h de recibido (RN-09)
    con_sesion     boolean     NOT NULL,                                      -- rol ConductorRegistrado y correo verificado (RN-14, D8)
    estado         smallint    NOT NULL DEFAULT 0 CHECK (estado IN (0, 1, 2)),-- 0 Recibido · 1 Procesado · 2 Descartado
    motivo         smallint    NULL CHECK (motivo BETWEEN 1 AND 8),           -- (+) MotivoDescarte del procesamiento (T-BD-CHK)
    payload        jsonb       NOT NULL                                       -- evento sin seudónimo (ADR-014)
);
CREATE INDEX ix_recibido_pendiente ON ingesta.evento_recibido (recibido_utc) WHERE estado = 0;   -- barredor y salud
CREATE INDEX ix_recibido_tope      ON ingesta.evento_recibido (seudonimo, detectado_utc)
                                    WHERE seudonimo IS NOT NULL;                                 -- RN-15a
CREATE INDEX ix_recibido_fecha     ON ingesta.evento_recibido (recibido_utc);                     -- purga de 30 días
CREATE INDEX ix_recibido_purga ON ingesta.evento_recibido (recibido_utc) WHERE seudonimo IS NOT NULL; -- (+) purga de 72 h (T-BD06)
CREATE INDEX ix_recibido_lote      ON ingesta.evento_recibido (lote_id) WHERE estado = 0;        -- (+) trabajador
```

`motivo` usa los códigos de `MotivoDescarte` (004): 1 `EsquemaInvalido` … 7 `VelocidadImposible`, 8 `SeudonimoInconsistente` (D2). En la práctica solo llega `VelocidadImposible` (y, si cambió una regla entre la recepción y el procesamiento, alguno de recepción); `SeudonimoInconsistente` se rechaza en la recepción y nunca se guarda, pero el CHECK admite todo el enum.

## 2. Estados de una fila

| Desde | Hacia | Disparador |
|---|---|---|
| (nace) | `Recibido` (0) | `INSERT` de la recepción |
| `Recibido` | `Procesado` (1) | `ProcesarReporteAsync` devuelve `Procesado` o `YaProcesado` |
| `Recibido` | `Descartado` (2) | `ProcesarReporteAsync` devuelve `Descartado(motivo)` |
| `Recibido` | `Recibido` | error al procesar: el barredor reintenta |
| cualquiera | (sin seudónimo, sin `lote_id`, hora redondeada) | purga de 72 h (R-011) |
| `Procesado` / `Descartado` | (borrada) | purga de 30 días (R-008) |

**Conciliación (RNF-06):** `aceptados` (métrica) = `count(estado = 1) + count(estado = 2) + count(estado = 0)` en la ventana de 30 días.

## 3. Purgas (`PurgasRecepcionPostgres`)

```sql
-- purga-seudonimos-recepcion: cada hora en el minuto :10 y al arrancar (RN-09; D3, T-OPS05)
-- @antesDe = ahora − (PurgaSeudonimoHoras − 1) h: con la tarea horaria, ningún seudónimo supera las 72 h
UPDATE ingesta.evento_recibido
SET seudonimo = NULL, lote_id = NULL, recibido_utc = date_trunc('hour', recibido_utc)
WHERE evento_id IN (SELECT evento_id FROM ingesta.evento_recibido
                    WHERE seudonimo IS NOT NULL AND recibido_utc < @antesDe   -- ix_recibido_purga
                    LIMIT 5000);

-- purga-recepcion: 03:00 hora local y al arrancar (RN-10; R-008)
DELETE FROM ingesta.evento_recibido
WHERE evento_id IN (SELECT evento_id FROM ingesta.evento_recibido
                    WHERE estado IN (1, 2) AND recibido_utc < @antesDe        -- ix_recibido_fecha
                    LIMIT 5000);
```

`@antesDe` sale de `TimeProvider` (nunca `now()`); cada sentencia se repite hasta afectar 0 filas.

## 4. Cuerpo del lote (entrada HTTP)

```json
{
  "eventos": [
    {
      "eventoId": "0192f1b5-0a44-7d20-8c1e-5e3b9f2d6a70",
      "detectadoUtc": "2026-10-20T14:32:05Z",
      "latitud": -13.2105, "longitud": -74.2581, "precisionMetros": 6.5,
      "tipo": "Bache", "severidad": 4, "confidence": 0.91,
      "modeloVersion": "1.2.0",
      "seudonimo": "6b1c9d3e-2f4a-4b8e-9c7d-1a2b3c4d5e6f"
    }
  ]
}
```

| Campo | Tipo esperado | Si falta o es de otro tipo |
|---|---|---|
| `loteId` | — (ya no va en el cuerpo) | si llega, se ignora; el servidor genera un UUIDv7 (D3) |
| `eventos` | arreglo de 1 a 50 | 400 `lote_invalido` |
| `eventoId` | uuid | el evento sale en `rechazados` con `id: null`, su `indice` y `EsquemaInvalido` (D5) |
| `seudonimo` | uuid igual al del lote (cabecera o primer evento válido) | inválido → `EsquemaInvalido`; otro uuid válido → `SeudonimoInconsistente` (D2) |
| resto de campos | ver ejemplo | el evento sale en `rechazados` con `EsquemaInvalido` (R-001) |

Respuesta 200: `{ loteId, aceptados: [uuid], duplicados: [uuid], rechazados: [{ id: uuid | null, indice: 0…49, motivo }] }`.

## 5. Tipos del dominio y la aplicación

| Capa | Tipo | Campos / forma | Nota |
|---|---|---|---|
| Domain | `EstadoRecepcion` | `Recibido = 0`, `Procesado = 1`, `Descartado = 2` | sin tipos de Anomalías (D4) |
| Domain | `HoraDeDeteccion` | `Bloque(detectadoUtc)` → [hh:00, hh:00 + 1 h) UTC | RN-15a |
| Application | `LoteEntrante` | `IReadOnlyList<JsonElement> Eventos`, `bool ConSesion`, `Guid? SeudonimoCabecera` | lo arma el controlador (D4) |
| Application | `SeudonimoDelLote` | `Resolver(cabecera, eventosLeidos)` → `Guid?` | regla de R-010 (D2) |
| Application | `LoteAceptado` | `LoteId` (UUIDv7 del servidor), `RecibidoUtc`, `ConSesion`, `IReadOnlyList<EventoReportado> Aceptados`, `IReadOnlyList<Guid> RepetidosEnLote` | `ComoParametros()` → arreglos para `unnest` |
| Application | `Rechazo` | `Guid? Id`, `int Indice`, `MotivoDescarte Motivo` | D5 |
| Application | `ResultadoLote` | `Aceptado(Guid LoteId, Guid[] Aceptados, Guid[] Duplicados, IReadOnlyList<Rechazo> Rechazados)` · `Invalido` · `Saturado(TimeSpan ReintentarEn)` | patrón *Result* |
| Application | `LoteRecibido` | `Guid? LoteId` | elemento de la cola; `null` = filas `Recibido` sin `lote_id` (R-011) |
| Application | `EventoPendiente` | `EventoReportado Evento`, `bool ConSesion`, `DateTimeOffset RecibidoUtc` | lo devuelve `RecibidosDelLoteAsync` (payload + columnas) |

## 6. Puertos

| Puerto | Operaciones |
|---|---|
| `IRecepcionEventos` | `Task<IReadOnlySet<Guid>> InsertarAsync(LoteAceptado lote, CancellationToken ct)` → ids insertados |
| `IColaLotes` | `bool HayCapacidad()` · `bool TryEscribir(LoteRecibido aviso)` · `IAsyncEnumerable<LoteRecibido> LeerAsync(CancellationToken ct)` |
| `IPendientesRepositorio` | `RecibidosDelLoteAsync(loteId?)` (orden `detectado_utc, evento_id`) · `ContarEnLaHoraAsync(seudonimo, inicioHora)` · `MarcarAsync(eventoId, estado, motivo?)` — lo usa el trabajador |
| `IBarridoPendientes` | `LotesPendientesAsync(anteriorA?, maximo)` · `MasAntiguoPendienteAsync()` — lo usan el barredor y la salud (T-ARQ08: separado para que ningún archivo pase de 200 líneas) |
| `IPurgasRecepcion` | `PurgarSeudonimosAsync(antesDe)` · `BorrarFinalesAsync(antesDe)`; ambos por lotes de 5 000 (§3), devuelven filas afectadas |

Métricas que emite el módulo (`IMetricas`, 001): `RegistrarLote`, `RegistrarRechazo` (motivo), `ObservarCola`, `RegistrarRetrasoCola(TimeSpan)` → `killauru.ingesta.retraso_cola_s` (D11) y `RegistrarTareaCorrecta` (`purga-seudonimos-recepcion`, `purga-recepcion`).

## 7. `IngestaOptions` (sección `Ingesta`)

```json
"Ingesta": {
  "MaxEventosPorLote": 50,
  "CapacidadCola": 2000,
  "Trabajadores": 0,
  "ReintentarEnSegundos": 30,
  "IntervaloBarredorSegundos": 60,
  "AntiguedadMinimaBarredorSegundos": 60,
  "AntiguedadMaximaColaMinutos": 5,
  "PurgaSeudonimoHoras": 72,
  "RetencionRecepcionDias": 30,
  "TamanoMaximoCuerpoKb": 256
}
```

Validación al arrancar (`ValidateOnStart`): todos > 0 salvo `Trabajadores` (≥ 0); `MaxEventosPorLote` ≤ 50 (contrato de la app).
