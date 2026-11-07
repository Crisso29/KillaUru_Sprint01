interface Props { filas?: number; alto?: number; className?: string }

export function Esqueleto({ filas = 1, alto = 20, className }: Props) {
  return (
    <div className={className} aria-busy="true" aria-label="Cargando…">
      {Array.from({ length: filas }, (_, i) => (
        <div key={i} style={{
          height: alto, borderRadius: 'var(--s-5)', background: 'var(--sunken)',
          marginBottom: i < filas - 1 ? 'var(--s-8)' : 0,
          animation: 'pulso 1.4s ease-in-out infinite alternate',
        }} />
      ))}
    </div>
  )
}
