// Worker MSW para modo simulado en el navegador
import { setupWorker } from 'msw/browser'
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
)
