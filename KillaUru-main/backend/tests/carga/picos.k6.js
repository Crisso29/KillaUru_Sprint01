// Picos (T075, EQ-06): un feriado largo, 600 colectivos recuperan la señal en Rumichaca a la vez y sincronizan.
import { enviarLote, umbralesComunes } from './comun.js';

export const options = {
  scenarios: {
    rafaga: {
      executor: 'ramping-vus',
      stages: [{ duration: '10s', target: 600 }, { duration: '1m', target: 600 }, { duration: '20s', target: 0 }],
      exec: 'sincronizar',
    },
  },
  thresholds: { ...umbralesComunes },
};

export function sincronizar() {
  enviarLote();
}
