// T084 — PaginaExportacion: US7 Exportación del dataset con 11 columnas y sección recientes
import { FormularioExportacion } from './FormularioExportacion'
import { ExportacionesRecientes } from './ExportacionesRecientes'
import { TEXTOS_EXPORTACION as T } from '../textos'

const COLUMNAS = [
  'id', 'tipo', 'severidad', 'estado', 'persistencia',
  'km', 'latitud', 'longitud', 'reportes', 'primer_reporte', 'ultimo_reporte',
]

interface Props { correoActual: string }

export function PaginaExportacion({ correoActual }: Props) {
  return (
    <div style={{ padding: 'var(--s-21) var(--s-34)' }}>
      <h1 style={{ fontSize: 'var(--t-21)', fontWeight: 700, margin: '0 0 var(--s-8)' }}>{T.titulo}</h1>
      <p style={{ color: 'var(--ink-3)', marginBottom: 'var(--s-21)' }}>{T.subtitulo}</p>

      <section style={{ marginBottom: 'var(--s-34)' }}>
        <h2 style={{ fontSize: 'var(--t-16)', fontWeight: 600, marginBottom: 'var(--s-13)' }}>{T.columnasTitulo}</h2>
        <ul style={{ display: 'flex', flexWrap: 'wrap', gap: 'var(--s-5)', listStyle: 'none', padding: 0, margin: '0 0 var(--s-8)' }}>
          {COLUMNAS.map(col => (
            <li key={col} style={{ background: 'var(--surface-2)', padding: 'var(--s-5) var(--s-8)', borderRadius: 'var(--r-5)', fontSize: 'var(--t-13)' }}>
              {col}
            </li>
          ))}
          <li style={{ padding: 'var(--s-5) var(--s-8)', fontSize: 'var(--t-13)' }}>
            <span
              data-tachado="true"
              style={{ textDecoration: 'line-through', color: 'var(--ink-3)' }}
            >
              seudónimo
            </span>
          </li>
        </ul>
        <p style={{ fontSize: 'var(--t-13)', color: 'var(--ink-3)' }}>{T.seudonimoNota}</p>
      </section>

      <section style={{ marginBottom: 'var(--s-34)' }}>
        <FormularioExportacion />
      </section>

      <section>
        <h2 style={{ fontSize: 'var(--t-16)', fontWeight: 600, marginBottom: 'var(--s-13)' }}>{T.recientesTitulo}</h2>
        <ExportacionesRecientes correoActual={correoActual} />
      </section>
    </div>
  )
}
