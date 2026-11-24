// T080 — Auditoría (US6): filtros, tabla y páginas. Solo compone: el actor lo resuelve `useActorFiltrado`, los
// correos de la página `useActores` (una sola petición `?ids=`, compartida con Exportación y el Historial del mapa).
import { useState } from 'react'
import { TEXTOS_AUDITORIA } from '../textos'
import { Esqueleto, Icono } from '@/compartido/ui'
import { useActores } from '@/funcionalidades/usuarios'
import { FiltrosAuditoria, type ValoresFiltros } from './FiltrosAuditoria'
import { TablaAuditoria } from './TablaAuditoria'
import { PaginacionAuditoria } from './PaginacionAuditoria'
import { useAuditoria } from '../api/useAuditoria'
import { useActorFiltrado } from '../api/useActorFiltrado'

const FILTROS_INICIALES: ValoresFiltros = { desde: '', hasta: '', categoria: '', accion: '', actor: '' }
const TAMANO_PAGINA = 50

export function PaginaAuditoria() {
  const [filtros, setFiltros] = useState<ValoresFiltros>(FILTROS_INICIALES)
  const [pagina, setPagina] = useState(1)
  const actor = useActorFiltrado(filtros.actor)
  const { data, isLoading, isError } = useAuditoria({ ...soloConValor(filtros), actorId: actor.actorId, pagina })
  const correos = useActores(data?.elementos.map((registro) => registro.actorId) ?? [])
  const cambiarFiltros = (valores: ValoresFiltros) => { setFiltros(valores); setPagina(1) }
  return (
    <>
      <Encabezado />
      <FiltrosAuditoria valores={filtros} onChange={cambiarFiltros} />
      <Estados sinCuenta={actor.sinCuenta} cargando={isLoading} fallo={isError} />
      {data && <div className="card">
        <TablaAuditoria registros={data.elementos} correosActores={correos} />
        <PaginacionAuditoria pagina={pagina} total={data.total} tamano={data.tamano || TAMANO_PAGINA} onCambiar={setPagina} />
      </div>}
    </>
  )
}

function Encabezado() {
  return (
    <div className="pg-h">
      <div><h2>{TEXTOS_AUDITORIA.titulo}</h2><p>Registro de acciones sensibles. Inmutable: nadie puede editarlo ni borrarlo.</p></div>
      <span className="badge expirada"><Icono nombre="lock" tamano={12} />{TEXTOS_AUDITORIA.insignia}</span>
    </div>
  )
}

/** Los filtros vacíos no se envían: «sin filtro», no «filtro igual a vacío». */
function soloConValor({ desde, hasta, categoria, accion }: ValoresFiltros) {
  return { desde: desde || undefined, hasta: hasta || undefined, categoria: categoria || undefined, accion: accion || undefined }
}

function Estados({ sinCuenta, cargando, fallo }: { sinCuenta: boolean; cargando: boolean; fallo: boolean }) {
  return (
    <>
      {sinCuenta && <p role="alert" className="note warn">{TEXTOS_AUDITORIA.sinCuenta}</p>}
      {cargando && <Esqueleto filas={5} alto={40} />}
      {fallo && <p role="alert" className="note bad">{TEXTOS_AUDITORIA.error}</p>}
    </>
  )
}
