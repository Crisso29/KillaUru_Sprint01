// T066 — textos UI del módulo moderación (US3)
export const TEXTOS_MODERACION = {
  accionMarcarReparada: 'Marcar como reparada',
  accionDescartar: 'Descartar',
  accionReabrir: 'Reabrir',
  accionMantenerDescartada: 'Mantener descartada',
  confirmarMarcarReparada: (idCorto: string) => `Marcar ${idCorto} como reparada`,
  confirmarDescartar: (idCorto: string) => `Descartar ${idCorto}`,
  confirmarReabrir: (idCorto: string) => `Reabrir ${idCorto}`,
  auditoria: 'Queda en la auditoría con tu usuario.',
  transicionNoPermitida: 'Ya no se puede: la anomalía cambió de estado. Actualizamos el detalle.',
  noDisponible: (desde: string, soloDesde: string) =>
    `No disponible desde ${desde}. Solo desde: ${soloDesde}.`,
  pestanaPorRevisar: 'Por revisar',
  pestanaPendientes: 'Esperan confirmación',
  vacioPorRevisar: 'No hay anomalías descartadas que requieran revisión.',
  vacioPendientes: 'No hay anomalías pendientes de confirmación.',
  verEnMapa: 'Ver en el mapa',
  confirmar: 'Confirmar',
  cancelar: 'Cancelar',
} as const
