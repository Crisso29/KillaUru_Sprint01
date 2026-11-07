import { Boton } from './Boton'

interface Props {
  pagina: number
  total: number
  tamano: number
  onChange: (pagina: number) => void
}

export function Paginador({ pagina, total, tamano, onChange }: Props) {
  const totalPaginas = Math.ceil(total / tamano)
  if (totalPaginas <= 1) return null

  return (
    <nav aria-label="Paginación" style={{ display: 'flex', gap: 'var(--s-8)', alignItems: 'center' }}>
      <Boton variante="secundario" disabled={pagina <= 1} onClick={() => onChange(pagina - 1)} aria-label="Página anterior">‹</Boton>
      <span style={{ fontSize: 'var(--t-13)', color: 'var(--ink-2)' }}>
        {pagina} de {totalPaginas}
      </span>
      <Boton variante="secundario" disabled={pagina >= totalPaginas} onClick={() => onChange(pagina + 1)} aria-label="Página siguiente">›</Boton>
    </nav>
  )
}
