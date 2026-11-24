// Muestra la dirección actual del enrutador para que una prueba compruebe a dónde navegó la pantalla (ruta + consulta).
import { useLocation } from 'react-router'

export const ID_RUTA_ACTUAL = 'ruta-actual'

export function RutaActual() {
  const { pathname, search } = useLocation()
  return <div data-testid={ID_RUTA_ACTUAL}>{pathname}{search}</div>
}
