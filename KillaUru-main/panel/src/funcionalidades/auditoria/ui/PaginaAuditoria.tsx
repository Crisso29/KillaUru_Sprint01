import { useState, useMemo } from 'react'
import { useQuery } from '@tanstack/react-query'
import { TEXTOS_AUDITORIA } from '../textos'
import { Esqueleto, Icono } from '@/compartido/ui'
import { FiltrosAuditoria, type ValoresFiltros } from './FiltrosAuditoria'
import { TablaAuditoria } from './TablaAuditoria'
import { detectarTipoActor } from '../modelo/actor'
import { useAuditoria } from '../api/useAuditoria'
import { entorno } from '@/config/entorno'
import type { PaginaUsuarios } from '@/api/dtos/001-identidad'

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
    <>
      <div className="pg-h">
        <div><h2>{T.titulo}</h2><p>Registro de acciones sensibles. Inmutable: nadie puede editarlo ni borrarlo.</p></div>
        <span className="badge expirada"><Icono nombre="lock" tamano={12} />{T.insignia}</span>
      </div>

      <FiltrosAuditoria valores={filtros} onChange={handleFiltrosChange} />

      {sinCuenta && <p role="alert" className="note warn">{T.sinCuenta}</p>}
      {isLoading && <Esqueleto filas={5} alto={40} />}
      {isError && <p role="alert" className="note bad">{T.error}</p>}

      {data && (
        <div className="card">
          <TablaAuditoria registros={data.elementos} correosActores={correosActores} />
          <div className="tfoot">
            <span>{T.contadorTotal(data.total)}</span>
            <nav aria-label="Paginación" style={{ display: 'inline-flex', gap: 'var(--s-5)', alignItems: 'center' }}>
              <button type="button" className="ob sm" disabled={pagina <= 1} onClick={() => setPagina(p => p - 1)}>Anterior</button>
              <span>Página {pagina}</span>
              <button type="button" className="ob sm" disabled={pagina * (data.tamano || 50) >= data.total} onClick={() => setPagina(p => p + 1)}>
                Siguiente
              </button>
            </nav>
          </div>
        </div>
      )}
    </>
  )
}
