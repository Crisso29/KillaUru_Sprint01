// Icono SVG accesible — envuelve un emoji/SVG con aria-hidden o aria-label
interface Props { children: React.ReactNode; label?: string; className?: string }

export function Icono({ children, label, className }: Props) {
  return (
    <span
      className={className}
      aria-hidden={label ? undefined : 'true'}
      aria-label={label}
      role={label ? 'img' : undefined}
      style={{ display: 'inline-flex', alignItems: 'center' }}
    >
      {children}
    </span>
  )
}
