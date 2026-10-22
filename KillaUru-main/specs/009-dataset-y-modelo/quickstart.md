# Quickstart: comprobar el dataset y el modelo

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
pytest                                   # unitarias, no-fuga, límite de 100 líneas, determinismo, equivalencia
ruff check .
python -m killauru_ml todo --version 0.0.1 --sintetico     # pipeline completo con el dataset sintético
python -m killauru_ml todo --version 0.0.2 --sintetico     # misma semilla → mismas métricas y misma huella de pesos (SC-003)
```

## 2. Escenarios que deben pasar

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

## 3. Con datos reales (orden de la campaña)

```powershell
# Grabador: Android Studio → ml/grabador → instalar por cable en los 3 celulares (no tiene internet)
adb -s <celular> pull /sdcard/Android/data/pe.killauru.grabador/files/<grabacion> crudos/
python -m killauru_ml ingerir crudos/20261026-R03-C2          # verifica, catálogo, sube a R2 privado
python -m killauru_ml candidatos 20261026-R03-C2               # hoja para etiquetar con el video
python -m killauru_ml validar-etiquetas; python -m killauru_ml acuerdo
python -m killauru_ml estado                                   # controles de los días 7 y 11
python -m killauru_ml particion --verificar                    # luego congelar ds-1
python -m killauru_ml todo --version 1.0.0                     # usa la prueba una sola vez
# Benchmark: adb push modelos/1.0.0/killauru-1.0.0.tflite y vectores-prueba.json → pantalla Benchmark → adb pull del JSON
python -m killauru_ml puerta --version 1.0.0                   # código 0 → manifiesto.json
```

## 4. Publicar (manual)

GitHub → Actions → **publicar-modelo** → `version = 1.0.0` → Run. Comprobar:

```powershell
curl -s https://cdn.killauru.pe/modelo/manifiesto.json
curl -s https://cdn.killauru.pe/modelo/1.0.0/killauru-1.0.0.tflite -o m.tflite; certutil -hashfile m.tflite SHA256   # = sha256 del manifiesto
```

## 5. Atributos de calidad: cómo se mide cada uno

| Atributo | Medida | Dónde |
|---|---|---|
| RNF-35 (*precision*, *recall*, severidad ± 1) | Decisión del sistema con el `.tflite` sobre la prueba congelada | `informe.json`, `matriz-confusion.*` |
| RNF-04 (< 50 ms p95) | 1 000 inferencias por celular, 1 hilo, ≥ 3 de gama media | `benchmark/<alias>.json` |
| RNF-15 (batería) | Tasa de paso del prefiltro en Normal (insumo); la medición es de 008 en campo | `informe.json` → `diagnostico.tasaPasoPrefiltroNormal` |
| Reproducibilidad | Dos corridas iguales → mismas métricas y huella | `ml-ci.yml` (humo) y SC-003 |
| No-fuga | Recorridos y pares disjuntos; ajustes sin prueba | `test_no_fuga.py`, `particion --verificar` |
| Integridad de la publicación | SHA-256 desde `cdn.killauru.pe` = manifiesto | paso de verificación de `publicar-modelo.yml` |
| Privacidad | Sin lat/lon ni identificadores en el derivado; video ≤ 90 días | `test_ventaneo.py`, `test_grabacion.py`, `borrar_videos.sh` |
