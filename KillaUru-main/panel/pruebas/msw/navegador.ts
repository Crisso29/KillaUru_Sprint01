// Worker MSW para modo simulado en el navegador
import { setupWorker } from 'msw/browser'
import { http, HttpResponse } from 'msw'
import { manejadoresTurnstile } from './manejadores/turnstile'
import { manejadoresIdentidad } from './manejadores/identidad'
import { manejadoresMapa } from './manejadores/mapa'
import { manejadoresAnomalias } from './manejadores/anomalias'
import { manejadoresDashboard } from './manejadores/dashboard'
import { manejadoresAuditoria } from './manejadores/auditoria'
import { manejadoresCuenta } from './manejadores/cuenta'
import { manejadoresUsuarios } from './manejadores/usuarios'
import { manejadoresMoederacion } from './manejadores/moderacion'
import { manejadoresExportacion } from './manejadores/exportacion'

export const worker = setupWorker(
  ...manejadoresIdentidad,
  ...manejadoresMapa,
  ...manejadoresAnomalias,
  ...manejadoresDashboard,
  ...manejadoresAuditoria,
  ...manejadoresCuenta,
  ...manejadoresUsuarios,
  ...manejadoresMoederacion,
  ...manejadoresExportacion,
  ...manejadoresTurnstile,
)

/**
 * Solo en modo simulado: deja el worker en `window.__killauruSimulado` para que una prueba E2E cambie una respuesta
 * (401, 423, 409…) con `worker.use`. El build de producción no incluye este archivo (`__SIMULADO__` es false).
 */
export function exponerParaPruebas() {
  Object.assign(window, { __killauruSimulado: { worker, http, HttpResponse, base: import.meta.env.VITE_URL_API } })
}
