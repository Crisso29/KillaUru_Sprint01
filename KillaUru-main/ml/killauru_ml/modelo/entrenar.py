"""FR-016, FR-019, R-011 — entrenamiento del MLP con dos salidas, semilla fija y parada temprana.

Pérdida = entropía cruzada del tipo, con pesos de clase (inverso de la frecuencia: Normal es mayoría)
        + 0,5 × Huber de la severidad, **solo** en anomalías (las Normales no tienen severidad: máscara 0).
"""
from __future__ import annotations

import hashlib
from dataclasses import dataclass

import keras
import numpy as np

from killauru_ml.comun.catalogo import CLASES
from killauru_ml.modelo.arquitectura import CAPAS_OCULTAS, construir_modelo

_PESO_SEVERIDAD = 0.5        # R-011
_INDICE_NORMAL = CLASES.index("Normal")


@dataclass(frozen=True)
class Conjunto:
    """Un conjunto listo para entrenar: características (n, 20), índice de clase (n,) y severidad 1–5 (n,)."""
    X: np.ndarray
    tipo: np.ndarray
    severidad: np.ndarray

    def objetivos(self) -> dict[str, np.ndarray]:
        return {"tipo_logits": self.tipo, "severidad": np.asarray(self.severidad, dtype=np.float32).reshape(-1, 1)}


def pesos_de_muestra(conjunto: Conjunto) -> dict[str, np.ndarray]:
    """Peso por ventana para cada salida: clase rara pesa más (n / (clases · n_clase)); severidad 0 en Normal."""
    conteos = np.bincount(conjunto.tipo, minlength=len(CLASES)).astype(np.float64)
    peso_clase = len(conjunto.tipo) / (len(CLASES) * np.maximum(conteos, 1))
    mascara = (conjunto.tipo != _INDICE_NORMAL).astype(np.float32)
    return {"tipo_logits": peso_clase[conjunto.tipo].astype(np.float32), "severidad": mascara}


def entrenar(entrenamiento: Conjunto, validacion: Conjunto, epocas: int = 200, lote: int = 64,
             capas_ocultas: tuple[int, ...] = CAPAS_OCULTAS, paciencia: int = 15
             ) -> tuple[keras.Model, keras.callbacks.History]:
    """Entrena con Adam; la parada temprana mira la pérdida de validación y restaura los mejores pesos.

    Los valores por defecto son los de `config/entrenamiento.yaml` (Epocas, TamanioLote, PacienciaParadaTemprana).
    """
    modelo = _compilado(construir_modelo(entrenamiento.X, capas_ocultas))
    parada = keras.callbacks.EarlyStopping(monitor="val_loss", patience=paciencia, restore_best_weights=True)
    historia = modelo.fit(
        entrenamiento.X, entrenamiento.objetivos(), sample_weight=pesos_de_muestra(entrenamiento),
        validation_data=(validacion.X, validacion.objetivos(), pesos_de_muestra(validacion)),
        epochs=epocas, batch_size=lote, callbacks=[parada], verbose=0, shuffle=True,
    )
    return modelo, historia


def huella_pesos(modelo: keras.Model) -> str:
    """SHA-256 de todos los pesos en orden: dos entrenamientos iguales deben dar la misma (SC-003)."""
    sha = hashlib.sha256()
    for peso in modelo.get_weights():
        sha.update(np.ascontiguousarray(peso).tobytes())
    return sha.hexdigest()


def _compilado(modelo: keras.Model) -> keras.Model:
    modelo.compile(
        optimizer=keras.optimizers.Adam(learning_rate=0.001),
        loss={"tipo_logits": keras.losses.SparseCategoricalCrossentropy(from_logits=True),
              "severidad": keras.losses.Huber()},
        loss_weights={"tipo_logits": 1.0, "severidad": _PESO_SEVERIDAD},
    )
    return modelo
