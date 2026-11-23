"""Ingesta (005) y consulta del mapa (006): lo que hace la app con señal.

Los eventos caen sobre el corredor real (`backend/db/datos/corredor_pe28a.geojson`, el mismo que usan la API y k6):
un punto fuera de la vía se rechazaría y la prueba no diría nada de la recepción.
"""
from __future__ import annotations

import json

from contratos import RAIZ
from peticion import Peticion, carpeta
from scripts_postman import codigo_de_error, estado, guardar, lote_aceptado, lote_duplicado

_EVENTOS = 3
_SEUDONIMO = {"X-KillaUru-Seudonimo": "{{seudonimo}}"}
_LOTE_NUEVO = [
    "// Lote nuevo: ids, horas y seudónimo únicos; un lote = un seudónimo (D2)",
    "const [longitud, latitud] = JSON.parse(pm.collectionVariables.get('puntoCorredor'));",
    "const seudonimo = pm.variables.replaceIn('{{$guid}}');",
    "const ahora = Date.now();",
    f"const eventos = [...Array({_EVENTOS}).keys()].map(i => ({{",
    "  eventoId: pm.variables.replaceIn('{{$guid}}'), detectadoUtc: new Date(ahora - (3 - i) * 10000).toISOString(),",
    "  latitud, longitud, precisionMetros: 6, tipo: 'Bache', severidad: 2, confidence: 0.9,",
    "  modeloVersion: '1.0.0', seudonimo }));",
    "pm.collectionVariables.set('seudonimo', seudonimo);",
    "pm.collectionVariables.set('lote', JSON.stringify({ eventos }));",
]


def punto_del_corredor() -> list[float]:
    """Primer vértice [lon, lat] de la polilínea PE-28A (Feature LineString o FeatureCollection)."""
    geo = json.loads((RAIZ / "backend/db/datos/corredor_pe28a.geojson").read_text(encoding="utf-8"))
    geometria = (geo["features"][0] if "features" in geo else geo)["geometry"]
    return geometria["coordinates"][0][:2]


def ingesta() -> dict:
    return carpeta("04 Ingesta", "Recepción de lotes de la app (005): idempotente por eventoId (FR-007).", [
        Peticion("Lote anónimo de 3 eventos", "POST", "/api/v1/anomalies/batch",
                 estado(200) + lote_aceptado(_EVENTOS) + guardar("loteId", "pm.response.json().loteId"),
                 cuerpo="{{lote}}", cabeceras=_SEUDONIMO, antes=_LOTE_NUEVO),
        Peticion("Reenviar el mismo lote (la app no supo si llegó)", "POST", "/api/v1/anomalies/batch",
                 estado(200) + lote_duplicado(_EVENTOS), cuerpo="{{lote}}", cabeceras=_SEUDONIMO),
        Peticion("Lote de un conductor con sesión", "POST", "/api/v1/anomalies/batch",
                 estado(200) + lote_aceptado(_EVENTOS), cuerpo="{{lote}}", cabeceras=_SEUDONIMO,
                 antes=_LOTE_NUEVO, token="tokenConductor"),
        Peticion("Lote sin eventos", "POST", "/api/v1/anomalies/batch",
                 estado(400) + codigo_de_error("lote_invalido"), cuerpo={"eventos": []}, cabeceras=_SEUDONIMO),
    ])


def mapa() -> dict:
    return carpeta("05 Mapa", "Consulta pública del mapa (006): GeoJSON con ETag para no descargar lo mismo.", [
        Peticion("Anomalías del mapa", "GET", "/api/v1/anomalies", estado(200) + [
            "pm.test('es GeoJSON (FeatureCollection)', () => {",
            "  pm.expect(pm.response.headers.get('Content-Type')).to.include('application/geo+json');",
            "  pm.expect(pm.response.json().type).to.eql('FeatureCollection');",
            "});",
            "pm.test('trae ETag para la caché condicional', () => pm.expect(pm.response.headers.get('ETag')).to.be.a('string'));",
        ] + guardar("etagMapa", "pm.response.headers.get('ETag')")),
        Peticion("Mismo mapa con If-None-Match: 304 sin cuerpo", "GET", "/api/v1/anomalies",
                 estado(304) + ["pm.test('no reenvía el cuerpo', () => pm.expect(pm.response.text()).to.be.empty);"],
                 cabeceras={"If-None-Match": "{{etagMapa}}"}),
        Peticion("Mapa de un tramo (bbox)", "GET", "/api/v1/anomalies?bbox=-74.35,-13.25,-74.20,-13.10", estado(200)),
        Peticion("bbox con 3 números en vez de 4", "GET", "/api/v1/anomalies?bbox=-74.35,-13.25,-74.20",
                 estado(400) + codigo_de_error("datos_invalidos")),
    ])
