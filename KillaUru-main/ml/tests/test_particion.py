"""T033 / FR-013 — partición por recorrido: tres conjuntos, sin fuga."""
import numpy as np

from killauru_ml.particion.particion import partir
from killauru_ml.ventanas.ventaneo import VentanaDataset


def _ventana(recorrido: str = "R01") -> VentanaDataset:
    return VentanaDataset(
        a_v=np.zeros(25, dtype=np.float32),
        t_inicio_ns=0,
        t_fin_ns=500_000_000,
        tipo="Normal",
        recorrido=recorrido,
    )


def _agrupado(n: int = 10) -> dict[str, list[VentanaDataset]]:
    """n recorridos, 20 ventanas cada uno."""
    return {f"R{i:02d}": [_ventana(f"R{i:02d}") for _ in range(20)] for i in range(1, n + 1)}


# --- conjuntos ---

def test_partir_devuelve_tres_conjuntos() -> None:
    # Arrange
    agrupado = _agrupado(10)
    # Act
    particion = partir(agrupado)
    # Assert
    assert set(particion.keys()) == {"entrenamiento", "validacion", "prueba"}


def test_conjuntos_no_vacios() -> None:
    # Arrange
    agrupado = _agrupado(10)
    # Act
    particion = partir(agrupado)
    # Assert — ningún conjunto vacío con 10 recorridos
    for nombre, ventanas in particion.items():
        assert len(ventanas) > 0, f"conjunto '{nombre}' vacío"


def test_total_ventanas_conservado() -> None:
    # Arrange
    agrupado = _agrupado(10)
    total_entrada = sum(len(v) for v in agrupado.values())
    # Act
    particion = partir(agrupado)
    total_salida = sum(len(v) for v in particion.values())
    # Assert — ninguna ventana se pierde
    assert total_salida == total_entrada


# --- reproducibilidad ---

def test_misma_semilla_mismo_resultado() -> None:
    # Arrange
    agrupado = _agrupado(12)
    # Act
    p1 = partir(agrupado, semilla=2828)
    p2 = partir(agrupado, semilla=2828)
    # Assert — mismos recorridos en prueba
    rec1 = {v.recorrido for v in p1["prueba"]}
    rec2 = {v.recorrido for v in p2["prueba"]}
    assert rec1 == rec2


def test_semillas_distintas_distinto_resultado() -> None:
    # Arrange — con muchos recorridos es muy improbable que coincidan
    agrupado = _agrupado(20)
    # Act
    p1 = partir(agrupado, semilla=2828)
    p2 = partir(agrupado, semilla=42)
    # Assert — al menos un recorrido de prueba distinto
    rec1 = {v.recorrido for v in p1["prueba"]}
    rec2 = {v.recorrido for v in p2["prueba"]}
    assert rec1 != rec2


# --- no fuga ---

def test_recorrido_aparece_en_un_solo_conjunto() -> None:
    # Arrange
    agrupado = _agrupado(10)
    # Act
    particion = partir(agrupado)
    # Assert — cada recorrido en exactamente un conjunto
    por_recorrido: dict[str, list[str]] = {}
    for nombre, ventanas in particion.items():
        for v in ventanas:
            por_recorrido.setdefault(v.recorrido, []).append(nombre)
    for rec, conjuntos in por_recorrido.items():
        assert len(set(conjuntos)) == 1, f"{rec} aparece en {set(conjuntos)}"
