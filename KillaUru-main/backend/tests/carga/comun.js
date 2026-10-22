// Piezas comunes de las pruebas de sistema (T073, D11). Se usan con k6 FUERA de la VM, contra «killauru-pruebas».
// Los eventos caen sobre la polilínea real de la PE-28A y respetan una velocidad plausible, para que no los
// descarten RN-13 (50 m de la vía) ni RN-15b (velocidad imposible): así se mide la carga, no el rechazo.
import http from 'k6/http';
import { check } from 'k6';
import { uuidv4 } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';

export const URL = __ENV.KU_URL_PRUEBAS || 'http://localhost:18080';

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

// Lote de `cantidad` eventos detectados cada 10 s (≈ 36 km/h entre vértices cercanos: plausible).
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
  check(r, { 'lote 200': (x) => x.status === 200 });
}

export function consultarMapa() {
  const r = http.get(`${URL}/api/v1/anomalies`, {
    headers: { 'X-KillaUru-Seudonimo': seudonimoDelVu() },
    tags: { ruta: 'mapa' },
  });
  check(r, { 'mapa 200/304': (x) => x.status === 200 || x.status === 304 });
}

// Criterio de error de D11: todo 5xx es error; un 429 no debe aparecer en «killauru-pruebas».
export const umbralesComunes = {
  'http_req_failed': ['rate<0.01'],
  'checks': ['rate>0.99'],
};
