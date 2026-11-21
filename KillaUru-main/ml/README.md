# Dataset y modelo — spec 009

Pipeline Python y grabador Android para construir y validar el modelo de detección.
Hay funciones y pruebas implementadas, pero el flujo real y la publicación no están cerrados.
[Pendientes de cierre](../specs/009-dataset-y-modelo/cierre.md) · [Pruebas](tests/README.md)

## Entorno y datos

Python **3.11** según `pyproject.toml`; instalar `requirements.lock` con hashes.
Trabajar desde `KillaUru-main/ml/`:

```powershell
python -m venv .venv
.\.venv\Scripts\python.exe -m pip install --require-hashes -r requirements.lock
.\.venv\Scripts\python.exe -m pip install --no-deps -e .
.\.venv\Scripts\python.exe -m pytest
```

| Carpeta | Contenido |
|---|---|
| `grabador/` | App para captura y futuro benchmark |
| `protocolo/` | Captura y etiquetado de campo |
| `datos/` | Catálogo, partición y datos de campaña; usar controles de privacidad |
| `config/` | Umbrales y configuración de etapas |
| `killauru_ml/` | Funciones del pipeline y CLI |
| `modelos/` | Artefactos por versión; sintético no equivale a aprobado |
| `tests/` | Unitarias, integración, E2E sintético y arquitectura |

## CLI disponible y límites actuales

| Comando | Comportamiento observado en el código |
|---|---|
| `python -m killauru_ml ingerir <carpeta>` | Valida/ingresa grabación; subida solo si se solicita `--subir` |
| `python -m killauru_ml validar-etiquetas <archivo.json>` | Valida etiquetas del archivo |
| `python -m killauru_ml estado` | Resume catálogo y partición |
| `python -m killauru_ml particion` | Lee versión/estado; no ejecuta toda la validación de separación |
| `python -m killauru_ml todo --version 0.0.1-test --sintetico` | Ejecuta pipeline sintético; no aprueba modelo de producción |
| `python -m killauru_ml todo --version 1.0.0` | Devuelve 2: flujo real pendiente |
| `python -m killauru_ml puerta` | Devuelve 2: cargador de configuración, puerta CLI pendiente |
| candidatos, acuerdo, ventanas, entrenar, evaluar, cuantizar | Subcomandos declarados; despacho CLI pendiente, aunque existan funciones |

Consultar `python -m killauru_ml --help` para la sintaxis real.
El mensaje de `puerta` sugiere opciones que el parser aún no implementa: no usarlas como receta.
La validación de funciones con pytest no demuestra que el CLI completo esté integrado.

## Flujo para llegar a una versión publicable

Capturar → validar grabaciones → etiquetar y medir acuerdo → separar recorridos →
entrenar/calibrar → evaluar sobre prueba congelada → cuantizar y verificar equivalencia →
medir en tres celulares → aprobar la puerta → publicar versión inmutable y manifiesto.

Los pendientes exactos están en la spec. El workflow `publicar-modelo.yml` existe,
pero su paso `puerta` impide actualmente publicar. No saltar esa comprobación.
Faltan el script/flujo final de publicación, rollback verificado, campaña y modelo aprobado para 008.

## Privacidad y operación

No incorporar IMEI, placa, nombres ni vínculos cuenta-evento a los datos.
Las pruebas de grabación rechazan metadatos prohibidos; revisar también exportaciones y permisos de R2.
Aplicar el protocolo de retención de video; el borrado automatizado sigue pendiente.
No guardar credenciales en el catálogo, informes o archivos versionados.

## Registro de simulacro de publicación

| Versión / commit | Entorno | Publicación | Vuelta atrás | Integridad | Evidencia |
|---|---|---|---|---|---|
| Pendiente | Prefijo de pruebas | Sin ejecutar | Sin ejecutar | Sin verificar | T067 |

Este registro vacío no acredita T067 ni la publicación de 1.0.0.
