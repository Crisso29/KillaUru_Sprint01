// T074 — textoQuePaso: tabla de data-model §5 (FR-020, R-014)

export function textoQuePaso(
  desde: string | null,
  hacia: string,
  actorId: string | null,
): string {
  const conActor = actorId !== null

  // Transiciones con actor
  if (conActor) {
    if (hacia === 'Reparada') return 'Marcada reparada'
    if (hacia === 'Descartada') return 'Descartada'
    if (hacia === 'Activa' && (desde === 'Reparada' || desde === 'Descartada')) return 'Reabierta'
    return `Cambió a ${hacia}`
  }

  // Transiciones sin actor
  if (desde === null && hacia === 'Activa') return 'Creada con el primer reporte'
  if (desde === null && hacia === 'PendienteConfirmacion') return 'Nueva: espera confirmación'
  if (desde === 'PendienteConfirmacion' && hacia === 'Activa') return 'Confirmada por 2.º vehículo'
  if ((desde === 'Activa' || desde === 'PendienteConfirmacion') && hacia === 'Expirada') {
    return 'Expiró sin reportes nuevos'
  }
  if ((desde === 'Expirada' || desde === 'Reparada') && hacia === 'Activa') {
    return 'Volvió con un reporte nuevo'
  }
  if (
    (desde === 'Expirada' || desde === 'Reparada') &&
    hacia === 'PendienteConfirmacion'
  ) {
    return 'Volvió con un reporte grave: espera confirmación'
  }
  return `Cambió a ${hacia}`
}
