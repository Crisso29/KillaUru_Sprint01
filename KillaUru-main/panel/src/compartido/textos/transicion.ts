// T074 / T047 — «qué pasó» en un cambio de estado: tabla de data-model §5 (FR-020, FR-039, R-014).
// Una sola tabla para el Resumen y el Historial del detalle (antes eran dos copias y ya decían cosas distintas).
import { textoEstado } from './estados'

interface Regla {
  /** Estados de origen aceptados; `null` = la anomalía recién nace. */
  desde: (string | null)[] | 'cualquiera'
  hacia: string
  /** true: lo hizo un administrador (moderación); false: el sistema (reportes, vencimiento). */
  conActor: boolean
  texto: string
}

const REGLAS: Regla[] = [
  { desde: [null], hacia: 'Activa', conActor: false, texto: 'Creada con el primer reporte' },
  { desde: [null], hacia: 'PendienteConfirmacion', conActor: false, texto: 'Nueva: espera confirmación' },
  { desde: ['PendienteConfirmacion'], hacia: 'Activa', conActor: false, texto: 'Confirmada por 2.º vehículo' },
  { desde: ['Activa', 'PendienteConfirmacion'], hacia: 'Expirada', conActor: false, texto: 'Expiró sin reportes nuevos' },
  { desde: ['Expirada', 'Reparada'], hacia: 'Activa', conActor: false, texto: 'Volvió con un reporte nuevo' },
  { desde: ['Expirada', 'Reparada'], hacia: 'PendienteConfirmacion', conActor: false, texto: 'Volvió con un reporte grave: espera confirmación' },
  { desde: 'cualquiera', hacia: 'Reparada', conActor: true, texto: 'Marcada reparada' },
  { desde: 'cualquiera', hacia: 'Descartada', conActor: true, texto: 'Descartada' },
  { desde: ['Reparada', 'Descartada'], hacia: 'Activa', conActor: true, texto: 'Reabierta' },
]

export function textoQuePaso(desde: string | null, hacia: string, actorId: string | null): string {
  const regla = REGLAS.find((candidata) => candidata.hacia === hacia && candidata.conActor === (actorId !== null)
    && (candidata.desde === 'cualquiera' || candidata.desde.includes(desde)))
  return regla?.texto ?? `Cambió a ${textoEstado(hacia).toLowerCase()}`
}
