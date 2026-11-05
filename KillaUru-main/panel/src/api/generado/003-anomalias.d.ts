// GENERADO — no editar a mano. Ejecutar: npm run tipos
// Fuente: specs/contratos/003-anomalias.yaml

import type { Anomalia } from './002-mapa'

export interface ListaAnomalias {
  items: Anomalia[]
  total: number
  pagina: number
  tamano: number
}

export interface paths {
  '/api/v1/anomalias': {
    get: {
      parameters: { query?: { estados?: string[]; severidad_min?: number; pagina?: number; tamano?: number } }
      responses: { 200: { content: { 'application/json': ListaAnomalias } } }
    }
  }
  '/api/v1/anomalias/{id}': {
    get: {
      parameters: { path: { id: string } }
      responses: { 200: { content: { 'application/json': Anomalia } }; 404: { content: never } }
    }
  }
  '/api/v1/anomalias/{id}/estado': {
    patch: {
      parameters: { path: { id: string } }
      requestBody: { content: { 'application/json': { estado: string; motivo?: string } } }
      responses: { 200: { content: { 'application/json': { ok: boolean } } } }
    }
  }
}
