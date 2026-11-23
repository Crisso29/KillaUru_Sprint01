"""T060 / FR-028, data-model §9 — la puerta falla por cada condición sola y pasa con todo en verde. Patrón AAA.

Regla de comparación: «≥» en los mínimos (0,900 pasa, 0,899 no) y «<» estricto en la latencia (49,9 pasa, 50,0 no).
Sin evidencia de una condición, la puerta falla: nada se aprueba por omisión.
"""
import dataclasses
import pathlib

import pytest

from killauru_ml.modelo.puerta import (
    Acuerdo,
    Equivalencia,
    EvidenciaPuerta,
    MedicionCelular,
    UmbralesPuerta,
    VerificacionParticion,
    verificar_puerta,
)

_UMBRALES = UmbralesPuerta()


def _evidencia(tmp_path: pathlib.Path, **cambios) -> EvidenciaPuerta:
    tflite = tmp_path / "killauru-1.0.0.tflite"
    tflite.write_bytes(b"x" * 1024)
    verde = EvidenciaPuerta(
        version="1.0.0", dataset_version="ds-1", version_minima_app="1.0.0", ruta_tflite=str(tflite),
        temperatura=1.3, umbral_prefiltro=1.2, clases=["Normal", "Bache", "Hundimiento"],
        precision={"Normal": 0.99, "Bache": 0.90, "Hundimiento": 0.91}, recall={"Normal": 0.99, "Bache": 0.92,
                                                                                "Hundimiento": 0.90},
        severidad_mas_menos_1=0.80, anio_medicion=2026,
        benchmark=[MedicionCelular(f"C{numero}", 4, 2023, 49.9) for numero in (1, 2, 3)],
        equivalencia=Equivalencia(tipo_igual=0.99, delta_confidence_p99=0.05, severidad_igual=0.98),
        acuerdo=Acuerdo(kappa=0.80, severidad_mas_menos_1=0.90), particion=VerificacionParticion(True, ""),
    )
    return dataclasses.replace(verde, **cambios)


def _fallidas(resultado) -> list[str]:
    return [condicion.nombre for condicion in resultado.checks if not condicion.aprobado]


def test_con_todo_en_los_limites_la_puerta_aprueba(tmp_path: pathlib.Path) -> None:
    # Arrange — cada valor está justo en su límite
    evidencia = _evidencia(tmp_path)
    # Act
    resultado = verificar_puerta(evidencia, _UMBRALES)
    # Assert
    assert (resultado.aprobado, _fallidas(resultado)) == (True, [])


@pytest.mark.parametrize(("cambios", "condicion"), [
    ({"precision": {"Normal": 0.99, "Bache": 0.899, "Hundimiento": 0.91}}, "rnf35"),
    ({"recall": {"Normal": 0.99, "Bache": 0.92, "Hundimiento": 0.899}}, "rnf35"),
    ({"severidad_mas_menos_1": 0.799}, "rnf35"),
    ({"benchmark": [MedicionCelular(f"C{numero}", 4, 2023, 50.0 if numero == 3 else 10.0) for numero in (1, 2, 3)]},
     "rnf04"),
    ({"benchmark": [MedicionCelular("C1", 4, 2023, 10.0), MedicionCelular("C2", 4, 2023, 10.0)]}, "rnf04"),
    ({"benchmark": [MedicionCelular("C1", 5, 2023, 10.0), MedicionCelular("C2", 4, 2023, 10.0),
                    MedicionCelular("C3", 4, 2021, 10.0)]}, "rnf04"),
    ({"equivalencia": Equivalencia(0.989, 0.01, 0.99)}, "equivalencia"),
    ({"equivalencia": Equivalencia(0.999, 0.051, 0.99)}, "equivalencia"),
    ({"clases": ["Normal", "Bache", "Rompemuelles"]}, "clases"),
    ({"acuerdo": Acuerdo(kappa=0.79, severidad_mas_menos_1=0.95)}, "acuerdo"),
    ({"particion": VerificacionParticion(False, "Prueba con 2 celulares")}, "particion"),
])
def test_cada_condicion_sola_hace_fallar_la_puerta(tmp_path: pathlib.Path, cambios: dict, condicion: str) -> None:
    # Arrange — todo verde salvo una condición
    evidencia = _evidencia(tmp_path, **cambios)
    # Act
    resultado = verificar_puerta(evidencia, _UMBRALES)
    # Assert — falla solo esa
    assert (resultado.aprobado, _fallidas(resultado)) == (False, [condicion])


def test_un_tflite_de_5_mb_o_mas_hace_fallar_la_puerta(tmp_path: pathlib.Path) -> None:
    # Arrange
    evidencia = _evidencia(tmp_path)
    pathlib.Path(evidencia.ruta_tflite).write_bytes(b"x" * (5 * 1024 * 1024))
    # Act
    resultado = verificar_puerta(evidencia, _UMBRALES)
    # Assert
    assert _fallidas(resultado) == ["tamano"]


@pytest.mark.parametrize("sin", ["benchmark", "equivalencia", "acuerdo", "particion"])
def test_sin_evidencia_de_una_condicion_la_puerta_falla(tmp_path: pathlib.Path, sin: str) -> None:
    # Arrange — p. ej. aún no se midió el benchmark en celulares
    evidencia = _evidencia(tmp_path, **{sin: [] if sin == "benchmark" else None})
    # Act
    resultado = verificar_puerta(evidencia, _UMBRALES)
    # Assert
    assert not resultado.aprobado
    assert "sin evidencia" in resultado.resumen() or sin == "benchmark"


def test_los_umbrales_se_leen_de_puerta_yaml() -> None:
    # Arrange
    ruta = pathlib.Path(__file__).resolve().parents[2] / "config" / "puerta.yaml"
    # Act
    umbrales = UmbralesPuerta.leer(ruta)
    # Assert — los valores de RNF-35, RNF-04 y R-014 que dice la spec
    assert (umbrales.precision_minima, umbrales.latencia_p95_maxima_ms, umbrales.tipo_igual_minimo,
            umbrales.kappa_minimo) == (0.90, 50.0, 0.99, 0.80)
