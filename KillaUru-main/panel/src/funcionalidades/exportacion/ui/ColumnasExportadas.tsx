// T084 — Las 11 columnas del dataset y el seudónimo tachado: se ve que nunca sale (RNF-10; prototipo `.cols`)
import { TEXTOS_EXPORTACION as T } from '../textos'

export const COLUMNAS = [
  'id', 'tipo', 'severidad', 'estado', 'persistencia',
  'km', 'latitud', 'longitud', 'reportes', 'primer_reporte', 'ultimo_reporte',
] as const

export function ColumnasExportadas() {
  return (
    <section aria-label={T.columnasTitulo} style={{ display: 'grid', gap: 'var(--s-5)' }}>
      <h4 className="hint" style={{ fontFamily: 'var(--f-body)', fontWeight: 600 }}>{T.columnasTitulo}</h4>
      <ul className="cols" style={{ listStyle: 'none', padding: 0, margin: 0 }}>
        {COLUMNAS.map(col => <li key={col}><code>{col}</code></li>)}
        <li><code className="no" data-tachado="true" aria-label="seudónimo (nunca se exporta)">seudónimo</code></li>
      </ul>
    </section>
  )
}
