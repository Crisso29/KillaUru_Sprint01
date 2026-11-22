# Consulta de mapa — spec 006

El módulo implementa consulta GeoJSON, selección por rol, filtros, cambios y caché.
Su integración dedicada, geometría demo y validación de borde siguen pendientes.
[Estado de cierre](../../../../specs/006-modulo-consulta-mapa/cierre.md) ·
[Contrato](../../../../specs/006-modulo-consulta-mapa/contracts/mapa.openapi.yaml)

## Responsabilidades y diseño

Application coordina consultas por celdas, filtros o cambios.
Infrastructure adapta los contratos de Anomalías y la caché; este módulo no posee tablas.
Los eventos de cambios invalidan las celdas correspondientes.
El controlador `AnomaliasController` publica `GET /api/v1/anomalies`;
no publica `/mapa/anomalias` ni `/mapa/resumen-dia`.

| Rol | Alcance previsto por contrato |
|---|---|
| Anónimo | Recorte al polígono demo; respuesta pública solo cuando corresponde |
| Conductor | Corredor completo dentro de las condiciones de su sesión |
| Administrador | Consulta administrativa con filtros de estado |

La API decide el alcance. Los tests HTTP de recorte y filtros están pendientes;
la tabla describe el contrato que deben acreditar.

## Caché, ETag y sincronización

- Ruta sin filtros: versiones por celda y alcance componen el ETag.
- `If-None-Match` permite 304 cuando no hay cambios.
- Consultas filtradas y `updatedSince` requieren comprobar las reglas del YAML.
- Respuestas autenticadas: `private, no-store`; el borde solo puede cachear respuestas anónimas.
- `updatedSince` permite sincronizar cambios y bajas. No mezclarlo con parámetros incompatibles.
- La respuesta no expone seudónimos ni relaciones con cuentas.

`MapaOptions` declara `PoligonoDemo`, `VigenciaCacheMinutos` (10),
`MaxAgeBordeSegundos` (60) y `TamanoCacheMb` (64).
La presencia de configuración no demuestra que el polígono ni el límite efectivo de memoria estén validados.

Ejemplos contra el entorno de pruebas:

```powershell
curl.exe "$env:KU_URL_PRUEBAS/api/v1/anomalies"
curl.exe "$env:KU_URL_PRUEBAS/api/v1/anomalies?estado=Activa" -H "Authorization: Bearer $env:KU_TOKEN_ADMIN"
```

`severidad` y `estado` son listas separadas por comas (`?severidad=4,5`); `Adapters/Dtos/ParametrosMapaDto.cs`
las convierte y una lista mal formada responde 400 `datos_invalidos`. Hasta el 2026-11-21 solo se aceptaba
la forma repetida (`?severidad=4&severidad=5`), y el panel recibía 400.
`Adapters/Documentacion/` publica los esquemas GeoJSON en `/openapi/v1.json` y `/docs`, porque la respuesta
son bytes preserializados por la caché.

Para `bbox`, fechas y cambios, usar ejemplos del contrato y el [quickstart](../../../../specs/006-modulo-consulta-mapa/quickstart.md).
No copiar el ETag entre usuarios o alcances diferentes.

## Cómo se prueba

Desde `backend/`:

```powershell
dotnet test --project tests/KillaUru.Mapa.Tests
dotnet test --project tests/KillaUru.Arquitectura.Tests
dotnet test --project tests/KillaUru.Integracion.Tests -- --filter-class "*ContratoMapaTests"
```

[Guía de pruebas](../../../../specs/006-modulo-consulta-mapa/pruebas.md).
El contrato OpenAPI está aprobado (T029). Faltan casos dedicados de ETag, serialización, métricas,
recorte, filtros y cambios.
La carga se comparte con Ingesta en `tests/carga/`; el p95 debe medirse en el entorno de pruebas.
