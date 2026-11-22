// T047 — Historial: entradas con texto derivado de R-014 y actores resueltos (FR-039)
import type { EntradaHistorial } from '@/api/dtos/002-mapa'
import { formatearUtcCorto } from '@/compartido/formato/fechas'
import { textoEstado } from '@/compartido/textos/estados'
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
  return `Cambió a ${textoEstado(hacia).toLowerCase()}`
}

interface Props { historial: EntradaHistorial[] }


export function Historial({ historial }: Props) {
  const ids = historial.map(e => e.actorId)
  const actores = useActores(ids)

  return (
    <section className="dsec" aria-label={TEXTOS_DETALLE.historial}>
      <h4>{TEXTOS_DETALLE.historial}</h4>
      <ol className="tl">
        {historial.map((e, i) => (
          <li key={i} className={i === 0 ? 'hl' : undefined}>
            <div>
              <b>{textoTransicion(e)}</b>
              <small>
                {formatearUtcCorto(e.ocurridaUtc)} · {e.actorId ? (actores[e.actorId] ?? e.actorId) : TEXTOS_MAPA.sistema}
              </small>
            </div>
          </li>
        ))}
      </ol>
    </section>
  )
}
