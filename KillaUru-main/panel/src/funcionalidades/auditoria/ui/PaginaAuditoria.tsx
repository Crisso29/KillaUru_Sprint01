import { useState, useMemo } from 'react'
import { useQuery } from '@tanstack/react-query'
import { TEXTOS_AUDITORIA } from '../textos'
import { FiltrosAuditoria, type ValoresFiltros } from './FiltrosAuditoria'
import { TablaAuditoria } from './TablaAuditoria'
import { detectarTipoActor } from '../modelo/actor'
import { useAuditoria } from '../api/useAuditoria'
import { entorno } from '@/config/entorno'
import type { PaginaUsuarios } from '@/api/generado/001-identidad'

const FILTROS_INICIALES: ValoresFiltros = {
  desde: '', hasta: '', categoria: '', accion: '', actor: '',
}

export function PaginaAuditoria() {
  const T = TEXTOS_AUDITORIA
  const [filtros, setFiltros] = useState<ValoresFiltros>(FILTROS_INICIALES)
  const [pagina, setPagina] = useState(1)

  const tipoActor = detectarTipoActor(filtros.actor)

  // Resolver correo → actorId via búsqueda
  const { data: busquedaActores } = useQuery<PaginaUsuarios>({
    queryKey: ['actores', 'busqueda', filtros.actor],
    queryFn: async () => {
      const r = await fetch(
        `${entorno.urlApi}/api/v1/usuarios?busqueda=${encodeURIComponent(filtros.actor)}`,
        { credentials: 'include' },
      )
      if (!r.ok) throw new Error('error_red')
      return r.json() as Promise<PaginaUsuarios>
    },
    enabled: tipoActor.tipo === 'correo',
    staleTime: 10 * 60_000,
    refetchOnWindowFocus: false,
  })

  const actorResuelto = useMemo(() => {
    if (!filtros.actor) return undefined
    if (tipoActor.tipo === 'id') return tipoActor.valor
    if (tipoActor.tipo === 'correo') {
      const encontrado = busquedaActores?.elementos.find(
        u => u.correo.toLowerCase() === filtros.actor.toLowerCase()
      )
      return encontrado?.id
    }
    return undefined
  }, [filtros.actor, tipoActor, busquedaActores])

  const sinCuenta = filtros.actor && tipoActor.tipo === 'correo' && busquedaActores && !actorResuelto

  const { data, isLoading, isError } = useAuditoria({
    desde: filtros.desde || undefined,
    hasta: filtros.hasta || undefined,
    categoria: filtros.categoria || undefined,
    accion: filtros.accion || undefined,
    actorId: actorResuelto,
    pagina,
  })

  // Resolver actorIds → correos con una sola petición ?ids=
  const actorIds = useMemo(() => {
    if (!data) return []
    return [...new Set(data.elementos.map(r => r.actorId).filter(Boolean) as string[])]
  }, [data])

  const { data: actoresPagina } = useQuery<PaginaUsuarios>({
    queryKey: ['actores', 'ids', actorIds.join(',')],
    queryFn: async () => {
      const r = await fetch(
        `${entorno.urlApi}/api/v1/usuarios?ids=${actorIds.join(',')}`,
        { credentials: 'include' },
      )
      if (!r.ok) throw new Error('error_red')
      return r.json() as Promise<PaginaUsuarios>
    },
    enabled: actorIds.length > 0,
    staleTime: 10 * 60_000,
    refetchOnWindowFocus: false,
  })

  const correosActores = useMemo(() => {
    const mapa = new Map<string, string | null>()
    actorIds.forEach(id => mapa.set(id, null))
    actoresPagina?.elementos.forEach(u => mapa.set(u.id, u.correo))
    return mapa
  }, [actorIds, actoresPagina])

  function handleFiltrosChange(v: ValoresFiltros) {
    setFiltros(v)
    setPagina(1)
  }

  return (
    <main>
      <header>
        <h1>{T.titulo}</h1>
        <span>{T.insignia}</span>
      </header>

      <FiltrosAuditoria valores={filtros} onChange={handleFiltrosChange} />

      {sinCuenta && <p role="alert">{T.sinCuenta}</p>}

      {isLoading && <p>{T.cargando}</p>}
      {isError && <p>{T.error}</p>}

      {data && (
        <>
          <p>{T.contadorTotal(data.total)}</p>
          <TablaAuditoria registros={data.elementos} correosActores={correosActores} />
          <nav aria-label="paginación">
            <button disabled={pagina <= 1} onClick={() => setPagina(p => p - 1)}>
              Anterior
            </button>
            <span>Página {pagina}</span>
            <button
              disabled={pagina * (data.tamano || 50) >= data.total}
              onClick={() => setPagina(p => p + 1)}
            >
              Siguiente
            </button>
          </nav>
        </>
      )}
    </main>
  )
}
