import { useEffect, useId, useRef, useState, type RefObject } from 'react'
import { Icono } from './Icono'

interface Opcion { valor: string; etiqueta: string }
interface Props {
  etiqueta: string
  opciones: Opcion[]
  seleccionadas: string[]
  onChange: (valores: string[]) => void
}

/** «Todos», «Ninguno» o las elegidas, para el texto del botón (prototipo «Estado: Activa, Pendiente de confirmar»). */
function resumen(opciones: Opcion[], seleccionadas: string[]): string {
  if (seleccionadas.length === opciones.length) return 'Todos'
  if (seleccionadas.length === 0) return 'Ninguno'
  return opciones.filter(opcion => seleccionadas.includes(opcion.valor)).map(opcion => opcion.etiqueta).join(', ')
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
  useCerrarAlPulsarFuera(abierto, raiz, () => setAbierto(false))
  const alTeclear = (evento: React.KeyboardEvent) => { if (evento.key === 'Escape') { setAbierto(false); boton.current?.focus() } }
  return (
    <div className="dd" ref={raiz} onKeyDown={alTeclear}>
      <button ref={boton} type="button" className="fbtn" onClick={() => setAbierto((actual) => !actual)} aria-expanded={abierto} aria-controls={id}>
        {etiqueta}: <b>{resumen(opciones, seleccionadas)}</b><Icono nombre="chevd" tamano={14} />
      </button>
      {abierto && <Casillas id={id} etiqueta={etiqueta} opciones={opciones} seleccionadas={seleccionadas} onChange={onChange} />}
    </div>
  )
}

function Casillas({ id, etiqueta, opciones, seleccionadas, onChange }: Props & { id: string }) {
  const todas = seleccionadas.length === opciones.length
  const alternar = (valor: string) =>
    onChange(seleccionadas.includes(valor) ? seleccionadas.filter((elegida) => elegida !== valor) : [...seleccionadas, valor])
  return (
    <div className="ddm" id={id} role="group" aria-label={etiqueta}>
      <label><input type="checkbox" checked={todas} onChange={() => onChange(todas ? [] : opciones.map((opcion) => opcion.valor))} />Todos</label>
      {opciones.map((opcion) => (
        <label key={opcion.valor}>
          <input type="checkbox" checked={seleccionadas.includes(opcion.valor)} onChange={() => alternar(opcion.valor)} />
          {opcion.etiqueta}
        </label>
      ))}
    </div>
  )
}

function useCerrarAlPulsarFuera(abierto: boolean, raiz: RefObject<HTMLDivElement | null>, cerrar: () => void) {
  useEffect(() => {
    if (!abierto) return
    const alPulsar = (evento: MouseEvent) => { if (!raiz.current?.contains(evento.target as Node)) cerrar() }
    document.addEventListener('mousedown', alPulsar)
    return () => document.removeEventListener('mousedown', alPulsar)
  }, [abierto, raiz, cerrar])
}
