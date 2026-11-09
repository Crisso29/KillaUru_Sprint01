import { useEffect, useRef } from 'react'
import { Boton } from './Boton'

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

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="dialogo-titulo"
      ref={dialogoRef}
      tabIndex={-1}
      style={{ position: 'fixed', inset: 0, zIndex: 100, display: 'flex', alignItems: 'center', justifyContent: 'center' }}
    >
      <div onClick={onCerrar} style={{ position: 'absolute', inset: 0, background: 'var(--shade)' }} aria-hidden="true" />
      <div style={{ position: 'relative', background: 'var(--surface)', borderRadius: 'var(--s-8)', padding: 'var(--s-21)', minWidth: 320, maxWidth: 480 }}>
        <h2 id="dialogo-titulo" style={{ fontSize: 'var(--t-21)', marginBottom: 'var(--s-13)' }}>{titulo}</h2>
        <div>{children}</div>
        {acciones && <div style={{ marginTop: 'var(--s-21)', display: 'flex', gap: 'var(--s-8)', justifyContent: 'flex-end' }}>{acciones}</div>}
        <Boton variante="fantasma" onClick={onCerrar} aria-label="Cerrar" style={{ position: 'absolute', top: 'var(--s-8)', right: 'var(--s-8)' }}>✕</Boton>
      </div>
    </div>
  )
}
