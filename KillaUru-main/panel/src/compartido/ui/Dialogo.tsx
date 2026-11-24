import { useEffect, useId, useRef } from 'react'
import { Icono } from './Icono'

interface Props {
  abierto: boolean
  titulo: string
  onCerrar: () => void
  children: React.ReactNode
  acciones?: React.ReactNode
}

export function Dialogo({ abierto, titulo, onCerrar, children, acciones }: Props) {
  const dialogoRef = useEnfocarYCerrarConEscape(abierto, onCerrar)
  const idTitulo = useId() // único: el título nombra a SU diálogo aunque haya otro montado
  if (!abierto) return null
  // Prototipo `.veil` + `.modal`: encabezado, cuerpo y pie con las acciones.
  return (
    <div className="veil" style={{ position: 'fixed', zIndex: 100 }}>
      <div onClick={onCerrar} style={{ position: 'absolute', inset: 0 }} aria-hidden="true" />
      <div role="dialog" aria-modal="true" aria-labelledby={idTitulo} ref={dialogoRef} tabIndex={-1} className="modal" style={{ position: 'relative' }}>
        <div className="modal-h"><h3 id={idTitulo}>{titulo}</h3></div>
        <div className="modal-b">{children}</div>
        {acciones && <div className="modal-f" style={{ justifyContent: 'flex-end' }}><div>{acciones}</div></div>}
        <button type="button" className="ob ic" onClick={onCerrar} aria-label="Cerrar" style={{ position: 'absolute', top: 'var(--s-8)', right: 'var(--s-8)', border: 0 }}>
          <Icono nombre="close" tamano={16} />
        </button>
      </div>
    </div>
  )
}

/**
 * Al abrir, el foco entra al diálogo (lo anuncia el lector de pantalla) y Escape lo cierra. El efecto depende solo de
 * `abierto`: si dependiera de `onCerrar`, un padre que crea esa función en cada render devolvería el foco al diálogo
 * con cada tecla y lo escrito en sus campos se perdería.
 */
function useEnfocarYCerrarConEscape(abierto: boolean, onCerrar: () => void) {
  const dialogoRef = useRef<HTMLDivElement>(null)
  const cerrar = useRef(onCerrar)
  cerrar.current = onCerrar
  useEffect(() => {
    if (!abierto) return
    dialogoRef.current?.focus()
    const alTeclear = (evento: KeyboardEvent) => { if (evento.key === 'Escape') cerrar.current() }
    document.addEventListener('keydown', alTeclear)
    return () => document.removeEventListener('keydown', alTeclear)
  }, [abierto])
  return dialogoRef
}
