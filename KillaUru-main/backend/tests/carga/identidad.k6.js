// SC-005: 300 cuentas sintéticas, login y renovación reales. Ejecutar solo contra pruebas.
import http from 'k6/http';
import exec from 'k6/execution';
import { check } from 'k6';
import { Trend } from 'k6/metrics';

const usuarios = JSON.parse(open(__ENV.USUARIOS_ARCHIVO));
const base = __ENV.BASE_URL;
const login = new Trend('login_ms', true);
const renovacion = new Trend('renovacion_ms', true);
const concurrencia = Number(__ENV.VUS || 5);

export const options = {
  scenarios: { identidad: { executor: 'shared-iterations', vus: concurrencia, iterations: 300, maxDuration: '3m' } },
  thresholds: {
    login_ms: ['p(95)<500'],
    renovacion_ms: ['p(95)<50'],
    http_req_failed: ['rate==0'],
    checks: ['rate==1'],
  },
};

export default function () {
  if (__ENV.CONFIRMAR_PRUEBAS !== 'si' || usuarios.length !== 300) throw new Error('Requiere pruebas y 300 cuentas sintéticas.');
  const cuenta = usuarios[exec.scenario.iterationInTest];
  const parametros = { headers: { 'Content-Type': 'application/json' } };
  const acceso = http.post(base + '/api/v1/auth/inicio-sesion', JSON.stringify({ ...cuenta, canal: 'app' }), parametros);
  login.add(acceso.timings.duration);
  if (!check(acceso, { 'login 200': r => r.status === 200 })) return;
  const tokenRenovacion = acceso.json('tokenRenovacion');
  const respuesta = http.post(base + '/api/v1/auth/renovar', JSON.stringify({ tokenRenovacion }), parametros);
  renovacion.add(respuesta.timings.duration);
  check(respuesta, { 'renovación 200': r => r.status === 200 });
}
