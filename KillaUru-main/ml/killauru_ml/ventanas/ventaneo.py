"""FR-005, FR-012 — ventaneo: corte de señal en ventanas de 25 muestras.

Solo se cortan ventanas aptas (RN-03 y sin huecos, ver `aptitud.py`); las demás no entran al dataset.
"""
from __future__ import annotations

from dataclasses import dataclass

import numpy as np

from killauru_ml.ventanas.aptitud import a_kmh, es_apta, inicio_de_racha

_MUESTRAS_VENTANA = 25
_ZONA_GRIS_NS = 250 * 1_000_000  # 250 ms estricto: < 250 ms es zona gris
_AUMENTO_NS = 200 * 1_000_000    # ±200 ms para aumento


@dataclass
class VentanaDataset:
    a_v: np.ndarray
    t_inicio_ns: int
    t_fin_ns: int
    tipo: str = "Normal"
    severidad: int | None = None
    zona_gris: bool = False
    aumentada: bool = False
    recorrido: str = ""   # asignado en partir(); NO lat/lon (FR-005)
    celular: str = ""     # alias (C1, C2…): FR-014 exige ≥ 3 en prueba
    sentido: str = ""     # AyacuchoVinchos | VinchosAyacucho: FR-014 exige ambos en prueba


def ventilar(
    t_ns: np.ndarray,
    a_v: np.ndarray,
    vel: np.ndarray,
    km: np.ndarray,
    etiquetas: list[dict],
    conjunto: str,
    aumentar: bool = False,
) -> list[VentanaDataset]:
    """Corta la señal en ventanas no solapadas de 25 muestras.

    - Descarta las ventanas fuera de RN-03 o con huecos (`vel` en m/s por muestra).
    - Asigna la etiqueta con mayor severidad a cada ventana.
    - Empate de severidad: Bache > Hundimiento > Normal (FR-012).
    - Ventana dentro de < 250 ms de un pico vecino = zona gris.
    - Aumento ±200 ms solo en conjunto='entrenamiento'.
    """
    ventanas = _cortar(np.asarray(t_ns, dtype=np.int64), np.asarray(a_v, dtype=np.float32), a_kmh(vel), etiquetas)
    if aumentar and conjunto == "entrenamiento":
        ventanas += _aumentar(ventanas, etiquetas)
    return ventanas


def _cortar(t_ns: np.ndarray, a_v: np.ndarray, kmh: np.ndarray, etiquetas: list[dict]) -> list[VentanaDataset]:
    """Ventanas aptas consecutivas de 25 muestras, cada una con su etiqueta ganadora y si cae en zona gris."""
    total = len(t_ns)
    racha = inicio_de_racha(t_ns, kmh)
    paso_ns = int(t_ns[1] - t_ns[0]) if total > 1 else 20_000_000  # muestreo uniforme (50 Hz)
    ventanas: list[VentanaDataset] = []
    for numero in range(total // _MUESTRAS_VENTANA):
        inicio = numero * _MUESTRAS_VENTANA
        fin = inicio + _MUESTRAS_VENTANA
        t_ini = int(t_ns[inicio])
        # t_fin = hora de la primera muestra de la ventana siguiente
        t_fin = int(t_ns[fin]) if fin < total else int(t_ns[fin - 1]) + paso_ns
        if not es_apta(t_ns, kmh, racha, slice(inicio, fin), t_fin):
            continue
        tipo, severidad = _etiqueta_ventana(t_ini, t_fin, etiquetas)
        ventanas.append(VentanaDataset(a_v=a_v[inicio:fin].copy(), t_inicio_ns=t_ini, t_fin_ns=t_fin, tipo=tipo,
                                       severidad=severidad, zona_gris=_es_zona_gris(t_ini, t_fin, etiquetas)))
    return ventanas


def _aumentar(ventanas: list[VentanaDataset], etiquetas: list[dict]) -> list[VentanaDataset]:
    """Solo para entrenamiento: copia la ventana más cercana a ±200 ms de cada pico, con la etiqueta de ese pico."""
    aumentadas = []
    for etiqueta in etiquetas:
        for desfase_ns in (-_AUMENTO_NS, +_AUMENTO_NS):
            base = _ventana_mas_cercana(ventanas, etiqueta["t_pico_ns"] + desfase_ns)
            if base is not None:
                aumentadas.append(VentanaDataset(
                    a_v=base.a_v.copy(), t_inicio_ns=base.t_inicio_ns, t_fin_ns=base.t_fin_ns,
                    tipo=etiqueta.get("tipo", "Normal"), severidad=etiqueta.get("severidad"),
                    zona_gris=base.zona_gris, aumentada=True,
                ))
    return aumentadas


# ---------------------------------------------------------------------------
# Helpers internos
# ---------------------------------------------------------------------------

_PRIORIDAD = {"Bache": 2, "Hundimiento": 1, "Normal": 0}


def _etiqueta_ventana(
    t_ini: int, t_fin: int, etiquetas: list[dict]
) -> tuple[str, int | None]:
    """Devuelve (tipo, severidad) ganadora para la ventana [t_ini, t_fin)."""
    candidatas = [
        etiqueta for etiqueta in etiquetas
        if t_ini <= etiqueta["t_pico_ns"] < t_fin
    ]
    if not candidatas:
        return "Normal", None

    def _clave(etiqueta: dict) -> tuple[int, int]:
        sev = etiqueta.get("severidad") or 0
        prio = _PRIORIDAD.get(etiqueta.get("tipo", "Normal"), 0)
        return (sev, prio)

    ganadora = max(candidatas, key=_clave)
    return ganadora.get("tipo", "Normal"), ganadora.get("severidad")


def _es_zona_gris(t_ini: int, t_fin: int, etiquetas: list[dict]) -> bool:
    """True si algún pico FUERA de esta ventana está a < 250 ms de sus bordes."""
    for etiqueta in etiquetas:
        t_pico = etiqueta["t_pico_ns"]
        # Pico dentro de la ventana no cuenta
        if t_ini <= t_pico < t_fin:
            continue
        # Distancia al borde más cercano
        distancia = min(abs(t_pico - t_ini), abs(t_pico - t_fin))
        if distancia < _ZONA_GRIS_NS:
            return True
    return False


def _ventana_mas_cercana(
    ventanas: list[VentanaDataset], t_ns: int
) -> VentanaDataset | None:
    """Ventana cuyo intervalo contiene t_ns o es la más cercana."""
    for ventana in ventanas:
        if ventana.t_inicio_ns <= t_ns < ventana.t_fin_ns:
            return ventana
    return None
