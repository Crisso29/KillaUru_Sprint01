// T044/T055 — useActores: resuelve actorId → correo con un solo ?ids= (data-model §3, R-015)
import { useQuery } from '@tanstack/react-query'
import { entorno } from '@/config/entorno'

export function useActores(ids: (string | null)[]): Record<string, string> {
  const limpios = [...new Set(ids.filter((id): id is string => id !== null))].sort()

  const { data } = useQuery({
    queryKey: ['actores', limpios.join(',')],
    queryFn: async () => {
      if (!limpios.length) return {}
      const r = await fetch(
        `${entorno.urlApi}/api/v1/usuarios?ids=${limpios.join(',')}`,
        { credentials: 'include' },
      )
      if (!r.ok) return {}
      const json = await r.json() as { elementos: Array<{ id: string; correo: string }> }
      return Object.fromEntries(json.elementos.map(u => [u.id, u.correo]))
    },
    staleTime: 10 * 60_000,
    enabled: limpios.length > 0,
  })

  return data ?? {}
}
