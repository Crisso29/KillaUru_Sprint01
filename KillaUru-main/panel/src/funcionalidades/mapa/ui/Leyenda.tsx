// T046 — Leyenda: formas e íconos de cada estado (FR-023)
import { formaParaEstado } from '../modelo/simbolos'
import { TEXTOS_MAPA } from '../textos'

const ESTADOS = ['Activa', 'PendienteConfirmacion', 'Expirada', 'Reparada', 'Descartada'] as const
const ETIQ: Record<string, string> = {
  Activa: 'Activa',
  PendienteConfirmacion: 'Pendiente',
  Expirada: 'Expirada',
  Reparada: 'Reparada',
  Descartada: 'Descartada',
}

export function Leyenda() {
  return (
    <details>
      <summary style={{ cursor: 'pointer', fontSize: 'var(--t-13)', color: 'var(--ink-3)' }}>
        {TEXTOS_MAPA.tituloMapa}
      </summary>
      <ul style={{ listStyle: 'none', padding: 0, margin: 'var(--s-5) 0' }}>
        {ESTADOS.map(estado => (
          <li key={estado} style={{ display: 'flex', alignItems: 'center', gap: 'var(--s-5)', fontSize: 'var(--t-13)' }}>
            <span
              aria-hidden="true"
              style={{ width: 12, height: 12, borderRadius: formaParaEstado(estado) === 'circulo' ? '50%' : 2, background: 'var(--ink-2)', display: 'inline-block' }}
            />
            {ETIQ[estado]}
          </li>
        ))}
        <li style={{ fontSize: 'var(--t-13)', color: 'var(--ink-3)', marginTop: 'var(--s-5)' }}>
          {TEXTOS_MAPA.dentroDemo}: km 268–335,073
        </li>
      </ul>
    </details>
  )
}
