// Resistencia (T076, D11): 4 h al 50 % de la carga combinada. Se busca lo que solo aparece con el tiempo:
// fugas de memoria (mirar Grafana: memoria del contenedor) y una cola que crece sin vaciarse.
import { enviarLote, consultarMapa, umbralesComunes } from './comun.js';
import { sleep } from 'k6';

export const options = {
  scenarios: {
    sincronizan: { executor: 'constant-vus', vus: 150, duration: '4h', exec: 'sincronizar' },
    miran_el_mapa: { executor: 'constant-vus', vus: 150, duration: '4h', exec: 'mirar' },
  },
  thresholds: {
    ...umbralesComunes,
    'http_req_duration{ruta:lote}': ['p(95)<300'],
    'http_req_duration{ruta:mapa}': ['p(95)<250'],
  },
};

export function sincronizar() {
  enviarLote();
  sleep(60);
}

export function mirar() {
  consultarMapa();
  sleep(5);
}
