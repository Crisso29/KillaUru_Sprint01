# Data Model: Módulo Ingesta

## 1. Tabla (`V0006__ingesta.sql`)

Base: `diseño-interno-de-modulos.md` §3.2. Lo agregado está marcado con `-- (+)`.

```sql
CREATE TABLE ingesta.evento_recibido (
    evento_id      uuid        PRIMARY KEY,                                   -- idempotencia (DA-02)
    lote_id        uuid        NOT NULL,
    recibido_utc   timestamptz NOT NULL,                                      -- una sola hora por lote
    detectado_utc  timestamptz NOT NULL,                                      -- hora GPS del celular (RF-03)
    seudonimo      uuid        NULL,                                          -- NULL a las 72 h de recibido (RN-09)
    con_sesion     boolean     NOT NULL,                                      -- por el rol del token (RN-14)
    estado         smallint    NOT NULL DEFAULT 0 CHECK (estado IN (0, 1, 2)),-- 0 Recibido · 1 Procesado · 2 Descartado
    motivo         smallint    NULL,                                          -- MotivoDescarte del procesamiento
    payload        jsonb       NOT NULL                                       -- evento sin seudónimo (ADR-014)
);
CREATE INDEX ix_recibido_pendiente ON ingesta.evento_recibido (recibido_utc) WHERE estado = 0;   -- barredor y salud
CREATE INDEX ix_recibido_tope      ON ingesta.evento_recibido (seudonimo, detectado_utc)
                                    WHERE seudonimo IS NOT NULL;                                 -- RN-15a
CREATE INDEX ix_recibido_fecha     ON ingesta.evento_recibido (recibido_utc);                     -- purgas
CREATE INDEX ix_recibido_lote      ON ingesta.evento_recibido (lote_id) WHERE estado = 0;        -- (+) trabajador
```

`motivo` usa los códigos de `MotivoDescarte` (004): 1 `EsquemaInvalido` … 7 `VelocidadImposible`. En la práctica solo llega `VelocidadImposible` (y, si cambió una regla entre la recepción y el procesamiento, alguno de recepción).

## 2. Estados de una fila

| Desde | Hacia | Disparador |
|---|---|---|
| (nace) | `Recibido` (0) | `INSERT` de la recepción |
| `Recibido` | `Procesado` (1) | `ProcesarReporteAsync` devuelve `Procesado` o `YaProcesado` |
| `Recibido` | `Descartado` (2) | `ProcesarReporteAsync` devuelve `Descartado(motivo)` |
| `Recibido` | `Recibido` | error al procesar: el barredor reintenta |
| `Procesado` / `Descartado` | (borrada) | purga de 30 días (R-008) |

**Conciliación (RNF-06):** `aceptados` (métrica) = `count(estado = 1) + count(estado = 2) + count(estado = 0)` en la ventana de 30 días.

## 3. Cuerpo del lote (entrada HTTP)

```json
{
  "loteId": "0192f1b6-6f1e-7c3a-9a51-2b7d0e4c8a11",
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
| `loteId` | uuid, opcional | el servidor genera un UUIDv7 |
| `eventos` | arreglo de 1 a 50 | 400 `lote_invalido` |
| `eventoId` | uuid | 400 `lote_invalido` (R-002) |
| resto de campos | ver ejemplo | el evento sale `rechazados` con `EsquemaInvalido` (R-001) |

## 4. Tipos del dominio y la aplicación

| Tipo | Campos / forma | Nota |
|---|---|---|
| `LoteEntrante` | `Guid LoteId`, `IReadOnlyList<EventoReportado> Eventos`, `bool ConSesion` | lo arma el controlador |
| `LoteAceptado` | `LoteId`, `RecibidoUtc`, `ConSesion`, `IReadOnlyList<EventoReportado> Aceptados`, `IReadOnlyList<Guid> RepetidosEnLote` | `ComoParametros()` → arreglos para `unnest` |
| `ResultadoLote` | `Aceptado(Guid LoteId, Guid[] Aceptados, Guid[] Duplicados, IReadOnlyDictionary<Guid, MotivoDescarte> Rechazados)` · `Invalido` · `Saturado(TimeSpan ReintentarEn)` | patrón *Result* |
| `LoteRecibido` | `Guid LoteId` | elemento de la cola |
| `EventoPendiente` | `EventoReportado Evento`, `bool ConSesion`, `DateTimeOffset RecibidoUtc` | lo devuelve `RecibidosDelLoteAsync` (payload + columnas) |

## 5. Puertos

| Puerto | Operaciones |
|---|---|
| `IRecepcionEventos` | `Task<IReadOnlySet<Guid>> InsertarAsync(LoteAceptado lote, CancellationToken ct)` → ids insertados |
| `IColaLotes` | `bool HayCapacidad()` · `bool TryEscribir(LoteRecibido aviso)` · `IAsyncEnumerable<LoteRecibido> LeerAsync(CancellationToken ct)` |
| `IPendientesRepositorio` | `RecibidosDelLoteAsync(loteId)` (orden `detectado_utc, evento_id`) · `ContarEnLaHoraAsync(seudonimo, inicioHora)` · `MarcarAsync(eventoId, estado, motivo?)` · `LotesPendientesAsync(anteriorA?, maximo)` · `MasAntiguoPendienteAsync()` |
| `IPurgasRecepcion` | `PurgarSeudonimosAsync(antesDe)` · `BorrarFinalesAsync(antesDe)`; ambos por lotes de 5 000, devuelven filas afectadas |

## 6. `IngestaOptions` (sección `Ingesta`)

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
