interface Props { datos: number[]; alto?: number; ancho?: number; color?: string }

export function Minigrafico({ datos, alto = 32, ancho = 80, color = 'var(--killa)' }: Props) {
  if (!datos.length) return null
  const max = Math.max(...datos, 1)
  const paso = ancho / (datos.length - 1 || 1)
  const puntos = datos.map((v, i) => `${i * paso},${alto - (v / max) * alto}`).join(' ')

  return (
    <svg width={ancho} height={alto} aria-hidden="true" style={{ overflow: 'visible' }}>
      <polyline points={puntos} fill="none" stroke={color} strokeWidth={1.5} />
    </svg>
  )
}
