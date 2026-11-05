// GENERADO — no editar a mano. Ejecutar: npm run tipos
// Fuente: specs/contratos/006-mapa.openapi.yaml

export type EstadoAnomalia = 'Activa' | 'PendienteConfirmacion' | 'Expirada' | 'Reparada' | 'Descartada'
export type TipoAnomalia = 'Bache' | 'Hundimiento'

export interface AnomaliaFeatureProperties {
  id: string
  kmVia: number
  tipo: TipoAnomalia
  severidad: 1 | 2 | 3 | 4 | 5
  persistencia: string
  ultimaDeteccionUtc: string
  venceUtc: string | null
  estado: EstadoAnomalia
}

export interface AnomaliaFeature {
  type: 'Feature'
  id: string
  geometry: { type: 'Point'; coordinates: [number, number] }
  properties: AnomaliaFeatureProperties
}

export interface ColeccionAnomalias {
  type: 'FeatureCollection'
  generadoUtc: string
  features: AnomaliaFeature[]
}

export interface paths {
  '/api/v1/anomalies': {
    get: {
      parameters: { query?: { estado?: string; tipo?: string; severidad_min?: number; desde?: string } }
      responses: { 200: { content: { 'application/json': ColeccionAnomalias } } }
    }
  }
  '/api/v1/anomalies/{id}': {
    get: {
      parameters: { path: { id: string } }
      responses: { 200: { content: { 'application/json': DetalleAnomalia } }; 404: { content: never } }
    }
  }
  '/api/v1/anomalies/{id}/estado': {
    patch: {
      parameters: { path: { id: string } }
      requestBody: { content: { 'application/json': { accion: string; motivo?: string } } }
      responses: { 200: { content: { 'application/json': DetalleAnomalia } } }
    }
  }
}

export interface EntradaHistorial {
  desde: string | null
  hacia: string
  ocurridaUtc: string
  actorId: string | null
}

export interface ReportePorDia {
  dia: string
  cantidad: number
}

export interface DetalleAnomalia {
  id: string
  tipo: TipoAnomalia
  severidadMostrada: number
  estado: EstadoAnomalia
  persistencia: string
  latitud: number
  longitud: number
  kmVia: number
  diasDistintos15d: number
  reportes: number
  ultimaDeteccionUtc: string
  venceUtc: string | null
  visibleConductores: boolean
  porRevisar: boolean
  vehiculosRevision: number
  reportesPorDia: ReportePorDia[]
  historial: EntradaHistorial[]
}
