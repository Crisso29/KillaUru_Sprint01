// Carga (T074, RNF-03, EQ-06, EQ-14): 300 celulares sincronizando y 300 mirando el mapa a la vez, 10 min.
import { enviarLote, consultarMapa, umbralesComunes } from './comun.js';
import { sleep } from 'k6';

export const options = {
  scenarios: {
    sincronizan: { executor: 'constant-vus', vus: 300, duration: '10m', exec: 'sincronizar' },
    miran_el_mapa: { executor: 'constant-vus', vus: 300, duration: '10m', exec: 'mirar' },
  },
  thresholds: {
    ...umbralesComunes,
    'http_req_duration{ruta:lote}': ['p(95)<300'],   // RNF-02
    'http_req_duration{ruta:mapa}': ['p(95)<250'],   // RNF-01 (medido en la API)
  },
};

// Cada celular sube 10 lotes de 50 en 10 min (una semana acumulada, como en RNF-03).
export function sincronizar() {
  enviarLote();
  sleep(60);
}

export function mirar() {
  consultarMapa();
  sleep(5);
}
