# ML (spec 009): pruebas, cobertura, revisión de calidad y corrección del pipeline

> Medido sobre el código el 2026-11-23, no declarado. Python 3.11.14, dependencias del `requirements.lock`.
> [Volver a evidencias](../../README.md) · [Spec 009](../../../../../specs/009-dataset-y-modelo/pruebas.md) ·
> [Revisión de app, backend y panel](../2026-11-23-calidad/README.md)

## 1. Resultado final

| Medida | Resultado | Archivo |
|---|---|---|
| Pruebas | **174 aprobadas, 0 fallidas, 0 omitidas** en 28 archivos | [resultados.xml](resultados.xml) (JUnit) · [salida-pytest.txt](salida-pytest.txt) |
| Por carpeta | unitarias 120 · integración 49 · e2e 3 · arquitectura 2 | |
| Cobertura de `killauru_ml` (líneas + ramas) | **97 %** (1 057 instrucciones, 176 ramas); el CI exige 90 % | [cobertura.xml](cobertura.xml) |
| Reproducibilidad (SC-003) | dos procesos aislados dan el mismo `.tflite` byte a byte | [salida-reproducibilidad.txt](salida-reproducibilidad.txt) |
| ruff | sin hallazgos | [salida-ruff.txt](salida-ruff.txt) |
| Avisos | 4, todos de TensorFlow al cuantizar («Statistics for quantized inputs…»): la entrada y la salida quedan en `float32` **a propósito** (R-014) | |

Repetir desde `KillaUru-main/ml`:

```powershell
.\.venv\Scripts\python.exe -m pytest --cov=killauru_ml --cov-branch --cov-report=term-missing:skip-covered
.\.venv\Scripts\python.exe -m ruff check .
.\.venv\Scripts\python.exe scripts\verificar_reproducibilidad.py
```

En Windows, si pytest da `PermissionError` en `pytest-of-<usuario>`, añade `--basetemp=<carpeta propia>`: es la
carpeta temporal del sistema, no el código.

## 2. Reglas comprobadas con herramientas

| Regla | Cómo | Antes → después |
|---|---|---|
| AAA en cada prueba | script `ast` que busca `Arrange` y `Assert` en cada `test_` | 1 sin AAA → **0** (153 funciones de prueba) |
| Funciones ≤ 20 líneas | **nueva prueba** [test_funciones_cortas.py](../../../../../ml/tests/arquitectura/test_funciones_cortas.py) | 11 → **0** |
| Archivos < 200 líneas | `test_limite_lineas.py` | 0 → 0 |
| Nombres claros | variables de una letra (salvo `i`, `j`, `x`, `y`, `t`, `X`) | 82 → **0** |
| Una sola fuente de verdad | catálogo de clases [catalogo.py](../../../../../ml/killauru_ml/comun/catalogo.py) | 4 copias → 1; también se quitó una verificación de métricas duplicada de la puerta |

## 3. Qué estaba mal y qué se hizo

Primero se midió y se reabrieron 7 tareas marcadas sin cumplir lo prometido. Al corregirlas aparecieron 5 más en la
misma situación y T036. Todas quedaron corregidas, salvo T036, que depende de la campaña.

| Tarea | Estaba | Ahora |
|---|---|---|
| T027 | el ventaneo recibía la velocidad pero no la usaba | `aptitud.py`: fuera de RN-03 (> 5 km/h sostenidos ≥ 10 s) o con hueco > 200 ms, la ventana no entra |
| T033 | reparto **al azar** con semilla | partición **declarada a mano** en `particion.yaml` (decisión del autor, FR-013) con su huella; FR-014 completo (≥ 200/100/400 por clase, ≥ 3 celulares y ambos sentidos en prueba) |
| T034 | solo comparaba recorridos | huella de cada ventana, pares (recorrido, celular) y `RegistroAjuste`: si la normalización, el prefiltro o la temperatura usaron una ventana de prueba, falla |
| T038, T044 | 20 características **distintas** de las del data-model §4 | las 20 del contrato `p1`, con las definiciones ambiguas fijadas por escrito |
| T039, T044 | prefiltro por varianza | prefiltro por pico a pico vertical (`v_pap`), umbral que pierde ≤ 1 % de las anomalías |
| T042, T045 | `Sequential` sin normalización y con **una** salida | normalización dentro → 32 → 16 → `tipo_logits` y `severidad`; pérdida con pesos de clase y severidad solo en anomalías |
| T050 | sin comprobar salidas ni int8 | firma TFLite con nombres, formas y tipos del contrato; tensores int8; decide igual que Keras |
| T060, T064 | la puerta revisaba 3 de 7 condiciones; `puerta` devolvía siempre 2 | 7 condiciones; sin evidencia, falla; `puerta --version X` devuelve 0/1 y solo si aprueba escribe el manifiesto |
| T061 | manifiesto con 4 de 21 campos | cumple `manifiesto-modelo.schema.json`; SHA-256 y tamaño salen del archivo |
| T041 | faltaba la severidad ± 1 de R-012 | solo sobre anomalías detectadas con el tipo correcto (79 % / 80 %) |
| T036 | marcada como hecha | **reabierta**: `particion.yaml` estaba en borrador y sin recorridos; se congela tras la campaña |

Defectos encontrados en la revisión de calidad (antes de lo anterior): κ de Cohen salía `nan` con una sola clase y
**rechazaba un acuerdo perfecto**; `tamano.py` no tenía ninguna prueba.

## 4. Consecuencia para la app (spec 008)

La app pasa muestras crudas al modelo y espera una salida aplanada tipo × severidad; el contrato pide las 20
características `p1` y dos salidas. Se registró **008 T130** con lo que debe cambiar. Hasta entonces, un modelo
publicado por ML no funcionaría en la app. También se corrigió una numeración: la tarea del sombreado del tramo demo
se había numerado T128, número que ya tenía otra tarea; ahora es T129.

## 5. TDD, contado con honestidad

| Cómo se escribió | Pruebas |
|---|---|
| **Rojo visto antes del código** | κ con una clase; catálogo único; funciones cortas; ventaneo (4 fallaron: RN-03 y huecos); partición y FR-014 (`ModuleNotFoundError`); no-fuga (import); características; modelo y entrenamiento; puerta; etapa puerta y severidad ± 1 |
| **Red de seguridad antes de refactorizar** | `test_ingerir.py` y `test_cli.py` pasaron con el código viejo; luego se refactorizó |
| **Prueba y código en el mismo paso** (sin rojo aparte) | prefiltro; manifiesto |
| **Caracterización** (el código ya hacía lo pedido) | 4 de 5 pruebas de `test_cuantizar.py` pasaron con el convertidor existente; solo `inferir_tflite` cambió |

Los refactors puros se compararon contra la versión anterior: `ventilar` 900 casos idénticos, generador de
grabaciones 12 carpetas iguales byte a byte.

## 6. Diseño

- **Funciones puras por condición** en la puerta (`_rnf35`, `_rnf04`…): añadir una condición es una función más.
- **Evidencia separada de la decisión:** `evidencia_puerta.py` guarda lo medido; `puerta.py` decide;
  `etapa_puerta.py` (lo comparten la CLI y `todo`) escribe el manifiesto solo si aprueba.
- **Tabla de comandos** en la CLI y **estrategia** por patrón en el generador sintético.
- **Objeto `Conjunto`** en lugar de seis arreglos sueltos al entrenar.

## 7. Lo que estas pruebas no demuestran

Los datos son sintéticos: no prueban precisión real, latencia en celulares ni el acuerdo de etiquetadores reales.
Con datos sintéticos la puerta **rechaza** (sin benchmark, acuerdo, equivalencia ni partición), que es lo correcto.
Falta: campaña (T022, T032), congelar `ds-1` (T036), calibración y decisión (T040, T043, T046, T047), medir
equivalencia y vectores (T051, T052, T056), benchmark (T053–T059) y publicación (T065–T068). T004 sigue abierta hasta
enlazar una corrida verde de `ml-ci.yml` en GitHub. Para confirmar en una próxima revisión: la prueba de GPS → km
sobre la polilínea (T010).
