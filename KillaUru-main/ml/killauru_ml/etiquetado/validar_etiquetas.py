"""FR-009, FR-010 — validación de etiquetas: tipos, severidad y subtipos."""
from __future__ import annotations

from killauru_ml.comun.catalogo import CLASES

_TIPOS_VALIDOS = set(CLASES)
_SUBTIPOS_NORMAL = {"Rompemuelles", "Junta", "Baden", "Riel", "Ripio", "CambioPavimento", ""}
_FUENTES_VALIDAS = {"video", "patron", "sensor"}
_SEVERIDAD_MIN = 1
_SEVERIDAD_MAX = 5


class EtiquetaError(ValueError):
    pass


def validar_etiqueta(etiqueta: dict) -> None:
    """Valida una etiqueta; lanza EtiquetaError si hay inconsistencias."""
    tipo = etiqueta.get("tipo")
    # tipo debe ser uno de los válidos (no subtipos como Rompemuelles)
    if tipo not in _TIPOS_VALIDOS:
        raise EtiquetaError(f"tipo '{tipo}' no válido; los tipos son {sorted(_TIPOS_VALIDOS)}")
    if tipo == "Normal":
        _validar_normal(etiqueta.get("severidad"), etiqueta.get("subtipo", ""))
    else:
        _validar_anomalia(tipo, etiqueta.get("severidad"), etiqueta.get("subtipo", ""))
    fuente = etiqueta.get("fuente", "video")
    if fuente not in _FUENTES_VALIDAS:
        raise EtiquetaError(f"fuente '{fuente}' no válida; use {sorted(_FUENTES_VALIDAS)}")


def _validar_normal(severidad: int | None, subtipo: str | None) -> None:
    """Normal no lleva severidad; su subtipo (rompemuelles, junta…) explica por qué la vía «salta» sin ser bache."""
    if severidad is not None:
        raise EtiquetaError("severidad debe ser None para tipo Normal")
    if (subtipo or "") not in _SUBTIPOS_NORMAL:
        raise EtiquetaError(f"subtipo '{subtipo or ''}' no válido para Normal")


def _validar_anomalia(tipo: str, severidad: int | None, subtipo: str | None) -> None:
    """Bache y Hundimiento llevan severidad de 1 a 5 y ningún subtipo."""
    if severidad is None:
        raise EtiquetaError(f"tipo '{tipo}' requiere severidad (1-{_SEVERIDAD_MAX})")
    if not (_SEVERIDAD_MIN <= severidad <= _SEVERIDAD_MAX):
        raise EtiquetaError(f"severidad {severidad} fuera de rango [{_SEVERIDAD_MIN}, {_SEVERIDAD_MAX}]")
    if subtipo:
        raise EtiquetaError(f"subtipo no permitido para tipo '{tipo}'; use solo para Normal")
