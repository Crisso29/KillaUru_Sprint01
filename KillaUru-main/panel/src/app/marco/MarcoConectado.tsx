import { Outlet, useLocation, useNavigate } from 'react-router'
import { useSesion } from '@/sesion/useSesion'
import { useMisDatos } from '@/sesion/useMisDatos'
import { usePorRevisar } from '@/funcionalidades/moderacion'
import { entorno } from '@/config/entorno'
import { Marco } from './Marco'

/**
 * Une el [Marco] con los datos reales: la sesión activa, cuántas anomalías esperan revisión y el entorno. Es la ruta de
 * diseño de todas las páginas protegidas (`<Outlet />`), así el menú no se vuelve a montar al cambiar de página.
 */
export function MarcoConectado() {
  const { sesion, cerrarSesion } = useSesion()
  const { data: porRevisar } = usePorRevisar()
  const { data: misDatos } = useMisDatos(sesion.tipo === 'activa')
  const { pathname } = useLocation()
  const navigate = useNavigate()
  if (sesion.tipo !== 'activa') return null // RutaProtegida ya redirige; esto solo satisface el tipo

  const salir = async () => {
    await cerrarSesion()
    void navigate('/login', { replace: true })
  }

  return (
    <Marco
      sesion={{ ...sesion, correo: misDatos?.correo ?? sesion.correo }}
      porRevisar={porRevisar?.length ?? 0}
      onSalir={() => void salir()}
      entorno={entorno.entorno}
      urlEstado={entorno.urlEstado}
      completo={pathname.startsWith('/mapa')}
    >
      <Outlet />
    </Marco>
  )
}
