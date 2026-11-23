"""T026 / FR-011 — acuerdo entre etiquetadores: κ de Cohen y severidad ± 1."""
import pytest

from killauru_ml.etiquetado.acuerdo import calcular_acuerdo


def _evento(t_ns: int, tipo: str, severidad: int | None = None):
    return {"t_pico_ns": t_ns, "tipo": tipo, "severidad": severidad}


# --- emparejamiento a ± 250 ms ---

def test_eventos_a_200_ms_se_emparejan() -> None:
    # Arrange — 200 ms < 250 ms → emparejan
    e1 = [_evento(1_000_000_000, "Bache", 3)]
    e2 = [_evento(1_200_000_000, "Bache", 3)]  # +200 ms
    # Act
    resultado = calcular_acuerdo(e1, e2)
    # Assert
    assert resultado.n_pares == 1


def test_eventos_a_300_ms_no_se_emparejan() -> None:
    # Arrange — 300 ms > 250 ms → no emparejan
    e1 = [_evento(1_000_000_000, "Bache", 3)]
    e2 = [_evento(1_300_000_000, "Bache", 3)]  # +300 ms
    # Act
    resultado = calcular_acuerdo(e1, e2)
    # Assert
    assert resultado.n_pares == 0


# --- κ de Cohen ---

def test_kappa_079_falla() -> None:
    # Arrange — acuerdo parcial: 79 % coinciden en tipo
    e1 = [_evento(i * 1_000_000_000, tipo, 2) for i, tipo in enumerate(["Bache"] * 79 + ["Hundimiento"] * 21)]
    e2 = [_evento(i * 1_000_000_000, tipo, 2) for i, tipo in enumerate(["Bache"] * 79 + ["Bache"] * 21)]
    resultado = calcular_acuerdo(e1, e2)
    # Assert — κ < 0.80 debe marcar como no aprobado
    assert resultado.kappa < 0.80
    assert not resultado.aprobado


def test_kappa_100_pasa() -> None:
    # Arrange — acuerdo perfecto
    tipos = ["Bache", "Hundimiento", "Normal", "Bache", "Normal"]
    sevs = [2, 3, None, 1, None]
    e1 = [_evento(i * 1_000_000_000, tipo, severidad) for i, (tipo, severidad) in enumerate(zip(tipos, sevs, strict=True))]
    e2 = [_evento(i * 1_000_000_000, tipo, severidad) for i, (tipo, severidad) in enumerate(zip(tipos, sevs, strict=True))]
    resultado = calcular_acuerdo(e1, e2)
    # Assert
    assert resultado.kappa >= 0.80
    assert resultado.aprobado


# --- severidad ± 1 ---

def test_severidad_89_pct_falla() -> None:
    # Arrange — 89 % dentro de ± 1
    tipos = ["Bache"] * 100
    sev1 = [3] * 100
    sev2 = [3] * 89 + [5] * 11  # 11 % fuera de ± 1
    e1 = [_evento(i * 1_000_000_000, tipo, severidad) for i, (tipo, severidad) in enumerate(zip(tipos, sev1, strict=True))]
    e2 = [_evento(i * 1_000_000_000, tipo, severidad) for i, (tipo, severidad) in enumerate(zip(tipos, sev2, strict=True))]
    resultado = calcular_acuerdo(e1, e2)
    # Assert
    assert resultado.acuerdo_severidad < 0.90
    assert not resultado.aprobado


def test_severidad_90_pct_pasa() -> None:
    # Arrange — exactamente 90 % dentro de ± 1
    tipos = ["Bache"] * 100
    sev1 = [3] * 100
    sev2 = [3] * 90 + [5] * 10  # 10 % fuera de ± 1
    e1 = [_evento(i * 1_000_000_000, tipo, severidad) for i, (tipo, severidad) in enumerate(zip(tipos, sev1, strict=True))]
    e2 = [_evento(i * 1_000_000_000, tipo, severidad) for i, (tipo, severidad) in enumerate(zip(tipos, sev2, strict=True))]
    resultado = calcular_acuerdo(e1, e2)
    # Assert
    assert resultado.acuerdo_severidad >= 0.90


def test_acuerdo_total_con_una_sola_clase_se_aprueba_con_kappa_1() -> None:
    # Arrange — los dos etiquetadores marcan los mismos 3 baches: acuerdo perfecto, pero una sola clase
    e1 = [_evento(i * 1_000_000_000, "Bache", 3) for i in range(1, 4)]
    e2 = [_evento(i * 1_000_000_000 + 100_000_000, "Bache", 3) for i in range(1, 4)]
    # Act
    resultado = calcular_acuerdo(e1, e2)
    # Assert — κ es indefinido (0/0) solo cuando ambos usan la misma clase siempre: eso es acuerdo total
    assert resultado.kappa == 1.0
    assert resultado.aprobado


def test_eventos_a_250_ms_exactos_se_emparejan_y_a_250_ms_mas_1_ns_no() -> None:
    # Arrange — el límite de ± 250 ms es inclusivo
    referencia = [_evento(1_000_000_000, "Bache", 3)]
    en_el_limite = [_evento(1_250_000_000, "Bache", 3)]
    fuera = [_evento(1_250_000_001, "Bache", 3)]
    # Act
    pares_limite = calcular_acuerdo(referencia, en_el_limite).n_pares
    pares_fuera = calcular_acuerdo(referencia, fuera).n_pares
    # Assert
    assert (pares_limite, pares_fuera) == (1, 0)


def test_kappa_exactamente_080_pasa() -> None:
    # Arrange — 20 eventos 10/10; discrepan 2 en sentidos opuestos: acuerdo 0,90, azar 0,50 → κ = 0,80
    tipos1 = ["Bache"] * 10 + ["Hundimiento"] * 10
    tipos2 = ["Hundimiento"] + ["Bache"] * 9 + ["Bache"] + ["Hundimiento"] * 9
    e1 = [_evento(i * 1_000_000_000, tipo, 2) for i, tipo in enumerate(tipos1)]
    e2 = [_evento(i * 1_000_000_000, tipo, 2) for i, tipo in enumerate(tipos2)]
    # Act
    resultado = calcular_acuerdo(e1, e2)
    # Assert
    assert resultado.kappa == pytest.approx(0.80)
    assert resultado.aprobado
