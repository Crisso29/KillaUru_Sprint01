// GENERADO — no editar a mano. Ejecutar: npm run tipos
// Fuente: specs/contratos/007-exportacion.yaml

export interface paths {
  '/api/v1/exportacion/anomalias': {
    get: {
      parameters: { query?: { desde?: string; hasta?: string; formato?: 'csv' | 'geojson' } }
      responses: {
        200: { content: { 'text/csv': string; 'application/geo+json': unknown } }
      }
    }
  }
}
