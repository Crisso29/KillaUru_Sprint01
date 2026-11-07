"""FR-011 — acuerdo inter-etiquetador: κ de Cohen y severidad ±1."""
from __future__ import annotations

from dataclasses import dataclass

from sklearn.metrics import cohen_kappa_score

_EMPAREJAMIENTO_NS = 250 * 1_000_000  # 250 ms en nanosegundos
_KAPPA_MINIMO = 0.80
_ACUERDO_SEVERIDAD_MINIMO = 0.90


class AcuerdoError(ValueError):
    pass


@dataclass
class ResultadoAcuerdo:
    kappa: float
    acuerdo_severidad: float
    aprobado: bool
    n_pares: int


def calcular_acuerdo(
    eventos1: list[dict],
    eventos2: list[dict],
) -> ResultadoAcuerdo:
    """Calcula κ de Cohen y acuerdo de severidad ±1 entre dos listas de eventos.

    Empareja eventos dentro de ±250 ms por greedy matching.
    """
    pares = _emparejar(eventos1, eventos2)
    n_pares = len(pares)

    if n_pares == 0:
        return ResultadoAcuerdo(
            kappa=0.0,
            acuerdo_severidad=0.0,
            aprobado=False,
            n_pares=0,
        )

    tipos1 = [p[0]["tipo"] for p in pares]
    tipos2 = [p[1]["tipo"] for p in pares]

    kappa = float(cohen_kappa_score(tipos1, tipos2))

    # Acuerdo de severidad ±1 solo en pares con severidad
    pares_sev = [
        p for p in pares
        if p[0].get("severidad") is not None and p[1].get("severidad") is not None
    ]
    if pares_sev:
        dentro = sum(
            1 for p in pares_sev
            if abs((p[0]["severidad"] or 0) - (p[1]["severidad"] or 0)) <= 1
        )
        acuerdo_sev = dentro / len(pares_sev)
    else:
        acuerdo_sev = 1.0

    aprobado = kappa >= _KAPPA_MINIMO and acuerdo_sev >= _ACUERDO_SEVERIDAD_MINIMO

    return ResultadoAcuerdo(
        kappa=kappa,
        acuerdo_severidad=acuerdo_sev,
        aprobado=aprobado,
        n_pares=n_pares,
    )


def _emparejar(
    eventos1: list[dict], eventos2: list[dict]
) -> list[tuple[dict, dict]]:
    """Greedy: cada evento de e1 se empareja con el más cercano de e2 dentro de 250 ms."""
    usados = set()
    pares = []
    for e1 in sorted(eventos1, key=lambda e: e["t_pico_ns"]):
        mejor = None
        mejor_delta = _EMPAREJAMIENTO_NS + 1
        for i, e2 in enumerate(eventos2):
            if i in usados:
                continue
            delta = abs(e1["t_pico_ns"] - e2["t_pico_ns"])
            if delta < mejor_delta:
                mejor_delta = delta
                mejor = i
        if mejor is not None and mejor_delta <= _EMPAREJAMIENTO_NS:
            usados.add(mejor)
            pares.append((e1, eventos2[mejor]))
    return pares
