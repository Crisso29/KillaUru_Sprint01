// DTO DE PRESENTACION — no editar a mano. Ejecutar: npm run tipos
// Fuente: specs/contratos/004-anomalias.yaml

export type { DetalleAnomalia, EntradaHistorial, ReportePorDia, EstadoAnomalia, TipoAnomalia } from './002-mapa'

export type AccionModeracion = 'MarcarReparada' | 'Descartar' | 'Reabrir' | 'MantenerDescartada'

export interface ElementoPorRevisar {
  id: string
  tipo: TipoAnomalia
  kmVia: number
  severidadMostrada: number
  vehiculos7d: number
  reportes: number
  ultimaDeteccionUtc: string
}

export interface ElementoPendiente {
  id: string
  tipo: TipoAnomalia
  kmVia: number
  severidadMostrada: number
  reportes: number
  primerReporteUtc: string
  venceUtc: string
}

export interface TransicionReciente {
  anomaliaId: string
  tipo: TipoAnomalia
  kmVia: number
  desde: string | null
  hacia: string
  ocurridaUtc: string
  actorId: string | null
}

export interface SolicitudModeracion {
  accion: AccionModeracion
}
