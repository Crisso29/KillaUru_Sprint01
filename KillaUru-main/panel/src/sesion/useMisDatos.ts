import { useQuery } from '@tanstack/react-query'
import type { components } from '@/api/generado/identidad'
import { entorno } from '@/config/entorno'

/** `MisDatos` del contrato 003 (`GET /api/v1/me`): de aquí sale el correo que muestra el armazón. */
export type MisDatos = components['schemas']['MisDatos']

/**
 * Datos de la cuenta con sesión. La respuesta de inicio de sesión (`SesionPanel`) no trae el correo, y tras recargar la
 * página la sesión se recupera solo con la cookie: por eso el correo se pide aquí y se guarda en caché 10 min.
 */
export function useMisDatos(activa: boolean) {
  return useQuery<MisDatos>({
    queryKey: ['mis-datos'],
    enabled: activa,
    staleTime: 10 * 60_000,
    queryFn: async () => {
      const r = await fetch(`${entorno.urlApi}/api/v1/me`, { credentials: 'include' })
      if (!r.ok) throw new Error(`GET /me ${r.status}`)
      return r.json() as Promise<MisDatos>
    },
  })
}
