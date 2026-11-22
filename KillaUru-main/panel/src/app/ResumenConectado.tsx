import { PaginaResumen, RequiereAtencion, FranjaCorredor } from '@/funcionalidades/resumen'
import { usePorRevisar, usePendientes } from '@/funcionalidades/moderacion'
import { useRevision } from '@/funcionalidades/usuarios'
import { useMapaAdminFranja } from '@/funcionalidades/mapa'

/** Une el Resumen con sus fuentes: moderación, revisión de privilegios y anomalías activas del corredor (FR-019). */
export function ResumenConectado() {
  const { data: porRevisar = [] } = usePorRevisar()
  const { data: pendientes = [] } = usePendientes()
  const { data: revision } = useRevision()
  const { data: activas } = useMapaAdminFranja()
  return (
    <PaginaResumen
      requiereAtencionSlot={<RequiereAtencion porRevisar={porRevisar} pendientes={pendientes} revision={revision} />}
      franjaSlot={<FranjaCorredor anomalias={activas?.features ?? []} />}
    />
  )
}
