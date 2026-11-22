# Ingesta — spec 005

Hay implementación de recepción, persistencia, procesamiento y purgas. Falta acreditar
la integración dedicada y las condiciones de recuperación/carga.
[Estado de cierre](../../../../specs/005-modulo-ingesta/cierre.md) ·
[Contrato](../../../../specs/005-modulo-ingesta/contracts/ingesta.openapi.yaml)

## Flujo y responsabilidades

1. `SyncController` recibe el lote y determina si existe sesión verificada.
2. `RecibirLoteEventosUseCase` valida tamaño/capacidad y evalúa eventos con Anomalías.
3. `IRecepcionEventos` persiste antes de notificar la cola; PostgreSQL es la fuente durable.
4. Trabajadores consumen la cola; un barredor recupera pendientes.
5. El resultado distingue aceptados, duplicados y rechazados.

Se usan puertos de Application, adaptadores PostgreSQL, cola en memoria y reloj inyectado.
Los descartes de ingesta son métricas; no son acciones administrativas auditadas.
No existe un endpoint de consulta de estado por lote.

## API implementada

| Método y ruta | Acceso | Resultado |
|---|---|---|
| `POST /api/v1/anomalies/batch` | Anónimo o conductor; administrador recibe 403 | Respuesta de recepción por evento |

El cuerpo contiene `eventos`, no un `loteId` aportado por el cliente.
El servidor genera el identificador del lote de recepción.
El rechazo incluye `id`, `indice` y `motivo`; la consistencia de seudónimo se
verifica mediante `SeudonimoDelLote`. Ver el YAML para cada campo y condición.
El 200 de recepción no significa que una anomalía ya esté confirmada en el mapa.

Para reproducir, preparar un archivo de lote sintético según el contrato:

```powershell
curl.exe -X POST "$env:KU_URL_PRUEBAS/api/v1/anomalies/batch" -H "Content-Type: application/json" -H "X-KillaUru-Seudonimo: $env:KU_SEUDONIMO_PRUEBA" --data-binary "@lote-prueba.json"
```

`Adapters/Documentacion/EsquemasIngesta.cs` completa el OpenAPI publicado (`/openapi/v1.json`, `/docs`)
con requeridos, límites y catálogos del YAML. `ContratoIngestaTests` compara ambos (T034, aprobado).
El cuerpo no se lee con `[FromBody]`: `Adapters/LectorCuerpoLote.cs` lo lee con el tope `TamanoMaximoCuerpoKb`.
Más que el tope es 413 `lote_demasiado_grande`, también si Kestrel corta antes, y un cuerpo que no es
`{ "eventos": [...] }` es 400 `lote_invalido`. Todos los errores son ProblemDetails (`application/problem+json`)
con `codigo` y `correlacionId`. `Adapters/Documentacion/OperacionIngesta.cs` declara el cuerpo `Lote` en el OpenAPI.

## Parámetros y mantenimiento

Valores declarados en `Application/IngestaOptions.cs`, configurables desde `Ingesta`:

| Parámetro | Valor por defecto |
|---|---:|
| MaxEventosPorLote | 50 |
| CapacidadCola | 2000 |
| Trabajadores | 0: número de núcleos |
| ReintentarEnSegundos | 30 |
| IntervaloBarredorSegundos / AntiguedadMinimaBarredorSegundos | 60 / 60 |
| AntiguedadMaximaColaMinutos | 5 |
| PurgaSeudonimoHoras / RetencionRecepcionDias | 72 / 30 |
| TamanoMaximoCuerpoKb | 256 |

La purga de seudónimos corre al arrancar y cada hora en el minuto 10.
La purga de recepción corre al arrancar y a las 03:00 de Lima.
Confirmar valores efectivos de configuración y límite HTTP al validar el despliegue.

## Cómo se prueba

Desde `backend/`:

```powershell
dotnet test --project tests/KillaUru.Ingesta.Tests
dotnet test --project tests/KillaUru.Arquitectura.Tests
dotnet test --project tests/KillaUru.Integracion.Tests -- --filter-class "*ContratoIngestaTests"
dotnet test --project tests/KillaUru.Integracion.Tests -- --filter-namespace "KillaUru.Integracion.Tests.Ingesta"
```

[Guía de pruebas](../../../../specs/005-modulo-ingesta/pruebas.md).
La recepción HTTP con PostgreSQL real (T012) está aprobada: 17/17 escenarios. Faltan reinicio, purga y salud.
Para cerrar: conciliar eventos aceptados contra recepción y procesamiento, demostrar
cero duplicados tras reintento/reinicio y ejecutar carga con datos sintéticos.
