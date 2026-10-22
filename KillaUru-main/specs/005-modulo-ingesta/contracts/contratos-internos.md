# Contratos internos del módulo Ingesta (`KillaUru.Ingesta/Contracts/`)

## 1. Lo que el módulo **ofrece**

Nada. `Contracts/` queda vacío a propósito: en el grafo de `componentes-arquitectonicos.md` (regla 4) ningún módulo depende de Ingesta. Lo único que aporta hacia afuera es su comprobación de salud, que se registra dentro de `AddIngestaModule` con el punto de extensión de 001:

```csharp
services.AddHealthChecks().AddCheck<ChequeoColaIngesta>("ingesta-cola", tags: ["ready"]);
```

## 2. Lo que el módulo **usa**

Solo **Application** usa los contratos de Anomalías; `Domain/` no conoce ningún tipo de otro módulo y `Adapters/` solo usa Application (D4).

| Contrato | De | Para qué |
|---|---|---|
| `IAnomaliasService.EvaluarIntrinseca(evento, recibidoUtc)` | Anomalías (004) | Reglas de recepción, por evento, antes del 200 |
| `IAnomaliasService.ProcesarReporteAsync(evento, contexto, ct)` | Anomalías (004) | Validar → agrupar → confirmar → clasificar, en la transacción de Anomalías |
| `EventoReportado`, `ContextoReporte`, `ResultadoProceso`, `MotivoDescarte` (incluido `SeudonimoInconsistente = 8`, D2) | Anomalías (004) | Tipos del intercambio; Ingesta usa el código 8 para la regla de R-010 |
| `IMetricas.RegistrarLote`, `RegistrarRechazo`, `ObservarCola`, `RegistrarRetrasoCola`, `RegistrarTareaCorrecta` | Shared (001) | RNF-22, RNF-23 (D11) |
| `ProgramaReloj` (`Shared/Tareas`) | Shared (001) | Próximo instante de las tareas por reloj (T-OPS05) |
| `Roles` y el *claim* `verificado` del token | Shared / host (001, 003) | `con_sesion` (D8) y el 403 al administrador |
| `TimeProvider` | Host (001) | Hora de recepción, `loteId` UUIDv7, barredor y purgas |

## 3. Cómo se arma `ContextoReporte` (004, R-001)

| Campo | De dónde sale |
|---|---|
| `ConSesion` | `evento_recibido.con_sesion` (guardado al recibir: `ConductorRegistrado` y `verificado = true`, RN-14, D8) |
| `EventosEnLaHora` | `IPendientesRepositorio.ContarEnLaHoraAsync(seudonimo, HoraDeDeteccion.Bloque(detectadoUtc))` (R-007) |
| `RecibidoUtc` | `evento_recibido.recibido_utc` |

## 4. Interpretación de `ResultadoProceso`

| Resultado | Fila en `evento_recibido` | Métrica |
|---|---|---|
| `Procesado` | `estado = 1` | `RegistrarRetrasoCola(ahora − recibido_utc)` |
| `YaProcesado` | `estado = 1` (otro intento ya lo procesó) | `RegistrarRetrasoCola(ahora − recibido_utc)` |
| `Descartado(motivo)` | `estado = 2`, `motivo = (smallint)motivo` | `RegistrarRetrasoCola(ahora − recibido_utc)` |
| excepción | sin cambios (`estado = 0`); el barredor reintenta | — |
