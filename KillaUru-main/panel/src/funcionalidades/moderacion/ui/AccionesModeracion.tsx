// T066 — AccionesModeracion: botones RN-07 con tooltip y confirmación (FR-027, FR-030)
import { useState } from 'react'
import { Boton } from '@/compartido/ui'
import { accionesDisponibles, esAccionDisponible, textoNoDisponible } from '../modelo/acciones'
import { useModerar } from '../api/useModerar'
import { DialogoModeracion } from './DialogoModeracion'
import { TEXTOS_MODERACION as T } from '../textos'
import type { DetalleAnomalia, AccionModeracion } from '@/api/dtos/003-anomalias'

const TODAS_ACCIONES: AccionModeracion[] = ['MarcarReparada', 'Descartar', 'Reabrir', 'MantenerDescartada']

const ETIQUETAS: Record<AccionModeracion, string> = {
  MarcarReparada: T.accionMarcarReparada,
  Descartar: T.accionDescartar,
  Reabrir: T.accionReabrir,
  MantenerDescartada: T.accionMantenerDescartada,
}

interface Props {
  detalle: DetalleAnomalia
  onModerado: (actualizado: DetalleAnomalia) => void
}

export function AccionesModeracion({ detalle, onModerado }: Props) {
  const [accionPendiente, setAccionPendiente] = useState<AccionModeracion | null>(null)
  const [errorMensaje, setErrorMensaje] = useState<string | null>(null)
  const moderar = useModerar()

  const idCorto = detalle.id.slice(0, 8)
  const disponibles = accionesDisponibles(detalle.estado, detalle.porRevisar)

  async function confirmar() {
    if (!accionPendiente) return
    setErrorMensaje(null)
    try {
      const actualizado = await moderar.mutateAsync({ id: detalle.id, accion: accionPendiente })
      setAccionPendiente(null)
      onModerado(actualizado)
    } catch (e) {
      const err = e as { codigo?: string }
      if (err.codigo === 'transicion_no_permitida') {
        setErrorMensaje(T.transicionNoPermitida)
      }
      setAccionPendiente(null)
    }
  }

  return (
    <section aria-label="Acciones de moderación" style={{ marginTop: 'var(--s-13)' }}>
      {errorMensaje && <p role="alert" style={{ marginBottom: 'var(--s-08)' }}>{errorMensaje}</p>}
      <div style={{ display: 'flex', gap: 'var(--s-08)', flexWrap: 'wrap' }}>
        {TODAS_ACCIONES.map((accion) => {
          const disponible = esAccionDisponible(accion, detalle.estado, detalle.porRevisar)
          return (
            <Boton
              key={accion}
              variante={disponibles.includes(accion) ? 'primario' : 'secundario'}
              onClick={() => disponible && setAccionPendiente(accion)}
              disabled={!disponible}
              title={!disponible ? textoNoDisponible(accion, detalle.estado) : undefined}
            >
              {ETIQUETAS[accion]}
            </Boton>
          )
        })}
      </div>
      {accionPendiente && (
        <DialogoModeracion
          accion={accionPendiente}
          idCorto={idCorto}
          cargando={moderar.isPending}
          onConfirmar={confirmar}
          onCancelar={() => setAccionPendiente(null)}
        />
      )}
    </section>
  )
}
