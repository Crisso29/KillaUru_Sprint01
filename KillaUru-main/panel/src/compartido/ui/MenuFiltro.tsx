import { useState } from 'react'

interface Opcion { valor: string; etiqueta: string }
interface Props {
  etiqueta: string
  opciones: Opcion[]
  seleccionadas: string[]
  onChange: (vals: string[]) => void
}

export function MenuFiltro({ etiqueta, opciones, seleccionadas, onChange }: Props) {
  const [abierto, setAbierto] = useState(false)
  const toggleTodos = () => onChange(seleccionadas.length === opciones.length ? [] : opciones.map(o => o.valor))
  const toggle = (v: string) =>
    onChange(seleccionadas.includes(v) ? seleccionadas.filter(x => x !== v) : [...seleccionadas, v])

  return (
    <div style={{ position: 'relative' }}>
      <button onClick={() => setAbierto(a => !a)} aria-expanded={abierto} style={{ background: 'var(--surface)', border: '1px solid var(--line-2)', borderRadius: 'var(--s-5)', padding: 'var(--s-5) var(--s-13)', cursor: 'pointer' }}>
        {etiqueta}
      </button>
      {abierto && (
        <div style={{ position: 'absolute', top: '110%', left: 0, background: 'var(--surface)', border: '1px solid var(--line)', borderRadius: 'var(--s-5)', boxShadow: 'var(--shadow)', zIndex: 20, minWidth: 160, padding: 'var(--s-8)' }}>
          <label style={{ display: 'flex', gap: 'var(--s-8)', padding: 'var(--s-5) 0', cursor: 'pointer' }}>
            <input type="checkbox" checked={seleccionadas.length === opciones.length} onChange={toggleTodos} />
            Todos
          </label>
          {opciones.map(o => (
            <label key={o.valor} style={{ display: 'flex', gap: 'var(--s-8)', padding: 'var(--s-5) 0', cursor: 'pointer' }}>
              <input type="checkbox" checked={seleccionadas.includes(o.valor)} onChange={() => toggle(o.valor)} />
              {o.etiqueta}
            </label>
          ))}
        </div>
      )}
    </div>
  )
}
