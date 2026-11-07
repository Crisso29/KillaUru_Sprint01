// T066 — DialogoModeracion: confirmación con texto del prototipo (FR-030)
import { Boton } from '@/compartido/ui'
import { TEXTOS_MODERACION as T } from '../textos'
import type { AccionModeracion } from '@/api/generado/003-anomalias'

interface Props {
  accion: AccionModeracion
  idCorto: string
  cargando: boolean
  onConfirmar: () => void
  onCancelar: () => void
}

function tituloDialogo(accion: AccionModeracion, idCorto: string): string {
  if (accion === 'MarcarReparada') return T.confirmarMarcarReparada(idCorto)
  if (accion === 'Descartar') return T.confirmarDescartar(idCorto)
  return T.confirmarReabrir(idCorto)
}

export function DialogoModeracion({ accion, idCorto, cargando, onConfirmar, onCancelar }: Props) {
  return (
    <dialog
      open
      role="dialog"
      aria-modal="true"
      style={{ padding: 'var(--s-21)', minWidth: 320 }}
    >
      <h2 style={{ marginBottom: 'var(--s-13)' }}>{tituloDialogo(accion, idCorto)}</h2>
      <p style={{ marginBottom: 'var(--s-13)' }}>{T.auditoria}</p>
      <div style={{ display: 'flex', gap: 'var(--s-08)', justifyContent: 'flex-end' }}>
        <Boton variante="secundario" onClick={onCancelar} disabled={cargando}>
          {T.cancelar}
        </Boton>
        <Boton variante="primario" onClick={onConfirmar} disabled={cargando}>
          {cargando ? 'Procesando…' : T.confirmar}
        </Boton>
      </div>
    </dialog>
  )
}
