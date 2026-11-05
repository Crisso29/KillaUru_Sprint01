// GENERADO — no editar a mano. Ejecutar: npm run tipos
// Fuente: specs/contratos/002-mapa.yaml

export interface Anomalia {
  id: string
  estado: 'Activa' | 'PendienteConfirmacion' | 'Resuelta' | 'Descartada'
  severidad: 1 | 2 | 3 | 4 | 5
  km: number
  tipo: string
  creado_utc: string
  coordenadas: [number, number]
}

export interface paths {
  '/api/v1/mapa/anomalias': {
    get: {
      parameters: { query?: { estados?: string[]; severidad_min?: number; km_desde?: number; km_hasta?: number } }
      responses: { 200: { content: { 'application/json': { items: Anomalia[] } } } }
    }
  }
}
