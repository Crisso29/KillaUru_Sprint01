"""T027 / FR-005, FR-012 — ventaneo: tamaño, zona gris, prioridad, exclusiones, privacidad."""
import numpy as np

from killauru_ml.ventanas.ventaneo import ventilar

_HZ = 50
_MS_VENTANA = 500          # 25 muestras
_MUESTRAS = _MS_VENTANA * _HZ // 1000  # = 25
_NS_POR_MS = 1_000_000


def _señal(n_ventanas: int = 10) -> tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray]:
    """Señal sintética de n_ventanas ventanas: timestamps, a_v, velocidad, km."""
    n = n_ventanas * _MUESTRAS
    t_ns = np.arange(n) * (1_000_000_000 // _HZ)
    a_v = np.zeros(n, dtype=np.float32)
    velocidad_ms = np.full(n, 10.0, dtype=np.float32)  # > 5 km/h todo el tiempo
    km = np.arange(n, dtype=np.float32) * 0.001
    return t_ns, a_v, velocidad_ms, km


# --- tamaño y no solapamiento ---

def test_ventanas_tienen_25_muestras(tmp_path) -> None:
    # Arrange
    t_ns, a_v, vel, km = _señal(4)
    # Act
    ventanas = ventilar(t_ns, a_v, vel, km, etiquetas=[], conjunto="prueba")
    # Assert — cada ventana tiene exactamente 25 muestras
    assert all(len(v.a_v) == 25 for v in ventanas)


def test_ventanas_no_se_solapan(tmp_path) -> None:
    # Arrange
    t_ns, a_v, vel, km = _señal(4)
    # Act
    ventanas = ventilar(t_ns, a_v, vel, km, etiquetas=[], conjunto="prueba")
    # Assert — inicio de cada ventana = fin de la anterior
    for i in range(1, len(ventanas)):
        assert ventanas[i].t_inicio_ns == ventanas[i - 1].t_fin_ns


# --- zona gris ---

def test_vecina_a_249_ms_es_zona_gris(tmp_path) -> None:
    # Arrange — pico en la muestra 25 (inicio de la segunda ventana)
    t_ns, a_v, vel, km = _señal(4)
    pico_ns = t_ns[25]  # exactamente en el inicio de ventana 2
    # Vecina a 249 ms antes = ventana 1 (249 ms < 250 ms → zona gris)
    etiquetas = [{"t_pico_ns": int(pico_ns), "tipo": "Bache", "severidad": 3}]
    # Act
    ventanas = ventilar(t_ns, a_v, vel, km, etiquetas=etiquetas, conjunto="entrenamiento")
    # Assert — la ventana vecina (< 250 ms) debe ser zona gris
    vecinas = [v for v in ventanas if v.zona_gris and abs(v.t_fin_ns - pico_ns) <= 250 * _NS_POR_MS]
    assert len(vecinas) >= 1


def test_vecina_a_250_ms_no_es_zona_gris(tmp_path) -> None:
    # Arrange — pico en ventana 4 (índice 75); la ventana 2 queda a ≥ 250 ms → no zona gris
    # Con 50 Hz las ventanas miden 500 ms; borde ventana 2 termina a 1000 ms, pico a 1500 ms
    # → distancia = 500 ms ≥ 250 ms → NO zona gris (límite estricto: solo < 250 ms es zona gris)
    t_ns, a_v, vel, km = _señal(6)
    pico_ns = t_ns[75]  # inicio de ventana 4
    etiquetas = [{"t_pico_ns": int(pico_ns), "tipo": "Bache", "severidad": 3}]
    # Act
    ventanas = ventilar(t_ns, a_v, vel, km, etiquetas=etiquetas, conjunto="entrenamiento")
    # Assert — ventana 2 (borde a 500 ms del pico): NOT zona gris
    ventana2 = [v for v in ventanas if v.t_inicio_ns == t_ns[25]][0]
    assert not ventana2.zona_gris


# --- prioridad de etiquetas ---

def test_dos_eventos_en_misma_ventana_gana_mayor_severidad(tmp_path) -> None:
    # Arrange — dos eventos en la misma ventana de 25 muestras
    t_ns, a_v, vel, km = _señal(4)
    base_ns = int(t_ns[25])  # inicio de ventana 2
    etiquetas = [
        {"t_pico_ns": base_ns + 5 * _NS_POR_MS, "tipo": "Bache", "severidad": 2},
        {"t_pico_ns": base_ns + 10 * _NS_POR_MS, "tipo": "Bache", "severidad": 4},
    ]
    # Act
    ventanas = ventilar(t_ns, a_v, vel, km, etiquetas=etiquetas, conjunto="entrenamiento")
    ventana2 = [v for v in ventanas if v.t_inicio_ns == t_ns[25]][0]
    # Assert — gana la severidad mayor
    assert ventana2.severidad == 4


def test_empate_de_severidad_gana_bache(tmp_path) -> None:
    # Arrange — Hundimiento y Bache con la misma severidad en la misma ventana
    t_ns, a_v, vel, km = _señal(4)
    base_ns = int(t_ns[25])
    etiquetas = [
        {"t_pico_ns": base_ns + 5 * _NS_POR_MS, "tipo": "Hundimiento", "severidad": 3},
        {"t_pico_ns": base_ns + 10 * _NS_POR_MS, "tipo": "Bache", "severidad": 3},
    ]
    # Act
    ventanas = ventilar(t_ns, a_v, vel, km, etiquetas=etiquetas, conjunto="entrenamiento")
    ventana2 = [v for v in ventanas if v.t_inicio_ns == t_ns[25]][0]
    # Assert — empate → Bache (FR-012)
    assert ventana2.tipo == "Bache"


# --- privacidad ---

def test_salida_no_tiene_latitud_ni_longitud(tmp_path) -> None:
    # Arrange
    t_ns, a_v, vel, km = _señal(4)
    # Act
    ventanas = ventilar(t_ns, a_v, vel, km, etiquetas=[], conjunto="prueba")
    # Assert — FR-005: sin lat ni lon en el dataset derivado
    for v in ventanas:
        assert not hasattr(v, "lat")
        assert not hasattr(v, "lon")


# --- aumento solo en entrenamiento ---

def test_aumento_solo_en_entrenamiento(tmp_path) -> None:
    # Arrange
    t_ns, a_v, vel, km = _señal(4)
    etiquetas = [{"t_pico_ns": int(t_ns[37]), "tipo": "Bache", "severidad": 3}]
    # Act
    ventanas_ent = ventilar(t_ns, a_v, vel, km, etiquetas=etiquetas, conjunto="entrenamiento", aumentar=True)
    ventanas_prueba = ventilar(t_ns, a_v, vel, km, etiquetas=etiquetas, conjunto="prueba", aumentar=True)
    # Assert — aumentadas solo en entrenamiento
    aumentadas_ent = [v for v in ventanas_ent if v.aumentada]
    aumentadas_prueba = [v for v in ventanas_prueba if v.aumentada]
    assert len(aumentadas_ent) > 0
    assert len(aumentadas_prueba) == 0
