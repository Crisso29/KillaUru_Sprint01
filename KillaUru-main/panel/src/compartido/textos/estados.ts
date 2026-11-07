// T021 — textos de estados de anomalía (data-model.md §4)
export const TEXTO_ESTADO: Record<string, string> = {
  Activa: 'Activa',
  PendienteConfirmacion: 'Pendiente de confirmar',
  Expirada: 'Expirada',
  Reparada: 'Reparada',
  Descartada: 'Descartada',
}

export function textoEstado(estado: string): string {
  return TEXTO_ESTADO[estado] ?? estado
}
