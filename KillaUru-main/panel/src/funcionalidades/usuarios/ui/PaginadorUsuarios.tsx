// T056 — pie de la tabla de usuarios: cuántos se ven y, si hay más de una página, anterior / siguiente
import { TEXTOS_USUARIOS as textos } from '../textos'

interface Props { visibles: number; total: number; tamano: number; pagina: number; onCambiar: (pagina: number) => void }

export function PaginadorUsuarios({ visibles, total, tamano, pagina, onCambiar }: Props) {
  return (
    <div className="tfoot">
      <span>{visibles} {textos.de} {total} {textos.usuariosLabel}</span>
      {total > tamano ? (
        <span style={{ display: 'inline-flex', gap: 'var(--s-5)', alignItems: 'center' }}>
          <button type="button" className="ob sm" aria-label="Página anterior" disabled={pagina <= 1} onClick={() => onCambiar(pagina - 1)}>‹</button>
          <span>Página {pagina}</span>
          <button type="button" className="ob sm" aria-label="Página siguiente" disabled={pagina * tamano >= total} onClick={() => onCambiar(pagina + 1)}>›</button>
        </span>
      ) : <span>{textos.pieTabla}</span>}
    </div>
  )
}
