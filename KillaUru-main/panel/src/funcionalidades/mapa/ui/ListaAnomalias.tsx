// T046 — ListaAnomalias: filas navegables con tipo, km, id corto y tiempo relativo (FR-024)
import { relojReal } from '@/compartido/reloj'
import { formatearKm } from '@/compartido/formato/km'
import { formatearRelativo } from '@/compartido/formato/relativo'
import { idCorto } from '@/compartido/formato/ids'
import { EstadoVacio, Severidad, InsigniaEstado } from '@/compartido/ui'
import type { AnomaliaFeature } from '@/api/dtos/002-mapa'
import { TEXTOS_MAPA } from '../textos'

interface Props {
  features: AnomaliaFeature[]
  total: number
  idSeleccionada: string | null
  onSeleccionar: (id: string) => void
}

export function ListaAnomalias({ features, total, idSeleccionada, onSeleccionar }: Props) {
  const ahora = relojReal.ahora()

  if (!features.length) {
    return <EstadoVacio mensaje={TEXTOS_MAPA.vacioPrincipal} />
  }

  return (
    <div>
      <p className="hint" style={{ padding: 'var(--s-8) var(--s-13)' }}>
        {features.length} de {total} anomalías
      </p>
      <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
        {features.map(f => {
          const p = f.properties
          return (
            <li key={p.id}>
              <button type="button" className="lrow" onClick={() => onSeleccionar(p.id)}
                aria-pressed={idSeleccionada === p.id} aria-selected={idSeleccionada === p.id}>
                <Severidad nivel={p.severidad} tamano="sm" />
                <span>
                  <b>{p.tipo} · km {formatearKm(p.kmVia)}</b>
                  <small style={{ display: 'block' }}>{idCorto(p.id)} · {formatearRelativo(p.ultimaDeteccionUtc, ahora)}</small>
                </span>
                <InsigniaEstado estado={p.estado} />
              </button>
            </li>
          )
        })}
      </ul>
    </div>
  )
}
