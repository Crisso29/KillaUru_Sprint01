// T047 — Historial: entradas con texto derivado de R-014 y actores resueltos (FR-039)
import type { EntradaHistorial } from '@/api/dtos/002-mapa'
import { formatearUtcCorto } from '@/compartido/formato/fechas'
import { textoQuePaso } from '@/compartido/textos/transicion'
import { useActores } from '@/funcionalidades/usuarios'
import { TEXTOS_DETALLE, TEXTOS_MAPA } from '../textos'

interface Props { historial: EntradaHistorial[] }

export function Historial({ historial }: Props) {
  const actores = useActores(historial.map((entrada) => entrada.actorId))
  const quien = (actorId: string | null) => (actorId ? actores[actorId] ?? actorId : TEXTOS_MAPA.sistema)
  return (
    <section className="dsec" aria-label={TEXTOS_DETALLE.historial}>
      <h4>{TEXTOS_DETALLE.historial}</h4>
      <ol className="tl">
        {historial.map((entrada, i) => (
          <li key={i} className={i === 0 ? 'hl' : undefined}>
            <div>
              <b>{textoQuePaso(entrada.desde, entrada.hacia, entrada.actorId)}</b>
              <small>{formatearUtcCorto(entrada.ocurridaUtc)} · {quien(entrada.actorId)}</small>
            </div>
          </li>
        ))}
      </ol>
    </section>
  )
}
