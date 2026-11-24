// T066 — DialogoModeracion: confirmación con texto del prototipo (FR-030). Usa el diálogo común del panel: el foco
// entra al abrirse (WCAG 2.4.3), Escape cancela y el título le da nombre accesible.
import { Boton, Dialogo } from '@/compartido/ui'
import { TEXTOS_MODERACION as textos } from '../textos'
import type { AccionModeracion } from '@/api/dtos/003-anomalias'

interface Props {
  accion: AccionModeracion
  idCorto: string
  cargando: boolean
  onConfirmar: () => void
  onCancelar: () => void
}

function tituloDialogo(accion: AccionModeracion, idCorto: string): string {
  if (accion === 'MarcarReparada') return textos.confirmarMarcarReparada(idCorto)
  if (accion === 'Descartar') return textos.confirmarDescartar(idCorto)
  return textos.confirmarReabrir(idCorto)
}

export function DialogoModeracion({ accion, idCorto, cargando, onConfirmar, onCancelar }: Props) {
  const acciones = (
    <>
      <Boton variante="secundario" onClick={onCancelar} disabled={cargando}>{textos.cancelar}</Boton>
      <Boton variante="primario" onClick={onConfirmar} disabled={cargando}>{cargando ? 'Procesando…' : textos.confirmar}</Boton>
    </>
  )
  return (
    <Dialogo abierto titulo={tituloDialogo(accion, idCorto)} onCerrar={onCancelar} acciones={acciones}>
      <p>{textos.auditoria}</p>
    </Dialogo>
  )
}
