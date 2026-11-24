// T044/T055 — useActores: resuelve actorId → correo con un solo ?ids= (data-model §3, R-015)
import { useQuery } from '@tanstack/react-query'
import { pedirApi } from '@/api/http'

export function useActores(ids: (string | null)[]): Record<string, string> {
  const limpios = [...new Set(ids.filter((id): id is string => id !== null))].sort()

  const { data } = useQuery({
    queryKey: ['actores', limpios.join(',')],
    queryFn: async () => {
      if (!limpios.length) return {}
      const respuesta = await pedirApi(
        `/api/v1/usuarios?ids=${limpios.join(',')}`
      )
      if (!respuesta.ok) return {}
      const json = await respuesta.json() as { elementos: Array<{ id: string; correo: string }> }
      return Object.fromEntries(json.elementos.map(usuario => [usuario.id, usuario.correo]))
    },
    staleTime: 10 * 60_000,
    enabled: limpios.length > 0,
  })

  return data ?? {}
}
