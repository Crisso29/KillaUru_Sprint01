// T010: servidor MSW para pruebas de unidad/integración
import { setupServer } from 'msw/node'
import { manejadoresIdentidad } from './manejadores/identidad'
import { manejadoresMapa } from './manejadores/mapa'
import { manejadoresAnomalias } from './manejadores/anomalias'
import { manejadoresDashboard } from './manejadores/dashboard'
import { manejadoresAuditoria } from './manejadores/auditoria'
import { manejadoresUsuarios } from './manejadores/usuarios'

export const server = setupServer(
  ...manejadoresIdentidad,
  ...manejadoresMapa,
  ...manejadoresAnomalias,
  ...manejadoresDashboard,
  ...manejadoresAuditoria,
  ...manejadoresUsuarios,
)
