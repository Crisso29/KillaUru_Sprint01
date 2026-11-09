// DTO DE PRESENTACION — no editar a mano. Ejecutar: npm run tipos
// Fuente: specs/contratos/007-dashboard.yaml

export interface Indicador {
  valor: number | null
  comparacion: number | null
  variacion: number | null
  unidad: string
  serie: Array<number | null>
}

export interface IndicadoresDashboard {
  eventosDia: Indicador
  conductoresActivos: Indicador
  dispositivosDia: Indicador
  coberturaPct: Indicador
  confirmacionPct: Indicador
  reaparicionPct: Indicador
}

export interface EventoPorDia {
  dia: string
  eventos: number | null
  eventosPeriodoAnterior: number | null
}

export interface ResumenDashboard {
  ultimoDiaCerrado: string | null
  calculadoUtc: string
  urlPanelesTecnicos: string
  indicadores: IndicadoresDashboard
  eventosPorDia: EventoPorDia[]
}

export interface paths {
  '/api/v1/dashboard/indicadores': {
    get: {
      parameters: { query?: { dias?: 7 | 14 | 30 } }
      responses: { 200: { content: { 'application/json': ResumenDashboard } } }
    }
  }
}
