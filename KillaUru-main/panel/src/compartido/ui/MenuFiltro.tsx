import { useEffect, useId, useRef, useState } from 'react'
import { Icono } from './Icono'

interface Opcion { valor: string; etiqueta: string }
interface Props {
  etiqueta: string
  opciones: Opcion[]
  seleccionadas: string[]
  onChange: (vals: string[]) => void
}

/** «Todos», «Ninguno» o las elegidas, para el texto del botón (prototipo «Estado: Activa, Pendiente de confirmar»). */
function resumen(opciones: Opcion[], seleccionadas: string[]): string {
  if (seleccionadas.length === opciones.length) return 'Todos'
  if (seleccionadas.length === 0) return 'Ninguno'
  return opciones.filter(o => seleccionadas.includes(o.valor)).map(o => o.etiqueta).join(', ')
}

/**
 * Filtro desplegable con casillas (prototipo `.fbtn` + `.ddm`). Se cierra con Escape o al pulsar fuera, y devuelve el
 * foco al botón (WCAG 2.1.2: sin trampa de teclado).
 */
export function MenuFiltro({ etiqueta, opciones, seleccionadas, onChange }: Props) {
  const [abierto, setAbierto] = useState(false)
  const raiz = useRef<HTMLDivElement>(null)
  const boton = useRef<HTMLButtonElement>(null)
  const id = useId()
  const toggleTodos = () => onChange(seleccionadas.length === opciones.length ? [] : opciones.map(o => o.valor))
  const toggle = (v: string) =>
    onChange(seleccionadas.includes(v) ? seleccionadas.filter(x => x !== v) : [...seleccionadas, v])

  useEffect(() => {
    if (!abierto) return
    const fuera = (e: MouseEvent) => { if (!raiz.current?.contains(e.target as Node)) setAbierto(false) }
    document.addEventListener('mousedown', fuera)
    return () => document.removeEventListener('mousedown', fuera)
  }, [abierto])

  return (
    <div className="dd" ref={raiz} onKeyDown={e => { if (e.key === 'Escape') { setAbierto(false); boton.current?.focus() } }}>
      <button ref={boton} type="button" className="fbtn" onClick={() => setAbierto(a => !a)}
        aria-expanded={abierto} aria-controls={id}>
        {etiqueta}: <b>{resumen(opciones, seleccionadas)}</b><Icono nombre="chevd" tamano={14} />
      </button>
      {abierto && (
        <div className="ddm" id={id} role="group" aria-label={etiqueta}>
          <label>
            <input type="checkbox" checked={seleccionadas.length === opciones.length} onChange={toggleTodos} />
            Todos
          </label>
          {opciones.map(o => (
            <label key={o.valor}>
              <input type="checkbox" checked={seleccionadas.includes(o.valor)} onChange={() => toggle(o.valor)} />
              {o.etiqueta}
            </label>
          ))}
        </div>
      )}
    </div>
  )
}
