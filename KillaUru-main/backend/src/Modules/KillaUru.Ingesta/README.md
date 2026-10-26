# Ingesta

> **Estado**: pendiente de implementación — spec 005.
> El módulo está registrado en el host (`AddIngestaModule`) pero no tiene endpoints ni lógica de negocio todavía.

## Propósito

Recibe los lotes de reportes enviados por la app del conductor, los valida, los desagrega
y los entrega al módulo de Anomalías para su procesamiento.
Es el punto de entrada de todos los datos de campo al sistema; ningún otro módulo acepta
reportes directamente (RN-05, spec 005).

## Responsabilidades planeadas (spec 005)

- Validar la autenticidad del lote (firma del dispositivo, seudónimo válido).
- Aplicar el límite de tasa por seudónimo + IP antes de procesar (FR-010, FR-030).
- Desagregar el lote en reportes individuales y publicarlos como `EventoReportado` (contratos de Anomalías).
- Registrar la auditoría del envío si el lote supera el umbral de anomalías nuevas.
- Devolver al cliente un resumen del procesamiento (aceptados, descartados, motivo).

## Dependencias planeadas

- `KillaUru.Anomalias.Contracts` — `IAnomaliasService`, `EventoReportado`, `ContextoReporte`
- `KillaUru.Auditoria.Contracts` — `IAuditoriaService` (solo si el lote genera una acción de riesgo)
- `KillaUru.Shared` — `TimeProvider`, `IPublicadorEventos`, dominios transversales

## API planeada (prefijo `/api/v1`)

| Ruta | Método | Acceso | Propósito |
|---|---|---|---|
| `/lotes` | POST | ConductorRegistrado | Enviar un lote de reportes geoespaciales |
| `/lotes/{id}/estado` | GET | ConductorRegistrado | Consultar el resultado del procesamiento |

> Las rutas y el contrato definitivos se especifican en `specs/005-modulo-ingesta/`.

## Verificación desde `backend/`

```powershell
dotnet build KillaUru.sln
# Los tests se agregan al implementar el spec 005
```

## Bloqueante para el E2E

El test de punta a punta (`PuntaAPunta/FlujoPuntaAPuntaTests.cs`) está marcado con `Skip`
hasta que este módulo esté implementado junto con Mapa (spec 006).
