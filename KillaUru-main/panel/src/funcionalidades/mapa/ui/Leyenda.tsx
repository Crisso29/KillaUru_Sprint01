// T046 — Leyenda: la marca de cada estado, igual que en el mapa (FR-023; prototipo `.mleg`)
import { marcaParaEstado, type MarcaEstado } from '../modelo/simbolos'
import { textoEstado } from '@/compartido/textos/estados'
import { TEXTOS_MAPA } from '../textos'

const ESTADOS = ['Activa', 'PendienteConfirmacion', 'Expirada', 'Reparada', 'Descartada'] as const

/** Muestra de 14 px con la marca del estado; mismo dibujo que los íconos del mapa (iconos.ts). */
function Muestra({ marca }: { marca: MarcaEstado }) {
  const relleno = { lleno: 'var(--s3)', punteado: 'var(--surface)', anillo: 'var(--surface)', check: 'var(--info)', cruz: 'var(--ink-3)' }[marca]
  const borde = marca === 'punteado' ? 'var(--s4)' : marca === 'anillo' ? 'var(--ink-3)' : 'none'
  return (
    <svg width="14" height="14" viewBox="0 0 14 14" aria-hidden="true">
      <circle cx="7" cy="7" r="5.5" fill={relleno} stroke={borde} strokeWidth="1.5" strokeDasharray={marca === 'punteado' ? '2 1.5' : undefined} />
      {marca === 'check' && <path d="M4.5 7.2 6.3 9 9.5 5.4" fill="none" stroke="#fff" strokeWidth="1.5" />}
      {marca === 'cruz' && <path d="M5 5l4 4M9 5l-4 4" stroke="#fff" strokeWidth="1.5" />}
    </svg>
  )
}

export function Leyenda() {
  return (
    <div className="mleg" role="note" aria-label="Leyenda del mapa">
      <div>
        {ESTADOS.map(estado => (
          <span key={estado}><Muestra marca={marcaParaEstado(estado)} />{textoEstado(estado)}</span>
        ))}
      </div>
      <div><span>El número es la severidad (1 a 5); 4 y 5 llevan anillo. {TEXTOS_MAPA.dentroDemo}: km 268–335,073</span></div>
    </div>
  )
}
