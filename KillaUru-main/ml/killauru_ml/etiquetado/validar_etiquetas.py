"""FR-009, FR-010 — validación de etiquetas: tipos, severidad y subtipos."""
from __future__ import annotations

_TIPOS_VALIDOS = {"Bache", "Hundimiento", "Normal"}
_SUBTIPOS_NORMAL = {"Rompemuelles", "Junta", "Baden", "Riel", "Ripio", "CambioPavimento", ""}
_FUENTES_VALIDAS = {"video", "patron", "sensor"}
_SEVERIDAD_MIN = 1
_SEVERIDAD_MAX = 5


class EtiquetaError(ValueError):
    pass


def validar_etiqueta(etiqueta: dict) -> None:
    """Valida una etiqueta; lanza EtiquetaError si hay inconsistencias."""
    tipo = etiqueta.get("tipo")
    severidad = etiqueta.get("severidad")
    subtipo = etiqueta.get("subtipo", "")
    fuente = etiqueta.get("fuente", "video")

    # tipo debe ser uno de los válidos (no subtipos como Rompemuelles)
    if tipo not in _TIPOS_VALIDOS:
        raise EtiquetaError(
            f"tipo '{tipo}' no válido; los tipos son {sorted(_TIPOS_VALIDOS)}"
        )

    # Normal no lleva severidad; Bache/Hundimiento sí
    if tipo == "Normal":
        if severidad is not None:
            raise EtiquetaError("severidad debe ser None para tipo Normal")
        subtipo_val = subtipo or ""
        if subtipo_val not in _SUBTIPOS_NORMAL:
            raise EtiquetaError(
                f"subtipo '{subtipo_val}' no válido para Normal"
            )
    else:
        # Bache / Hundimiento
        if severidad is None:
            raise EtiquetaError(
                f"tipo '{tipo}' requiere severidad (1-{_SEVERIDAD_MAX})"
            )
        if not (_SEVERIDAD_MIN <= severidad <= _SEVERIDAD_MAX):
            raise EtiquetaError(
                f"severidad {severidad} fuera de rango "
                f"[{_SEVERIDAD_MIN}, {_SEVERIDAD_MAX}]"
            )
        if subtipo:
            raise EtiquetaError(
                f"subtipo no permitido para tipo '{tipo}'; use solo para Normal"
            )

    # fuente
    if fuente not in _FUENTES_VALIDAS:
        raise EtiquetaError(
            f"fuente '{fuente}' no válida; use {sorted(_FUENTES_VALIDAS)}"
        )
