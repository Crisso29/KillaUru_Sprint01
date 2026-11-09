"""T007 / FR-003, FR-016 — generador de grabaciones sintéticas para pruebas.

Genera carpetas con meta.json + acelerometro.csv + giroscopio.csv + gps.csv + marcas.csv
en el formato exacto de contracts/formato-grabacion.md, con semilla fija para TDD.
"""
from __future__ import annotations

import json
import pathlib

import numpy as np

_SEMILLA = 2828
_HZ = 50
_GPS_HZ = 1
_NS_POR_S = 1_000_000_000
_T0_NS = 1_761_000_000 * _NS_POR_S  # ~2025-10-20 en nanosegundos relativos


def _generar_acelerometro(
    rng: np.random.Generator,
    n_muestras: int,
    patron: str,
    t0_ns: int,
) -> list[tuple[int, float, float, float]]:
    """Genera muestras de acelerómetro según el patrón indicado."""
    dt_ns = _NS_POR_S // _HZ
    gravedad = np.array([0.0, 0.0, 9.81])
    ruido = rng.normal(0, 0.05, (n_muestras, 3))
    señal = np.tile(gravedad, (n_muestras, 1)) + ruido

    if patron == "bache":
        pico = n_muestras // 2
        señal[pico, 2] += 12.0
        señal[pico + 2, 2] -= 8.0
    elif patron == "hundimiento":
        pico = n_muestras // 2
        señal[pico, 2] -= 10.0
    elif patron == "rompemuelles":
        pico = n_muestras // 2
        señal[pico, 2] += 6.0
    # "liso" y "inclinado" solo usan ruido / gravedad modificada

    if patron == "inclinado":
        angulo = np.radians(30)
        señal[:, 1] += 9.81 * np.sin(angulo)
        señal[:, 2] = 9.81 * np.cos(angulo) + ruido[:, 2]

    filas = []
    for i, (ax, ay, az) in enumerate(señal):
        filas.append((t0_ns + i * dt_ns, float(ax), float(ay), float(az)))
    return filas


def _generar_giroscopio(
    rng: np.random.Generator,
    n_muestras: int,
    t0_ns: int,
) -> list[tuple[int, float, float, float]]:
    dt_ns = _NS_POR_S // _HZ
    ruido = rng.normal(0, 0.01, (n_muestras, 3))
    return [(t0_ns + i * dt_ns, float(wx), float(wy), float(wz)) for i, (wx, wy, wz) in enumerate(ruido)]


def _generar_gps(
    n_segundos: int,
    velocidad_ms: float,
    t0_ns: int,
) -> list[tuple[int, int, float, float, float, float, float]]:
    filas = []
    for i in range(n_segundos):
        t_ns = t0_ns + i * _NS_POR_S
        utc_ms = 1_761_000_000_000 + i * 1000
        lat = -13.1627 + i * 0.0001
        lon = -74.2242 + i * 0.0001
        filas.append((t_ns, utc_ms, lat, lon, 5.0, velocidad_ms, 45.0))
    return filas


def _escribir_csv(ruta: pathlib.Path, encabezado: str, filas: list) -> None:
    with ruta.open("w", encoding="utf-8") as f:
        f.write(encabezado + "\n")
        for fila in filas:
            f.write(",".join(str(v) for v in fila) + "\n")


def crear_grabacion(
    directorio_salida: pathlib.Path,
    patron: str = "bache",
    duracion_s: int = 60,
    celular_alias: str = "C1",
    recorrido: str = "R01",
    sentido: str = "AyacuchoVinchos",
    es_video: bool = False,
) -> pathlib.Path:
    """Crea una carpeta de grabación sintética y devuelve su ruta."""
    fecha = "20261026"
    grabacion_id = f"{fecha}-{recorrido}-{celular_alias}"
    carpeta = directorio_salida / grabacion_id
    carpeta.mkdir(parents=True, exist_ok=True)

    rng = np.random.default_rng(_SEMILLA)
    n_muestras = duracion_s * _HZ

    meta = {
        "grabacionId": grabacion_id,
        "recorrido": recorrido,
        "grabadorVersion": "1.0.0",
        "celular": {
            "alias": celular_alias,
            "marcaModelo": "Samsung Galaxy A34",
            "ramGb": 4,
            "anioLanzamiento": 2023,
            "android": "14",
        },
        "vehiculo": {"alias": "V1", "categoria": "Liviano"},
        "soporte": {"tipo": "rejilla", "posicion": "centro-tablero"},
        "tramo": {"kmInicio": 268.0, "kmFin": 335.073, "sentido": sentido},
        "fecha": "2026-10-26",
        "esCelularVideo": es_video,
        "parametros": {"samplingPeriodUs": 20000, "maxReportLatencyUs": 1000000, "gpsIntervaloMs": 1000},
    }
    (carpeta / "meta.json").write_text(json.dumps(meta, indent=2), encoding="utf-8")

    accel = _generar_acelerometro(rng, n_muestras, patron, _T0_NS)
    _escribir_csv(carpeta / "acelerometro.csv", "t_ns,x,y,z", accel)

    giro = _generar_giroscopio(rng, n_muestras, _T0_NS)
    _escribir_csv(carpeta / "giroscopio.csv", "t_ns,x,y,z", giro)

    gps = _generar_gps(duracion_s, velocidad_ms=8.33, t0_ns=_T0_NS)
    _escribir_csv(carpeta / "gps.csv", "t_ns,utc_ms,lat,lon,precision_m,velocidad_ms,rumbo", gps)

    _escribir_csv(carpeta / "marcas.csv", "t_ns,origen", [])

    if es_video:
        video_meta = {"t_ns_primer_cuadro": _T0_NS, "fps": 30}
        (carpeta / "video.json").write_text(json.dumps(video_meta), encoding="utf-8")
        (carpeta / "video.mp4").write_bytes(b"")

    return carpeta
