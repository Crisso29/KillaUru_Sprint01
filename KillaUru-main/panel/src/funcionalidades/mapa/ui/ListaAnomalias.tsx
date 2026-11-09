// T046 — ListaAnomalias: filas navegables con tipo, km, id corto y tiempo relativo (FR-024)
import { relojReal } from '@/compartido/reloj'
import { formatearKm } from '@/compartido/formato/km'
import { formatearRelativo } from '@/compartido/formato/relativo'
import { idCorto } from '@/compartido/formato/ids'
import { EstadoVacio } from '@/compartido/ui'
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
      <p style={{ padding: 'var(--s-5) var(--s-13)', fontSize: 'var(--t-13)', color: 'var(--ink-3)' }}>
        {features.length} de {total} anomalías
      </p>
      <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
        {features.map(f => {
          const p = f.properties
          return (
            <li key={p.id}>
              <button
                type="button"
                onClick={() => onSeleccionar(p.id)}
                style={{
                  width: '100%', textAlign: 'left', padding: 'var(--s-8) var(--s-13)',
                  background: idSeleccionada === p.id ? 'var(--surface-2)' : 'transparent',
                  border: 'none', cursor: 'pointer',
                }}
                aria-pressed={idSeleccionada === p.id}
              >
                <span style={{ fontWeight: 600 }}>{p.tipo} · km {formatearKm(p.kmVia)}</span>
                <span style={{ display: 'block', fontSize: 'var(--t-13)', color: 'var(--ink-3)' }}>
                  {idCorto(p.id)} · {formatearRelativo(p.ultimaDeteccionUtc, ahora)}
                </span>
              </button>
            </li>
          )
        })}
      </ul>
    </div>
  )
}
