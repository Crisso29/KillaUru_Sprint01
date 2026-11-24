// T066 — AccionesModeracion: botones RN-07 con tooltip y confirmación (FR-027, FR-030)
import { Boton } from '@/compartido/ui'
import { esAccionDisponible, textoNoDisponible } from '../modelo/acciones'
import { useConfirmarModeracion } from './useConfirmarModeracion'
import { DialogoModeracion } from './DialogoModeracion'
import { TEXTOS_MODERACION as textos } from '../textos'
import type { DetalleAnomalia, AccionModeracion } from '@/api/dtos/003-anomalias'

const TODAS_ACCIONES: AccionModeracion[] = ['MarcarReparada', 'Descartar', 'Reabrir', 'MantenerDescartada']

const ETIQUETAS: Record<AccionModeracion, string> = {
  MarcarReparada: textos.accionMarcarReparada,
  Descartar: textos.accionDescartar,
  Reabrir: textos.accionReabrir,
  MantenerDescartada: textos.accionMantenerDescartada,
}

interface Props {
  detalle: DetalleAnomalia
  onModerado: (actualizado: DetalleAnomalia) => void
}

export function AccionesModeracion({ detalle, onModerado }: Props) {
  const flujo = useConfirmarModeracion(detalle, onModerado)
  return (
    <section aria-label="Acciones de moderación" style={{ display: 'grid', gap: 'var(--s-8)' }}>
      {flujo.errorMensaje && <p role="alert" className="note bad">{flujo.errorMensaje}</p>}
      <div style={{ display: 'flex', gap: 'var(--s-8)', flexWrap: 'wrap' }}>
        {TODAS_ACCIONES.map((accion) => <BotonAccion key={accion} accion={accion} detalle={detalle} onElegir={flujo.elegir} />)}
      </div>
      {flujo.accionPendiente && <DialogoModeracion accion={flujo.accionPendiente} idCorto={detalle.id.slice(0, 8)}
        cargando={flujo.cargando} onConfirmar={flujo.confirmar} onCancelar={flujo.cancelar} />}
    </section>
  )
}

/** RN-07: una acción que no corresponde al estado se ve deshabilitada y explica por qué (tooltip). */
function BotonAccion({ accion, detalle, onElegir }: { accion: AccionModeracion; detalle: DetalleAnomalia; onElegir: (accion: AccionModeracion) => void }) {
  const disponible = esAccionDisponible(accion, detalle.estado, detalle.porRevisar)
  const peligrosa = accion === 'Descartar'
  return (
    <Boton variante={!disponible || peligrosa ? 'secundario' : 'primario'} className={peligrosa && disponible ? 'dng' : undefined}
      onClick={() => disponible && onElegir(accion)} disabled={!disponible}
      title={!disponible ? textoNoDisponible(accion, detalle.estado) : undefined}>
      {ETIQUETAS[accion]}
    </Boton>
  )
}
