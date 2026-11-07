// T010: servidor MSW para pruebas de unidad/integración
import { setupServer } from 'msw/node'
import { manejadoresIdentidad } from './manejadores/identidad'
import { manejadoresMapa } from './manejadores/mapa'
import { manejadoresAnomalias } from './manejadores/anomalias'
import { manejadoresDashboard } from './manejadores/dashboard'
import { manejadoresAuditoria } from './manejadores/auditoria'
import { manejadoresUsuarios } from './manejadores/usuarios'
import { manejadoresMoederacion } from './manejadores/moderacion'
import { manejadoresCuenta } from './manejadores/cuenta'
import { manejadoresExportacion } from './manejadores/exportacion'

export const server = setupServer(
  ...manejadoresIdentidad,
  ...manejadoresMapa,
  ...manejadoresAnomalias,
  ...manejadoresDashboard,
  ...manejadoresAuditoria,
  ...manejadoresUsuarios,
  ...manejadoresMoederacion,
  ...manejadoresCuenta,
  ...manejadoresExportacion,
)
