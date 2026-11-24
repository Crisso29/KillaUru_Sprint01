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
  if (!features.length) return <EstadoVacio mensaje={TEXTOS_MAPA.vacioPrincipal} />
  const ahora = relojReal.ahora()
  return (
    <div>
      <p className="hint" style={{ padding: 'var(--s-8) var(--s-13)' }}>{features.length} de {total} anomalías</p>
      <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
        {features.map(({ properties: anomalia }) => (
          <li key={anomalia.id}>
            <FilaAnomalia anomalia={anomalia} ahora={ahora} elegida={idSeleccionada === anomalia.id} onElegir={() => onSeleccionar(anomalia.id)} />
          </li>
        ))}
      </ul>
    </div>
  )
}

interface PropsFila { anomalia: AnomaliaFeature['properties']; ahora: Date; elegida: boolean; onElegir: () => void }

function FilaAnomalia({ anomalia, ahora, elegida, onElegir }: PropsFila) {
  return (
    <button type="button" className="lrow" onClick={onElegir} aria-pressed={elegida}>
      <Severidad nivel={anomalia.severidad} tamano="sm" />
      <span>
        <b>{anomalia.tipo} · km {formatearKm(anomalia.kmVia)}</b>
        <small style={{ display: 'block' }}>{idCorto(anomalia.id)} · {formatearRelativo(anomalia.ultimaDeteccionUtc, ahora)}</small>
      </span>
      <InsigniaEstado estado={anomalia.estado} />
    </button>
  )
}
