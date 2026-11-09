import { textoEstado } from '@/compartido/textos/estados'

interface Props { estado: string; className?: string }

const COLOR: Record<string, string> = {
  Activa: 'var(--ok)', PendienteConfirmacion: 'var(--warn)',
  Expirada: 'var(--ink-3)', Reparada: 'var(--info)', Descartada: 'var(--ink-3)',
}

export function InsigniaEstado({ estado, className }: Props) {
  return (
    <span
      className={className}
      style={{ display: 'inline-flex', alignItems: 'center', gap: 'var(--s-5)', fontSize: 'var(--t-13)', fontFamily: 'var(--f-data)' }}
    >
      <span style={{ width: 8, height: 8, borderRadius: '50%', background: COLOR[estado] ?? 'var(--ink-3)', flexShrink: 0 }} aria-hidden="true" />
      {textoEstado(estado)}
    </span>
  )
}
