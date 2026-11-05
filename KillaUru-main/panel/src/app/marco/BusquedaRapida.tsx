// T049 — BusquedaRapida: Ctrl+K, navega por id/km/correo (FR-040)
import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router'
import { parsearBusqueda } from '@/funcionalidades/mapa/modelo/busqueda'

export function BusquedaRapida() {
  const [abierto, setAbierto] = useState(false)
  const [texto, setTexto] = useState('')
  const [sinResultados, setSinResultados] = useState(false)
  const inputRef = useRef<HTMLInputElement>(null)
  const navigate = useNavigate()

  useEffect(() => {
    function manejar(e: KeyboardEvent) {
      if (e.ctrlKey && e.key === 'k') {
        e.preventDefault()
        setAbierto(true)
        setTimeout(() => inputRef.current?.focus(), 0)
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
        setAbierto(false)
        break
      case 'id-prefijo':
        void navigate(`/mapa?anomalia=${encodeURIComponent(intento.prefijo)}`)
        setAbierto(false)
        break
      case 'km':
        void navigate(`/mapa?kmVia=${intento.km}`)
        setAbierto(false)
        break
      case 'correo':
        void navigate(`/usuarios?busqueda=${encodeURIComponent(intento.correo)}`)
        setAbierto(false)
        break
      default:
        setSinResultados(true)
    }
  }

  if (!abierto) {
    return (
      <button type="button" onClick={() => setAbierto(true)} aria-keyshortcuts="Ctrl+K" style={{ fontSize: 'var(--t-13)' }}>
        Buscar (Ctrl+K)
      </button>
    )
  }

  return (
    <div role="search" style={{ display: 'flex', flexDirection: 'column', gap: 'var(--s-5)' }}>
      <input
        ref={inputRef}
        role="searchbox"
        type="text"
        placeholder="ID, km o correo…"
        value={texto}
        onChange={e => { setTexto(e.target.value); setSinResultados(false) }}
        onKeyDown={e => { if (e.key === 'Enter') navegar() }}
        aria-label="Búsqueda rápida"
      />
      {sinResultados && (
        <p role="status" style={{ fontSize: 'var(--t-13)', color: 'var(--ink-3)' }}>
          Sin resultados para «{texto}»
        </p>
      )}
    </div>
  )
}
