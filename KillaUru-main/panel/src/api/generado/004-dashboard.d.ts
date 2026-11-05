// GENERADO — no editar a mano. Ejecutar: npm run tipos
// Fuente: specs/contratos/004-dashboard.yaml

export interface ResumenDashboard {
  total_senales: number
  anomalias_activas: number
  pendientes_confirmacion: number
  resueltas_7d: number
  senales_por_dia: Array<{ fecha: string; total: number }>
}

export interface paths {
  '/api/v1/dashboard/resumen': {
    get: { responses: { 200: { content: { 'application/json': ResumenDashboard } } } }
  }
  '/api/v1/dashboard/senales-por-dia': {
    get: {
      parameters: { query?: { dias?: number } }
      responses: { 200: { content: { 'application/json': Array<{ fecha: string; total: number }> } } }
    }
  }
}
