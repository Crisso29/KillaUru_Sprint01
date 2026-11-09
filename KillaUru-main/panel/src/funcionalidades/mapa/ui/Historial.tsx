// T047 — Historial: entradas con texto derivado de R-014 y actores resueltos (FR-039)
import type { EntradaHistorial } from '@/api/dtos/002-mapa'
import { useActores } from '@/funcionalidades/usuarios'
import { TEXTOS_DETALLE, TEXTOS_MAPA } from '../textos'

function textoTransicion(entrada: EntradaHistorial): string {
  const { desde, hacia, actorId } = entrada
  const actor = actorId !== null
  if (!desde && hacia === 'Activa') return 'Creada con el primer reporte'
  if (!desde && hacia === 'PendienteConfirmacion') return 'Nueva: espera confirmación'
  if (desde === 'PendienteConfirmacion' && hacia === 'Activa' && !actor) return 'Confirmada por 2.º vehículo'
  if ((desde === 'Activa' || desde === 'PendienteConfirmacion') && hacia === 'Expirada' && !actor) return 'Expiró sin reportes nuevos'
  if ((desde === 'Expirada' || desde === 'Reparada') && hacia === 'Activa' && !actor) return 'Volvió con un reporte nuevo'
  if ((desde === 'Expirada' || desde === 'Reparada') && hacia === 'PendienteConfirmacion' && !actor) return 'Volvió con un reporte grave: espera confirmación'
  if (hacia === 'Reparada' && actor) return 'Marcada reparada'
  if (hacia === 'Descartada' && actor) return 'Descartada'
  if ((desde === 'Reparada' || desde === 'Descartada') && hacia === 'Activa' && actor) return 'Reabierta'
  return `Cambió a ${hacia}`
}

interface Props { historial: EntradaHistorial[] }

export function Historial({ historial }: Props) {
  const ids = historial.map(e => e.actorId)
  const actores = useActores(ids)

  return (
    <section>
      <h3 style={{ fontSize: 'var(--t-14)', fontWeight: 600 }}>{TEXTOS_DETALLE.historial}</h3>
      <ol style={{ listStyle: 'none', padding: 0, margin: 0 }}>
        {historial.map((e, i) => (
          <li key={i} style={{ padding: 'var(--s-5) 0', borderBottom: '1px solid var(--border)', fontSize: 'var(--t-13)' }}>
            <span style={{ fontWeight: 500 }}>{textoTransicion(e)}</span>
            <span style={{ display: 'block', color: 'var(--ink-3)' }}>
              {e.actorId ? (actores[e.actorId] ?? e.actorId) : TEXTOS_MAPA.sistema}
              {' · '}{e.ocurridaUtc}
            </span>
          </li>
        ))}
      </ol>
    </section>
  )
}
