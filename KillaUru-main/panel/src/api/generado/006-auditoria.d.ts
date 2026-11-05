// GENERADO — no editar a mano. Ejecutar: npm run tipos
// Fuente: specs/contratos/006-auditoria.yaml

export interface EntradaAuditoria {
  id: string
  accion: string
  correo: string
  ip: string
  ts_utc: string
  detalle?: Record<string, unknown>
}

export interface paths {
  '/api/v1/auditoria': {
    get: {
      parameters: { query?: { desde?: string; hasta?: string; correo?: string; pagina?: number; tamano?: number } }
      responses: { 200: { content: { 'application/json': { items: EntradaAuditoria[]; total: number; pagina: number; tamano: number } } } }
    }
  }
  '/api/v1/auditoria/exportar': {
    get: {
      parameters: { query?: { desde?: string; hasta?: string } }
      responses: { 200: { content: { 'text/csv': string } } }
    }
  }
}
