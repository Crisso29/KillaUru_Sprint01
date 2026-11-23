"""T027 / FR-005, FR-012, RN-03 — ventaneo: tamaño, zona gris, prioridad, exclusiones, privacidad. Patrón AAA.

La señal de prueba empieza con 10 s de marcha (`_CALENTAMIENTO`): RN-03 exige > 5 km/h sostenidos ≥ 10 s, así que
antes de eso ninguna ventana es apta. `_inicio(t_ns, k)` es la hora de la ventana k contada después del calentamiento.
"""
from itertools import pairwise

import numpy as np

from killauru_ml.ventanas.ventaneo import ventilar

_HZ = 50
_MUESTRAS = 25
_NS_POR_MS = 1_000_000
_PASO_NS = 1_000_000_000 // _HZ
_CALENTAMIENTO = 10 * _HZ                  # 500 muestras = 10 s de marcha
_VELOCIDAD_MS = 10.0                       # 36 km/h


def _señal(n_ventanas: int = 4, velocidad_ms: float = _VELOCIDAD_MS) -> tuple[np.ndarray, ...]:
    """Calentamiento de 10 s + n ventanas: horas, a_v, velocidad (m/s) por muestra y km."""
    muestras = _CALENTAMIENTO + n_ventanas * _MUESTRAS
    t_ns = np.arange(muestras, dtype=np.int64) * _PASO_NS
    velocidad = np.full(muestras, velocidad_ms, dtype=np.float32)
    return t_ns, np.zeros(muestras, dtype=np.float32), velocidad, np.arange(muestras, dtype=np.float32) * 0.001


def _inicio(t_ns: np.ndarray, ventana: int) -> int:
    return int(t_ns[_CALENTAMIENTO + ventana * _MUESTRAS])


def _ventana(ventanas: list, t_inicio_ns: int):
    return next(ventana for ventana in ventanas if ventana.t_inicio_ns == t_inicio_ns)


def _bache(t_pico_ns: int, severidad: int = 3, tipo: str = "Bache") -> dict:
    return {"t_pico_ns": int(t_pico_ns), "tipo": tipo, "severidad": severidad}


# --- tamaño y no solapamiento ---

def test_ventanas_tienen_25_muestras_y_no_se_solapan() -> None:
    # Arrange
    t_ns, a_v, vel, km = _señal(4)
    # Act
    ventanas = ventilar(t_ns, a_v, vel, km, etiquetas=[], conjunto="prueba")
    # Assert
    assert all(len(ventana.a_v) == 25 for ventana in ventanas)
    assert all(siguiente.t_inicio_ns == anterior.t_fin_ns for anterior, siguiente in pairwise(ventanas))


def test_pico_en_la_ultima_muestra_pertenece_a_esa_ventana() -> None:
    # Arrange — muestra 24 de la ventana 1 (la última); la ventana 2 empieza 20 ms después
    t_ns, a_v, vel, km = _señal(4)
    ultima_muestra = _inicio(t_ns, 1) + 24 * _PASO_NS
    # Act
    ventanas = ventilar(t_ns, a_v, vel, km, etiquetas=[_bache(ultima_muestra)], conjunto="entrenamiento")
    # Assert
    assert (_ventana(ventanas, _inicio(t_ns, 1)).tipo, _ventana(ventanas, _inicio(t_ns, 2)).tipo) == ("Bache", "Normal")


# --- RN-03: > 5 km/h sostenidos ≥ 10 s ---

def test_a_5_km_h_ninguna_ventana_es_apta_y_a_5_1_si() -> None:
    # Arrange — 5 km/h = 1,3889 m/s; RN-03 exige estrictamente más
    a_5 = _señal(4, velocidad_ms=5.0 / 3.6)
    a_5_1 = _señal(4, velocidad_ms=5.1 / 3.6)
    # Act
    ventanas_a_5 = ventilar(*a_5, etiquetas=[], conjunto="prueba")
    ventanas_a_5_1 = ventilar(*a_5_1, etiquetas=[], conjunto="prueba")
    # Assert
    assert (len(ventanas_a_5), len(ventanas_a_5_1) > 0) == (0, True)


def test_la_marcha_debe_durar_10_s_antes_del_fin_de_la_ventana() -> None:
    # Arrange — detenido 2 s; desde la muestra 100 (t = 2 s) va a 36 km/h
    t_ns, a_v, vel, km = _señal(4)
    vel[:100] = 0.0
    arranque_ns = int(t_ns[100])
    # Act
    ventanas = ventilar(t_ns, a_v, vel, km, etiquetas=[], conjunto="prueba")
    # Assert — la primera apta termina justo 10 s después del arranque; ninguna termina antes
    assert min(ventana.t_fin_ns for ventana in ventanas) - arranque_ns == 10_000 * _NS_POR_MS


def test_ventana_con_frenazo_a_4_km_h_se_excluye() -> None:
    # Arrange — una sola muestra a 4 km/h dentro de la ventana 2
    t_ns, a_v, vel, km = _señal(4)
    vel[_CALENTAMIENTO + 2 * _MUESTRAS + 10] = 4.0 / 3.6
    # Act
    ventanas = ventilar(t_ns, a_v, vel, km, etiquetas=[], conjunto="prueba")
    # Assert — la racha se corta: tampoco valen las siguientes hasta acumular otros 10 s
    assert [ventana.t_inicio_ns for ventana in ventanas if ventana.t_inicio_ns >= _inicio(t_ns, 0)] == \
        [_inicio(t_ns, 0), _inicio(t_ns, 1)]


# --- huecos (FR-007: solo cuenta un intervalo estrictamente > 200 ms) ---

def test_ventana_con_hueco_de_201_ms_se_excluye_y_con_200_ms_no() -> None:
    # Arrange — se corre el tiempo de las muestras posteriores: un intervalo de 201 ms en la ventana 1, de 200 en la 3
    t_ns, a_v, vel, km = _señal(4)
    t_ns[_CALENTAMIENTO + _MUESTRAS + 5:] += 181 * _NS_POR_MS    # 20 ms + 181 ms = 201 ms
    t_ns[_CALENTAMIENTO + 3 * _MUESTRAS + 5:] += 180 * _NS_POR_MS  # 20 ms + 180 ms = 200 ms
    # Act
    inicios = {ventana.t_inicio_ns for ventana in ventilar(t_ns, a_v, vel, km, etiquetas=[], conjunto="prueba")}
    # Assert
    assert (_inicio(t_ns, 1) in inicios, _inicio(t_ns, 3) in inicios) == (False, True)


# --- zona gris: vecina a < 250 ms del pico ---

def test_vecina_a_249_ms_es_zona_gris_y_a_250_ms_no() -> None:
    # Arrange — picos dentro de la ventana 2 a 249 ms y a 250 ms del fin de la ventana 1
    t_ns, a_v, vel, km = _señal(4)
    fin_ventana_1 = _inicio(t_ns, 2)
    # Act
    a_249 = ventilar(t_ns, a_v, vel, km, etiquetas=[_bache(fin_ventana_1 + 249 * _NS_POR_MS)], conjunto="prueba")
    a_250 = ventilar(t_ns, a_v, vel, km, etiquetas=[_bache(fin_ventana_1 + 250 * _NS_POR_MS)], conjunto="prueba")
    # Assert
    assert _ventana(a_249, _inicio(t_ns, 1)).zona_gris
    assert not _ventana(a_250, _inicio(t_ns, 1)).zona_gris


# --- prioridad de etiquetas ---

def test_dos_eventos_en_misma_ventana_gana_mayor_severidad() -> None:
    # Arrange
    t_ns, a_v, vel, km = _señal(4)
    base = _inicio(t_ns, 1)
    etiquetas = [_bache(base + 5 * _NS_POR_MS, 2), _bache(base + 10 * _NS_POR_MS, 4)]
    # Act
    ventanas = ventilar(t_ns, a_v, vel, km, etiquetas=etiquetas, conjunto="entrenamiento")
    # Assert
    assert _ventana(ventanas, base).severidad == 4


def test_empate_de_severidad_gana_bache() -> None:
    # Arrange
    t_ns, a_v, vel, km = _señal(4)
    base = _inicio(t_ns, 1)
    etiquetas = [_bache(base + 5 * _NS_POR_MS, 3, "Hundimiento"), _bache(base + 10 * _NS_POR_MS, 3)]
    # Act
    ventanas = ventilar(t_ns, a_v, vel, km, etiquetas=etiquetas, conjunto="entrenamiento")
    # Assert — FR-012
    assert _ventana(ventanas, base).tipo == "Bache"


# --- privacidad y aumento ---

def test_salida_no_tiene_latitud_ni_longitud() -> None:
    # Arrange
    t_ns, a_v, vel, km = _señal(4)
    # Act
    ventanas = ventilar(t_ns, a_v, vel, km, etiquetas=[], conjunto="prueba")
    # Assert — FR-005
    assert not any(hasattr(ventana, "lat") or hasattr(ventana, "lon") for ventana in ventanas)


def test_aumento_solo_en_entrenamiento() -> None:
    # Arrange
    t_ns, a_v, vel, km = _señal(4)
    etiquetas = [_bache(_inicio(t_ns, 1) + 240 * _NS_POR_MS)]
    # Act
    entrenamiento = ventilar(t_ns, a_v, vel, km, etiquetas=etiquetas, conjunto="entrenamiento", aumentar=True)
    prueba = ventilar(t_ns, a_v, vel, km, etiquetas=etiquetas, conjunto="prueba", aumentar=True)
    # Assert
    assert any(ventana.aumentada for ventana in entrenamiento)
    assert not any(ventana.aumentada for ventana in prueba)
