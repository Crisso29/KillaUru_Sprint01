// T076 — UltimosCambios: lista de transiciones con «Qué pasó» y «Quién» (FR-020)
import { Link } from 'react-router'
import type { TransicionReciente } from '@/api/generado/003-anomalias'
import { textoQuePaso } from '../modelo/transicion'
import { TEXTOS_RESUMEN } from '../textos'

interface Props {
  transiciones: TransicionReciente[]
}

function formatearHora(utc: string): string {
  return new Date(utc).toLocaleTimeString('es-PE', {
    hour: '2-digit',
    minute: '2-digit',
    timeZone: 'America/Lima',
  })
}

export function UltimosCambios({ transiciones }: Props) {
  if (transiciones.length === 0) {
    return (
      <section aria-label={TEXTOS_RESUMEN.ultimosCambios.titulo}>
        <h3>{TEXTOS_RESUMEN.ultimosCambios.titulo}</h3>
        <p>{TEXTOS_RESUMEN.vacio.ultimosCambios}</p>
      </section>
    )
  }

  return (
    <section aria-label={TEXTOS_RESUMEN.ultimosCambios.titulo}>
      <h3>{TEXTOS_RESUMEN.ultimosCambios.titulo}</h3>
      <ul>
        {transiciones.map((t) => (
          <li key={`${t.anomaliaId}-${t.ocurridaUtc}`}>
            <Link to={`/mapa?anomalia=${t.anomaliaId}`}>
              <span>{formatearHora(t.ocurridaUtc)}</span>
              <span>
                {t.tipo} · km {t.kmVia.toLocaleString('es-PE', { maximumFractionDigits: 1 })}
              </span>
              <span>{textoQuePaso(t.desde, t.hacia, t.actorId)}</span>
              <span>
                {t.actorId
                  ? t.actorId.slice(0, 8)
                  : TEXTOS_RESUMEN.ultimosCambios.sistema}
              </span>
              <span aria-label="Estado nuevo">{t.hacia}</span>
            </Link>
          </li>
        ))}
      </ul>
    </section>
  )
}
