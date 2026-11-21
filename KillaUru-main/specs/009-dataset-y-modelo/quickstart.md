# Quickstart: comprobar el dataset y el modelo

> Vigencia documental (2026-11-15): [estado y pendientes de cierre](cierre.md). Los requisitos y planes no equivalen a evidencia de implementación o despliegue.

## Requisitos previos

Python 3.11; `rclone` configurado con el *bucket* privado `killauru-datos` (solo para datos reales); Android Studio y `adb` para el grabador; 3 celulares de gama media; `backend/db/datos/corredor_pe28a.geojson` (004, D16).

```powershell
cd ml
python -m venv .venv; .venv\Scripts\Activate.ps1
pip install --require-hashes -r requirements.lock
pip install -e . --no-deps
```

## 1. Pruebas automáticas (sin datos reales)

```powershell
python -m pytest                         # solo los casos implementados; equivalencia completa pendiente
ruff check .
python -m killauru_ml todo --version 0.0.1 --sintetico     # pipeline completo con el dataset sintético
python -m killauru_ml todo --version 0.0.2 --sintetico     # misma semilla → mismas métricas y misma huella de pesos (SC-003)
```

## 2. Criterios de aceptación (incluye escenarios todavía pendientes)

| # | Escenario | Esperado |
|---|---|---|
| 1 | Grabación sintética a 44,9 Hz / 45,0 Hz | `NoApta(FrecuenciaBaja)` / `Apta` |
| 2 | `meta.json` con `categoria: Pesado` o con `imei` | `ingerir` la rechaza (RN-18, FR-005) |
| 3 | Etiqueta Normal con severidad 3 | `validar-etiquetas` falla con la fila |
| 4 | Pico a 249 ms del borde de la ventana vecina | la vecina es zona gris y se excluye |
| 5 | `particion.yaml` con `R03` en entrenamiento y prueba | `particion` falla (FR-013) |
| 6 | Prueba con 199 ventanas de Hundimiento | `particion` falla antes de entrenar (FR-014) |
| 7 | Normalización ajustada con datos de prueba | `test_no_fuga` falla |
| 8 | Ventana con confidence 0.74 / 0.75 | Normal / anomalía (RN-04) |
| 9 | Informe con *recall* de Bache 0,899 | `puerta` termina con código 1; no hay `manifiesto.json` |
| 10 | *Benchmark* con p95 de 50,0 ms en un celular | `puerta` falla (RNF-04 es «< 50 ms») |
| 11 | Solo 2 celulares de gama media medidos | `puerta` falla |
| 12 | `.tflite` con un peso alterado | `equivalencia` falla |
| 13 | Manifiesto generado | valida contra `contracts/manifiesto-modelo.schema.json`; SHA-256 = archivo; sin `ConfidenceMinimo` |
| 14 | Publicar dos veces la misma versión | `publicar_r2.sh` falla sin tocar R2 |
| 15 | Volver atrás a la versión anterior | el manifiesto vuelve; los archivos de ambas versiones siguen iguales |

## 3. Con datos reales: preparación y límites actuales

El [README de ML](../../ml/README.md) enumera lo que despacha el CLI actual.
Se puede ingerir una grabación y validar un archivo de etiquetas con:

```powershell
python -m killauru_ml ingerir crudos/mi-grabacion
python -m killauru_ml validar-etiquetas datos/etiquetas/mi-grabacion.json
python -m killauru_ml estado
python -m killauru_ml particion
```

Usar archivos reales existentes y mantenerlos fuera de informes públicos.
`ingerir` solo solicita subida al añadir `--subir`; no sube automáticamente.
`particion` solo informa versión/estado: todavía no valida todo el contrato del dataset.
`candidatos`, `acuerdo`, `ventanas`, `entrenar`, `evaluar` y `cuantizar` están declarados,
pero su despacho CLI no está implementado. `todo` sin `--sintetico` devuelve 2.
Las opciones `particion --verificar` y `puerta --version` no están implementadas.
No ejecutar esas recetas de la planificación inicial como si estuvieran completas.

Completar las tareas de [cierre](cierre.md), después ejecutar la campaña, etiquetado,
partición congelada, entrenamiento/evaluación, equivalencia y benchmark.

## 4. Publicación: bloqueada hasta completar la puerta

El workflow manual `publicar-modelo.yml` existe, pero `python -m killauru_ml puerta`
carga configuración y devuelve 2. Falta integrar la validación de modelo/métricas;
no ignorar el código de salida ni sustituirla por la ejecución sintética.
`ml/scripts/publicar_r2.sh` tampoco existe todavía.

T065–T068 exigen publicación inmutable, manifiesto al final, comprobación de SHA-256
por HTTP y simulacro de vuelta atrás en prefijo de pruebas. Cuando estén implementados,
registrar resultados antes de publicar el modelo aprobado para 008.

## 5. Atributos de calidad: cómo se mide cada uno

| Atributo | Medida | Dónde |
|---|---|---|
| RNF-35 (*precision*, *recall*, severidad ± 1) | Decisión del sistema con el `.tflite` sobre la prueba congelada | `informe.json`, `matriz-confusion.*` |
| RNF-04 (< 50 ms p95) | 1 000 inferencias por celular, 1 hilo, ≥ 3 de gama media | `benchmark/<alias>.json` |
| RNF-15 (batería) | Tasa de paso del prefiltro en Normal (insumo); la medición es de 008 en campo | `informe.json` → `diagnostico.tasaPasoPrefiltroNormal` |
| Reproducibilidad | Dos corridas iguales → mismas métricas y huella | `ml-ci.yml` (humo) y SC-003 |
| No-fuga | Recorridos y pares disjuntos; ajustes sin prueba | `tests/unitarias/test_no_fuga.py`; integración de validación CLI pendiente |
| Integridad de la publicación | SHA-256 desde `cdn.killauru.pe` = manifiesto | T065–T068: comparación HTTP de SHA-256 pendiente; listar objetos R2 no basta |
| Privacidad | Sin lat/lon ni identificadores en el derivado; video ≤ 90 días | `test_ventaneo.py`, `test_grabacion.py`, `borrar_videos.sh` |
