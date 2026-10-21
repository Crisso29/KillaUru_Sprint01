# Contratos internos del módulo Ingesta (`KillaUru.Ingesta/Contracts/`)

## 1. Lo que el módulo **ofrece**

Nada. `Contracts/` queda vacío a propósito: en el grafo de `componentes-arquitectonicos.md` (regla 4) ningún módulo depende de Ingesta. Lo único que aporta hacia afuera es su comprobación de salud, que se registra dentro de `AddIngestaModule` con el punto de extensión de 001:

```csharp
services.AddHealthChecks().AddCheck<ChequeoColaIngesta>("ingesta-cola", tags: ["ready"]);
```

## 2. Lo que el módulo **usa**

| Contrato | De | Para qué |
|---|---|---|
| `IAnomaliasService.EvaluarIntrinseca(evento, recibidoUtc)` | Anomalías (004) | Reglas de recepción, por evento, antes del 200 |
| `IAnomaliasService.ProcesarReporteAsync(evento, contexto, ct)` | Anomalías (004) | Validar → agrupar → confirmar → clasificar, en la transacción de Anomalías |
| `EventoReportado`, `ContextoReporte`, `ResultadoProceso`, `MotivoDescarte` | Anomalías (004) | Tipos del intercambio |
| `IMetricas.RegistrarLote`, `RegistrarRechazo`, `ObservarCola`, `RegistrarTareaCorrecta` | Shared (001) | RNF-22 |
| `Roles` | Shared (001) | `con_sesion` y el 403 al administrador |
| `TimeProvider` | Host (001) | Hora de recepción, barredor y purgas |

## 3. Cómo se arma `ContextoReporte` (004, R-001)

| Campo | De dónde sale |
|---|---|
| `ConSesion` | `evento_recibido.con_sesion` (guardado al recibir según el token, RN-14) |
| `EventosEnLaHora` | `IPendientesRepositorio.ContarEnLaHoraAsync(seudonimo, HoraDeDeteccion.Bloque(detectadoUtc))` (R-007) |
| `RecibidoUtc` | `evento_recibido.recibido_utc` |

## 4. Interpretación de `ResultadoProceso`

| Resultado | Fila en `evento_recibido` |
|---|---|
| `Procesado` | `estado = 1` |
| `YaProcesado` | `estado = 1` (otro intento ya lo procesó) |
| `Descartado(motivo)` | `estado = 2`, `motivo = (smallint)motivo` |
| excepción | sin cambios (`estado = 0`); el barredor reintenta |
