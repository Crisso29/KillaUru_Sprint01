interface Props { tamano?: 'sm' | 'md' | 'lg' }

export function Marca({ tamano = 'md' }: Props) {
  const tamanoPx = tamano === 'sm' ? 'var(--t-16)' : tamano === 'lg' ? 'var(--t-34)' : 'var(--t-21)'
  return (
    <span style={{ fontFamily: 'var(--f-display)', fontWeight: 700, fontSize: tamanoPx, letterSpacing: '-0.02em' }}>
      <span style={{ color: 'var(--killa)' }}>Killa</span>
      <span style={{ color: 'var(--luna)' }}>Uru</span>
    </span>
  )
}
