// Estrés (T075, D11): sube de 100 en 100 usuarios hasta encontrar el punto de quiebre
// (p95 > 2× la meta o errores > 1 %). Aquí, y solo aquí, un 503 con Retry-After es la defensa esperada.
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';
import { URL, lote, seudonimoDelVu } from './comun.js';

const escalones = Array.from({ length: 12 }, (_, i) => ({ duration: '2m', target: (i + 1) * 100 }));

export const options = {
  scenarios: { escalera: { executor: 'ramping-vus', stages: escalones, exec: 'sincronizarYMirar' } },
  thresholds: {
    // abortOnFail: k6 se detiene en el escalón donde se rompe y ese es el punto de quiebre a anotar
    'http_req_duration{ruta:lote}': [{ threshold: 'p(95)<600', abortOnFail: true, delayAbortEval: '30s' }],
    'http_req_duration{ruta:mapa}': [{ threshold: 'p(95)<500', abortOnFail: true, delayAbortEval: '30s' }],
    'errores_reales': [{ threshold: 'rate<0.01', abortOnFail: true, delayAbortEval: '30s' }],
  },
};

const erroresReales = new Rate('errores_reales');

export function sincronizarYMirar() {
  const r = http.post(`${URL}/api/v1/anomalies/batch`, lote(), {
    headers: { 'Content-Type': 'application/json', 'X-KillaUru-Seudonimo': seudonimoDelVu() }, tags: { ruta: 'lote' },
  });
  // 503 con Retry-After = contrapresión correcta (DA-05): no cuenta como error en esta prueba
  const contrapresion = r.status === 503 && r.headers['Retry-After'] !== undefined;
  erroresReales.add(r.status >= 500 && !contrapresion);
  check(r, { 'lote 200 o 503 con Retry-After': () => r.status === 200 || contrapresion });

  const m = http.get(`${URL}/api/v1/anomalies`, { tags: { ruta: 'mapa' } });
  erroresReales.add(m.status >= 500);
  sleep(1);
}
