// T022 — clientes openapi-fetch con middleware de sesión
import createClient from 'openapi-fetch'
import { armarError } from './problema'

export interface OpcionesCliente {
  urlBase: string
  renovar: () => Promise<Date>
  onSesionTerminada: () => void
}

const NO_RENUEVA = new Set(['credenciales_invalidas', 'cuenta_bloqueada', 'solo_administradores'])

export function crearCliente({ urlBase, renovar, onSesionTerminada }: OpcionesCliente) {
  const cliente = createClient<Record<string, unknown>>({
    baseUrl: urlBase,
    credentials: 'include',
  })

  cliente.use({
    async onResponse({ request, response }) {
      if (response.status !== 401) return response
      const error = await armarError(response.clone())

      if (NO_RENUEVA.has(error.codigo)) {
        throw error
      }

      // token_invalido → renovar una vez y reintentar con URL original
      let reintento: Response
      try {
        await renovar()
        reintento = await fetch(new Request(request.url, { method: request.method, credentials: 'include', headers: request.headers }))
      } catch (err) {
        onSesionTerminada()
        throw err
      }
      if (reintento.status === 401) {
        onSesionTerminada()
        throw await armarError(reintento)
      }
      return reintento
    },
  })

  return cliente
}
