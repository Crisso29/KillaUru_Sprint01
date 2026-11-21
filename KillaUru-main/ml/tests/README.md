# Pruebas del dataset y modelo — spec 009

| Carpeta | Qué comprobar |
|---|---|
| `unitarias` | Señal, ventanas, etiquetas, particiones, métricas y reglas |
| `integracion` | Archivos, configuración, entrenamiento y conversión |
| `e2e` | Pipeline completo con datos sintéticos |
| `arquitectura` | Límite de líneas de todas las fuentes ML |

`sinteticos.py` prepara grabaciones artificiales compartidas.
La prueba del modelo en `unitarias` usa TensorFlow: unitario no implica ausencia de librerías.
Ninguna de estas carpetas sustituye la campaña de campo ni el benchmark en celulares.

Desde `ml/`, con Python 3.11 y las dependencias fijadas instaladas:

```powershell
python -m pytest tests/unitarias
python -m pytest tests/integracion
python -m pytest tests/e2e
python -m pytest tests/arquitectura
python -m pytest
```

Consulta [la spec](../../specs/009-dataset-y-modelo/pruebas.md) y la [guía común](../../docs/guias/pruebas.md).

## Clasificación revisada el 21 de noviembre de 2026

`unitarias/test_no_fuga.py` comprueba el verificador con particiones preparadas.
`integracion/test_particion_sin_fuga.py` ejecuta el particionador y después el
verificador: pertenece a integración porque comprueba su colaboración.
Ambas suites usan datos en memoria; integración no implica necesariamente red o BD.

