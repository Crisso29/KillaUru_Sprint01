// T049 — BusquedaRapida: campo de la barra superior (prototipo `.search`); Ctrl+K lo enfoca; navega por id/km/correo (FR-040)
import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router'
import { parsearBusqueda } from '@/funcionalidades/mapa'
import { Icono } from '@/compartido/ui'

export function BusquedaRapida() {
  const [texto, setTexto] = useState('')
  const [sinResultados, setSinResultados] = useState(false)
  const inputRef = useRef<HTMLInputElement>(null)
  const navigate = useNavigate()

  useEffect(() => {
    function manejar(e: KeyboardEvent) {
      if (e.ctrlKey && e.key.toLowerCase() === 'k') {
        e.preventDefault()
        inputRef.current?.focus()
      }
    }
    document.addEventListener('keydown', manejar)
    return () => document.removeEventListener('keydown', manejar)
  }, [])

  function navegar() {
    const intento = parsearBusqueda(texto)
    setSinResultados(false)
    switch (intento.tipo) {
      case 'id':
        void navigate(`/mapa?anomalia=${encodeURIComponent(intento.id)}`)
        break
      case 'id-prefijo':
        void navigate(`/mapa?anomalia=${encodeURIComponent(intento.prefijo)}`)
        break
      case 'km':
        void navigate(`/mapa?kmVia=${intento.km}`)
        break
      case 'correo':
        void navigate(`/usuarios?busqueda=${encodeURIComponent(intento.correo)}`)
        break
      default:
        setSinResultados(true)
    }
  }

  return (
    <div role="search" style={{ position: 'relative', flex: 1, maxWidth: 377, marginLeft: 'var(--s-21)' }}>
      <label className="search" style={{ margin: 0, maxWidth: 'none' }}>
        <Icono nombre="search" tamano={14} />
        <input
          ref={inputRef}
          role="searchbox"
          type="text"
          placeholder="Ir a anomalía, km o correo…"
          value={texto}
          onChange={e => { setTexto(e.target.value); setSinResultados(false) }}
          onKeyDown={e => { if (e.key === 'Enter') navegar() }}
          aria-label="Búsqueda rápida"
          aria-keyshortcuts="Control+K"
        />
        <kbd>Ctrl K</kbd>
      </label>
      {sinResultados && (
        <p role="status" className="hint" style={{ position: 'absolute', top: 34, left: 0, background: 'var(--surface)', border: '1px solid var(--line)', borderRadius: 6, padding: 'var(--s-5) var(--s-8)', boxShadow: 'var(--shadow)', zIndex: 20 }}>
          Sin resultados para «{texto}»
        </p>
      )}
    </div>
  )
}
