// Progresiva hasta 2 000 usuarios virtuales concurrentes (pedido del Sprint 2; D11).
// Escalones de usuarios ACTIVOS en total: mitad sincroniza lotes y mitad mira el mapa, al ritmo de carga-combinada.
// Criterio de aprobación = metas documentadas en el escalón de 300 (RNF-03): lote p95 < 300 ms (RNF-02) y
// mapa p95 < 250 ms (RNF-01). Por encima de 300 no hay meta escrita: se anota hasta dónde aguanta; un 503 con
// Retry-After es la defensa esperada (DA-05) y cualquier otro 5xx es un error.
//   k6 run -e KU_URL_PRUEBAS=http://localhost:18080 -e SALIDA=<carpeta> progresiva-2000.k6.js
import { sleep } from 'k6';
import exec from 'k6/execution';
import { Counter, Rate } from 'k6/metrics';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.1.0/index.js';
import { enviarLote, consultarMapa } from './comun.js';

// ESCALONES=10,20 permite una prueba de humo corta; sin variable, los escalones del pedido.
const ESCALONES = (__ENV.ESCALONES || '100,300,500,1000,1500,2000').split(',').map(Number);
const SUBIDA_S = Number(__ENV.SUBIDA_S || 60);     // segundos para subir al siguiente escalón
const MANTENER_S = Number(__ENV.MANTENER_S || 120); // segundos que se sostiene cada escalón (lo que se mide)

// 503 con Retry-After = contrapresión correcta; cualquier otro 5xx = error del sistema.
const errores5xx = new Rate('errores_5xx');
const contrapresion = new Rate('contrapresion_503');
// Peticiones por ruta y escalón: sin esto, http_reqs no separa lotes de consultas al mapa en el resumen.
const peticiones = new Counter('peticiones');

function etapas() {
  const lista = ESCALONES.flatMap((total) => [
    { duration: `${SUBIDA_S}s`, target: total / 2 },
    { duration: `${MANTENER_S}s`, target: total / 2 },
  ]);
  return [...lista, { duration: '30s', target: 0 }];
}

// Escalón que se está SOSTENIENDO ahora; durante las subidas la petición se etiqueta «transicion».
function escalonActual() {
  const segundos = exec.instance.currentTestRunDuration / 1000;
  const tramo = SUBIDA_S + MANTENER_S;
  const indice = Math.floor(segundos / tramo);
  const sosteniendo = indice < ESCALONES.length && segundos - indice * tramo >= SUBIDA_S;
  return sosteniendo ? String(ESCALONES[indice]) : 'transicion';
}

// porEscalon('http_req_duration', 'ruta:lote', ...) → 'http_req_duration{ruta:lote,escalon:100}', etc.
function porEscalon(metrica, filtro, limite) {
  const prefijo = filtro ? `${filtro},` : '';
  return Object.fromEntries(ESCALONES.map((e) => [`${metrica}{${prefijo}escalon:${e}}`, [limite]]));
}

export const options = {
  scenarios: {
    sincronizan: { executor: 'ramping-vus', stages: etapas(), exec: 'sincronizar', gracefulRampDown: '30s' },
    miran_el_mapa: { executor: 'ramping-vus', stages: etapas(), exec: 'mirar', gracefulRampDown: '30s' },
  },
  thresholds: {
    // Criterio de aprobación (metas documentadas en la carga de RNF-03)
    'http_req_duration{ruta:lote,escalon:300}': ['p(95)<300'],
    'http_req_duration{ruta:mapa,escalon:300}': ['p(95)<250'],
    'errores_5xx': ['rate<0.01'],
    // Informativos: obligan a k6 a reportar cada escalón por separado (límites holgados a propósito)
    ...porEscalon('http_req_duration', 'ruta:lote', 'p(95)<600000'),
    ...porEscalon('http_req_duration', 'ruta:mapa', 'p(95)<600000'),
    ...porEscalon('contrapresion_503', '', 'rate<=1'),
    ...porEscalon('errores_5xx', '', 'rate<=1'),
    ...porEscalon('peticiones', 'ruta:lote', 'count>=0'),
    ...porEscalon('peticiones', 'ruta:mapa', 'count>=0'),
  },
  summaryTrendStats: ['avg', 'med', 'p(90)', 'p(95)', 'p(99)', 'max'],
};

function registrar(r, etiquetas, ruta) {
  peticiones.add(1, { ...etiquetas, ruta });
  const defensa = r.status === 503 && r.headers['Retry-After'] !== undefined;
  contrapresion.add(defensa, etiquetas);
  errores5xx.add(r.status >= 500 && !defensa, etiquetas);
}

// Cada celular sube un lote de 50 por minuto (ritmo de carga-combinada, RNF-03).
export function sincronizar() {
  const etiquetas = { escalon: escalonActual() };
  registrar(enviarLote(etiquetas), etiquetas, 'lote');
  sleep(60);
}

export function mirar() {
  const etiquetas = { escalon: escalonActual() };
  registrar(consultarMapa(etiquetas), etiquetas, 'mapa');
  sleep(5);
}

export function handleSummary(datos) {
  const carpeta = __ENV.SALIDA || '.';
  return {
    stdout: textSummary(datos, { indent: ' ', enableColors: false }),
    [`${carpeta}/resumen.txt`]: textSummary(datos, { indent: ' ', enableColors: false }),
    [`${carpeta}/resumen.json`]: JSON.stringify(datos, null, 2),
  };
}
