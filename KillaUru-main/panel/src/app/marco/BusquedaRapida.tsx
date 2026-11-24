// T049 — BusquedaRapida: campo de la barra superior (prototipo `.search`); Ctrl+K lo enfoca; navega por id/km/correo (FR-040)
import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router'
import { parsearBusqueda } from '@/funcionalidades/mapa'
import { Icono } from '@/compartido/ui'
import { rutaDeBusqueda } from './rutaBusqueda'

const ESTILO_AVISO: React.CSSProperties = {
  position: 'absolute', top: 34, left: 0, background: 'var(--surface)', border: '1px solid var(--line)', borderRadius: 6,
  padding: 'var(--s-5) var(--s-8)', boxShadow: 'var(--shadow)', zIndex: 20,
}

export function BusquedaRapida() {
  const { texto, cambiar, sinResultados, buscar } = useBusqueda()
  const campo = useAtajoEnfocar()
  return (
    <div role="search" style={{ position: 'relative', flex: 1, maxWidth: 377, marginLeft: 'var(--s-21)' }}>
      <label className="search" style={{ margin: 0, maxWidth: 'none' }}>
        <Icono nombre="search" tamano={14} />
        <input ref={campo} role="searchbox" type="text" placeholder="Ir a anomalía, km o correo…" value={texto}
          onChange={(evento) => cambiar(evento.target.value)}
          onKeyDown={(evento) => { if (evento.key === 'Enter') buscar() }} aria-label="Búsqueda rápida" aria-keyshortcuts="Control+K" />
        <kbd>Ctrl K</kbd>
      </label>
      {sinResultados && <p role="status" className="hint" style={ESTILO_AVISO}>Sin resultados para «{texto}»</p>}
    </div>
  )
}

/** Enter abre la pantalla del id, km o correo; si no se reconoce, avisa hasta que se cambie el texto. */
function useBusqueda() {
  const [texto, setTexto] = useState('')
  const [sinResultados, setSinResultados] = useState(false)
  const navigate = useNavigate()
  const cambiar = (nuevo: string) => { setTexto(nuevo); setSinResultados(false) }
  const buscar = () => {
    const ruta = rutaDeBusqueda(parsearBusqueda(texto))
    setSinResultados(ruta === null)
    if (ruta) void navigate(ruta)
  }
  return { texto, cambiar, sinResultados, buscar }
}

/** Ctrl+K enfoca el campo desde cualquier pantalla (se anuncia con `aria-keyshortcuts`). */
function useAtajoEnfocar() {
  const campo = useRef<HTMLInputElement>(null)
  useEffect(() => {
    const alPresionar = (evento: KeyboardEvent) => {
      if (!evento.ctrlKey || evento.key.toLowerCase() !== 'k') return
      evento.preventDefault()
      campo.current?.focus()
    }
    document.addEventListener('keydown', alPresionar)
    return () => document.removeEventListener('keydown', alPresionar)
  }, [])
  return campo
}
