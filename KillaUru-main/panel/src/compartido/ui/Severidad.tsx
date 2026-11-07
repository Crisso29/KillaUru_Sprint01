interface Props { nivel: number; className?: string }

const ETIQUETAS = ['', 'Baja', 'Leve', 'Moderada', 'Alta', 'Crítica']

export function Severidad({ nivel, className }: Props) {
  return (
    <span
      className={className}
      data-nivel={nivel}
      style={{ display: 'inline-flex', alignItems: 'center', gap: 'var(--s-5)' }}
    >
      <span className={`severidad`} data-nivel={nivel} aria-label={`Severidad ${nivel}: ${ETIQUETAS[nivel] ?? nivel}`}>
        {nivel}
        <span aria-hidden="true">{ETIQUETAS[nivel]}</span>
      </span>
    </span>
  )
}
