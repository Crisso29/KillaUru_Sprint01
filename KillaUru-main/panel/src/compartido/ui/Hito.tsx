import { formatearKm } from '@/compartido/formato/km'

interface Props { km: number; label?: string }

export function Hito({ km, label }: Props) {
  return (
    <span style={{ display: 'inline-flex', alignItems: 'center', gap: 'var(--s-5)', fontSize: 'var(--t-13)', fontFamily: 'var(--f-data)', color: 'var(--ink-2)' }}>
      <span aria-hidden="true">📍</span>
      km {formatearKm(km)}{label ? ` · ${label}` : ''}
    </span>
  )
}
