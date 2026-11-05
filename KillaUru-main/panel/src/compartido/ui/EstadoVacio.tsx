interface Props { mensaje: string; className?: string }

export function EstadoVacio({ mensaje, className }: Props) {
  return (
    <div
      className={className}
      role="status"
      style={{ padding: 'var(--s-34)', textAlign: 'center', color: 'var(--ink-3)', fontSize: 'var(--t-16)' }}
    >
      {mensaje}
    </div>
  )
}
