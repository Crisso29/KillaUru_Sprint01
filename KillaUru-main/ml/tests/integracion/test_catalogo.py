"""RN-01, R-017 — el catálogo de clases es uno solo y su orden es el del tensor que lee la app.

El modelo devuelve las probabilidades en este orden: si cambiara, la app leería un bache como hundimiento. Se compara con
el manifiesto que trae el APK y con el contrato de la CDN. Patrón AAA.
"""
import json
from pathlib import Path

import yaml

from killauru_ml.comun.catalogo import CLASES

RAIZ = Path(__file__).resolve().parents[3]


def test_el_orden_de_clases_es_el_del_manifiesto_del_apk() -> None:
    # Arrange
    manifiesto = json.loads((RAIZ / "app/src/main/assets/modelo/manifiesto.json").read_text(encoding="utf-8"))
    # Act
    etiquetas_apk = tuple(manifiesto["clases"])  # la app lee "clases" (ManifiestoModelo.kt); CDN usa "etiquetas"
    # Assert
    assert etiquetas_apk == CLASES


def test_el_orden_de_clases_es_el_del_contrato_de_la_cdn() -> None:
    # Arrange
    contrato = yaml.safe_load((RAIZ / "specs/008-app-android/contracts/cdn.openapi.yaml").read_text(encoding="utf-8"))
    # Act
    ejemplo = contrato["paths"]["/modelo/manifiesto.json"]["get"]["responses"]["200"]["content"]["application/json"]["example"]
    # Assert
    assert tuple(ejemplo["etiquetas"]) == CLASES
