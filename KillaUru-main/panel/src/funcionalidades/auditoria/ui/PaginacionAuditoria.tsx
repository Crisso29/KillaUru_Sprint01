// T080 — pie de la tabla de auditoría: total y páginas anterior / siguiente
import { TEXTOS_AUDITORIA } from '../textos'

interface Props {
  pagina: number
  total: number
  tamano: number
  onCambiar: (pagina: number) => void
}

export function PaginacionAuditoria({ pagina, total, tamano, onCambiar }: Props) {
  const hayMas = pagina * tamano < total
  return (
    <div className="tfoot">
      <span>{TEXTOS_AUDITORIA.contadorTotal(total)}</span>
      <nav aria-label="Paginación" style={{ display: 'inline-flex', gap: 'var(--s-5)', alignItems: 'center' }}>
        <button type="button" className="ob sm" disabled={pagina <= 1} onClick={() => onCambiar(pagina - 1)}>Anterior</button>
        <span>Página {pagina}</span>
        <button type="button" className="ob sm" disabled={!hayMas} onClick={() => onCambiar(pagina + 1)}>Siguiente</button>
      </nav>
    </div>
  )
}
