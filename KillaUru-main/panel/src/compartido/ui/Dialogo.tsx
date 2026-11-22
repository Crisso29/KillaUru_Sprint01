import { useEffect, useRef } from 'react'
import { Icono } from './Icono'

interface Props {
  abierto: boolean
  titulo: string
  onCerrar: () => void
  children: React.ReactNode
  acciones?: React.ReactNode
}

export function Dialogo({ abierto, titulo, onCerrar, children, acciones }: Props) {
  const dialogoRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!abierto) return
    dialogoRef.current?.focus()
    const handler = (e: KeyboardEvent) => { if (e.key === 'Escape') onCerrar() }
    document.addEventListener('keydown', handler)
    return () => document.removeEventListener('keydown', handler)
  }, [abierto, onCerrar])

  if (!abierto) return null

  // Prototipo `.veil` + `.modal`: encabezado, cuerpo y pie con las acciones.
  return (
    <div className="veil" style={{ position: 'fixed', zIndex: 100 }}>
      <div onClick={onCerrar} style={{ position: 'absolute', inset: 0 }} aria-hidden="true" />
      <div role="dialog" aria-modal="true" aria-labelledby="dialogo-titulo" ref={dialogoRef} tabIndex={-1}
        className="modal" style={{ position: 'relative' }}>
        <div className="modal-h">
          <h3 id="dialogo-titulo">{titulo}</h3>
        </div>
        <div className="modal-b">{children}</div>
        {acciones && <div className="modal-f" style={{ justifyContent: 'flex-end' }}><div>{acciones}</div></div>}
        <button type="button" className="ob ic" onClick={onCerrar} aria-label="Cerrar"
          style={{ position: 'absolute', top: 'var(--s-8)', right: 'var(--s-8)', border: 0 }}>
          <Icono nombre="close" tamano={16} />
        </button>
      </div>
    </div>
  )
}
