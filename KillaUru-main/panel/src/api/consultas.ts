// T022 — tiempos de TanStack Query (data-model.md §3)
import {
  REFRESCO_OPERATIVO_S,
  CACHE_LECTURA_AUDITADA_MIN,
} from '@/config/constantes'

const S = 1_000
const MIN = 60 * S

export const TIEMPOS = {
  operativo: {
    staleTime: 30 * S,
    refetchInterval: REFRESCO_OPERATIVO_S * S,
    refetchOnWindowFocus: true,
  },
  auditado: {
    staleTime: CACHE_LECTURA_AUDITADA_MIN * MIN,
    refetchInterval: false as const,
    refetchOnWindowFocus: false,
  },
  me: {
    staleTime: Infinity,
    refetchInterval: false as const,
    refetchOnWindowFocus: false,
  },
} as const
