// Piezas comunes de las pruebas de sistema (T073, D11). Se usan con k6 FUERA de la VM, contra «killauru-pruebas».
// Usa vértices de la polilínea PE-28A. No comprueba la distancia entre vértices ni la continuidad temporal:
// antes de medir rendimiento hay que verificar que las reglas de negocio acepten el recorrido generado.
import http from 'k6/http';
import { check } from 'k6';
import { Rate } from 'k6/metrics';
import { uuidv4 } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';

export const URL = __ENV.KU_URL_PRUEBAS || 'http://localhost:18080';

// D11: bajo carga normal no debe haber throttling de usuarios individuales.
// Cada función que hace HTTP suma 1 a esta métrica cuando recibe 429.
const tasa429 = new Rate('tasa_429');

// Polilínea de 004 (T002): [[lon, lat], ...]
const corredor = JSON.parse(open('../../db/datos/corredor_pe28a.geojson')).features[0].geometry.coordinates;

// Un seudónimo por usuario virtual (= un celular), como hace la app (D2: un lote = un seudónimo).
const seudonimos = {};
export function seudonimoDelVu() {
  if (!seudonimos[__VU]) seudonimos[__VU] = uuidv4();
  return seudonimos[__VU];
}

// Punto i-ésimo del recorrido del VU: avanza por la vía, empezando en un vértice distinto por VU.
function puntoDelRecorrido(i) {
  const indice = (__VU * 37 + i) % corredor.length;
  return corredor[indice];
}

// Lote con detecciones separadas 10 s dentro de esta llamada; no implica una velocidad fija entre vértices.
export function lote(cantidad = 50) {
  const ahora = Date.now();
  const eventos = [];
  for (let i = 0; i < cantidad; i++) {
    const [lon, lat] = puntoDelRecorrido(__ITER * cantidad + i);
    eventos.push({
      eventoId: uuidv4(),
      detectadoUtc: new Date(ahora - (cantidad - i) * 10_000).toISOString(),
      latitud: lat, longitud: lon, precisionMetros: 6,
      tipo: i % 5 === 0 ? 'Hundimiento' : 'Bache', severidad: 1 + (i % 3), confidence: 0.9,
      modeloVersion: '1.0.0', seudonimo: seudonimoDelVu(),
    });
  }
  return JSON.stringify({ eventos });
}

export function enviarLote() {
  const r = http.post(`${URL}/api/v1/anomalies/batch`, lote(), {
    headers: { 'Content-Type': 'application/json', 'X-KillaUru-Seudonimo': seudonimoDelVu() },
    tags: { ruta: 'lote' },
  });
  tasa429.add(r.status === 429);
  check(r, { 'lote 200': (x) => x.status === 200 });
}

export function consultarMapa() {
  const r = http.get(`${URL}/api/v1/anomalies`, {
    headers: { 'X-KillaUru-Seudonimo': seudonimoDelVu() },
    tags: { ruta: 'mapa' },
  });
  tasa429.add(r.status === 429);
  check(r, { 'mapa 200/304': (x) => x.status === 200 || x.status === 304 });
}

// D11: cero 429 bajo carga normal (los límites de cuota están dimensionados para 300 VUs).
export const umbralesComunes = {
  'http_req_failed': ['rate<0.01'],
  'checks': ['rate>0.99'],
  'tasa_429': ['rate==0'],
};
