// Caos (T079, D11): mezcla tráfico válido con entradas inválidas para verificar que el sistema
// nunca devuelve 5xx bajo un mix real de peticiones bien formadas y tráfico basura.
// Un 500 bajo caos indica un camino de error no controlado: hay que remediarlo antes de producción.
// Ejecutar DESPUÉS de carga-combinada: si ese pasa, este busca defensas de validación.
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';
import { URL, lote, seudonimoDelVu } from './comun.js';

const erroresServidor = new Rate('errores_servidor');
const respuestasBasura4xx = new Rate('basura_4xx');

export const options = {
  scenarios: {
    // 150 VUs con tráfico legítimo: deben seguir recibiendo 200
    validos: { executor: 'constant-vus', vus: 150, duration: '5m', exec: 'traficoValido' },
    // 50 VUs enviando peticiones malformadas: deben recibir 4xx, nunca 5xx
    invalidos: { executor: 'constant-vus', vus: 50, duration: '5m', exec: 'traficoBasura' },
  },
  thresholds: {
    // Cero 500/502/503 bajo caos: el sistema valida la entrada antes de llegar a la capa de dominio (D11)
    'errores_servidor': ['rate==0'],
    // El tráfico válido no debe verse afectado por el tráfico basura
    'http_req_duration{ruta:valido}': ['p(95)<300'],
    // La basura debe recibir respuesta (4xx): verificamos que no se descarte sin responder
    'basura_4xx': ['rate>0.90'],
  },
};

// Catálogo de peticiones malformadas. Cada una DEBE producir un 4xx, nunca un 5xx.
function peticionesInvalidas() {
  return [
    // Cuerpo vacío → 400
    () => http.post(
      `${URL}/api/v1/anomalies/batch`,
      '',
      { headers: { 'Content-Type': 'application/json', 'X-KillaUru-Seudonimo': seudonimoDelVu() }, tags: { ruta: 'basura' } },
    ),
    // JSON sintácticamente inválido → 400
    () => http.post(
      `${URL}/api/v1/anomalies/batch`,
      '{"eventos":[{mal json',
      { headers: { 'Content-Type': 'application/json', 'X-KillaUru-Seudonimo': seudonimoDelVu() }, tags: { ruta: 'basura' } },
    ),
    // Lote 20× el normal (1 000 eventos): debe llegar a 413 o 400, nunca a un timeout sin respuesta
    () => http.post(
      `${URL}/api/v1/anomalies/batch`,
      lote(1000),
      { headers: { 'Content-Type': 'application/json', 'X-KillaUru-Seudonimo': seudonimoDelVu() }, tags: { ruta: 'basura' } },
    ),
    // Content-Type ausente → 415
    () => http.post(
      `${URL}/api/v1/anomalies/batch`,
      lote(),
      { headers: { 'X-KillaUru-Seudonimo': seudonimoDelVu() }, tags: { ruta: 'basura' } },
    ),
  ];
}

export function traficoValido() {
  const r = http.post(`${URL}/api/v1/anomalies/batch`, lote(), {
    headers: { 'Content-Type': 'application/json', 'X-KillaUru-Seudonimo': seudonimoDelVu() },
    tags: { ruta: 'valido' },
  });
  erroresServidor.add(r.status >= 500);
  check(r, { 'válido no es 5xx': (x) => x.status < 500 });
  sleep(1);
}

export function traficoBasura() {
  const invalidas = peticionesInvalidas();
  const fn = invalidas[__VU % invalidas.length];
  const r = fn();
  // Cualquier 5xx es un defecto; 4xx es la respuesta esperada para entrada inválida
  erroresServidor.add(r.status >= 500);
  respuestasBasura4xx.add(r.status >= 400 && r.status < 500);
  check(r, { 'basura no es 5xx': (x) => x.status < 500 });
  sleep(2);
}
