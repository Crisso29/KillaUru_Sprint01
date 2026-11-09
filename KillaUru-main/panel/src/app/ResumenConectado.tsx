import { PaginaResumen, RequiereAtencion } from '@/funcionalidades/resumen'
import { usePorRevisar, usePendientes } from '@/funcionalidades/moderacion'
import { useRevision } from '@/funcionalidades/usuarios'

export function ResumenConectado() {
  const { data: porRevisar = [] } = usePorRevisar()
  const { data: pendientes = [] } = usePendientes()
  const { data: revision } = useRevision()
  return (
    <PaginaResumen requiereAtencionSlot={
      <RequiereAtencion porRevisar={porRevisar} pendientes={pendientes} revision={revision} />
    } />
  )
}
