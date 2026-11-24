// T067 — piezas comunes de las tablas de Moderación (prototipo `.dt`): cabecera, celda de la anomalía y pie
import { Severidad } from '@/compartido/ui'

export interface Columna { texto: string; derecha?: boolean; oculta?: boolean }

export function Cabecera({ columnas }: { columnas: Columna[] }) {
  return (
    <thead>
      <tr>
        {columnas.map((columna) => (
          <th key={columna.texto} scope="col" className={columna.derecha ? 'r' : undefined}>
            {columna.oculta ? <span className="sr-only">{columna.texto}</span> : columna.texto}
          </th>
        ))}
      </tr>
    </thead>
  )
}

/** Severidad, tipo y los 8 primeros caracteres del id: lo que el moderador usa para reconocer la anomalía. */
export function CeldaAnomalia({ id, tipo, severidad }: { id: string; tipo: string; severidad: number }) {
  return (
    <td>
      <span style={{ display: 'inline-flex', gap: 'var(--s-5)', alignItems: 'center' }}>
        <Severidad nivel={severidad} tamano="sm" />
        <b>{tipo}</b> <span className="mono">{id.slice(0, 8)}</span>
      </span>
    </td>
  )
}

export function PieTabla({ cantidad, reglas }: { cantidad: number; reglas: string }) {
  return <div className="tfoot"><span>{cantidad} {cantidad === 1 ? 'elemento' : 'elementos'}</span><span>{reglas}</span></div>
}
