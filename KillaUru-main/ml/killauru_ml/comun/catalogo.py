"""RN-01, R-017 — catálogo único de clases del modelo.

El orden es el del tensor de salida: la app lee las probabilidades en este orden (manifiesto `etiquetas`). Cambiarlo
obliga a subir la versión mayor del modelo (ADR-004). `test_catalogo.py` lo compara con el APK y con el contrato.
"""

CLASES: tuple[str, ...] = ("Normal", "Bache", "Hundimiento")
