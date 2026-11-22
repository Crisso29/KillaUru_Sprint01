import { textoEstado } from '@/compartido/textos/estados'

interface Props { estado: string; className?: string }

/** Estado → clase `.sd` del prototipo: cada estado tiene color y forma propios (WCAG 1.4.1, no solo color). */
const CLASE: Record<string, string> = {
  Activa: 'activa', PendienteConfirmacion: 'pendiente', Expirada: 'expirada', Reparada: 'reparada', Descartada: 'descartada',
}

/** Estado de una anomalía con su marca (prototipo `.sd`): punto lleno, discontinuo, anillo o «×». */
export function InsigniaEstado({ estado, className = '' }: Props) {
  return (
    <span className={`sd ${CLASE[estado] ?? 'expirada'} ${className}`.trim()}>
      <span>{textoEstado(estado)}</span>
    </span>
  )
}
