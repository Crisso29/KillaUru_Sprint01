"""FR-005, FR-012 — ventaneo: corte de señal en ventanas de 25 muestras."""
from __future__ import annotations

from dataclasses import dataclass

import numpy as np

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

    - Asigna la etiqueta con mayor severidad a cada ventana.
    - Empate de severidad: Bache > Hundimiento > Normal (FR-012).
    - Ventana dentro de < 250 ms de un pico vecino = zona gris.
    - Aumento ±200 ms solo en conjunto='entrenamiento'.
    """
    t_ns = np.asarray(t_ns, dtype=np.int64)
    a_v = np.asarray(a_v, dtype=np.float32)
    n = len(t_ns)
    ventanas: list[VentanaDataset] = []

    # Paso temporal entre muestras (asumimos uniforme)
    paso_ns = int(t_ns[1] - t_ns[0]) if n > 1 else 20_000_000

    n_ventanas = n // _MUESTRAS_VENTANA
    for i in range(n_ventanas):
        inicio = i * _MUESTRAS_VENTANA
        fin = inicio + _MUESTRAS_VENTANA
        t_ini = int(t_ns[inicio])
        # t_fin = timestamp del primer sample de la siguiente ventana
        t_fin = int(t_ns[fin]) if fin < n else int(t_ns[fin - 1]) + paso_ns

        # Etiqueta ganadora
        tipo, severidad = _etiqueta_ventana(t_ini, t_fin, etiquetas)

        # Zona gris: algún pico vecino (fuera de esta ventana) a < 250 ms
        zona_gris = _es_zona_gris(t_ini, t_fin, etiquetas)

        ventanas.append(
            VentanaDataset(
                a_v=a_v[inicio:fin].copy(),
                t_inicio_ns=t_ini,
                t_fin_ns=t_fin,
                tipo=tipo,
                severidad=severidad,
                zona_gris=zona_gris,
                aumentada=False,
            )
        )

    # Aumento solo en entrenamiento
    if aumentar and conjunto == "entrenamiento":
        for etq in etiquetas:
            t_pico = etq["t_pico_ns"]
            for delta_ns in (-_AUMENTO_NS, +_AUMENTO_NS):
                t_aug = t_pico + delta_ns
                # Buscar ventana base más cercana
                base = _ventana_mas_cercana(ventanas, t_aug)
                if base is not None:
                    aug = VentanaDataset(
                        a_v=base.a_v.copy(),
                        t_inicio_ns=base.t_inicio_ns,
                        t_fin_ns=base.t_fin_ns,
                        tipo=etq.get("tipo", "Normal"),
                        severidad=etq.get("severidad"),
                        zona_gris=base.zona_gris,
                        aumentada=True,
                    )
                    ventanas.append(aug)

    return ventanas


# ---------------------------------------------------------------------------
# Helpers internos
# ---------------------------------------------------------------------------

_PRIORIDAD = {"Bache": 2, "Hundimiento": 1, "Normal": 0}


def _etiqueta_ventana(
    t_ini: int, t_fin: int, etiquetas: list[dict]
) -> tuple[str, int | None]:
    """Devuelve (tipo, severidad) ganadora para la ventana [t_ini, t_fin)."""
    candidatas = [
        e for e in etiquetas
        if t_ini <= e["t_pico_ns"] < t_fin
    ]
    if not candidatas:
        return "Normal", None

    def _clave(e: dict) -> tuple[int, int]:
        sev = e.get("severidad") or 0
        prio = _PRIORIDAD.get(e.get("tipo", "Normal"), 0)
        return (sev, prio)

    ganadora = max(candidatas, key=_clave)
    return ganadora.get("tipo", "Normal"), ganadora.get("severidad")


def _es_zona_gris(t_ini: int, t_fin: int, etiquetas: list[dict]) -> bool:
    """True si algún pico FUERA de esta ventana está a < 250 ms de sus bordes."""
    for e in etiquetas:
        t_pico = e["t_pico_ns"]
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
    for v in ventanas:
        if v.t_inicio_ns <= t_ns < v.t_fin_ns:
            return v
    return None
