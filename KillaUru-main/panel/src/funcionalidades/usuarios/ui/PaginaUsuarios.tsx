// T056 — PaginaUsuarios: tabla de usuarios con filtros, paginación y revisión (FR-031-FR-034)
import { useState } from 'react'
import type { Usuario } from '@/api/dtos/001-identidad'
import { useUsuarios } from '../api/useUsuarios'
import { useRevision } from '../api/useRevision'
import { FiltrosUsuarios } from './FiltrosUsuarios'
import { TablaUsuarios } from './TablaUsuarios'
import { AvisoRevision } from './AvisoRevision'
import { DialogoInvitar } from './DialogoInvitar'
import { DialogoRevision } from './DialogoRevision'
import { PaginadorUsuarios } from './PaginadorUsuarios'
import { useFiltrosUsuarios } from './useFiltrosUsuarios'
import { TEXTOS_USUARIOS as textos } from '../textos'
import { Icono } from '@/compartido/ui'

const TAMANO_PAGINA = 50

interface Props { correoActual: string }

export function PaginaUsuarios({ correoActual }: Props) {
  const filtros = useFiltrosUsuarios()
  const [dialogo, setDialogo] = useState<'invitar' | 'revision' | null>(null)
  const { data, isError, refetch } = useUsuarios(filtros.consulta)
  const { data: revision } = useRevision()
  if (isError) return <ErrorConReintento onReintentar={() => void refetch()} />
  return (
    <>
      <Encabezado onInvitar={() => setDialogo('invitar')} />
      {revision && <AvisoRevision venceUtc={revision.venceUtc} onAbrir={() => setDialogo('revision')} />}
      <FiltrosUsuarios texto={filtros.busqueda.texto} rol={filtros.rol} estado={filtros.estado} onChange={filtros.cambiarCampo} onBuscar={filtros.busqueda.buscarYa} />
      <TarjetaUsuarios usuarios={data?.elementos ?? []} total={data?.total ?? 0} tamano={data?.tamano ?? TAMANO_PAGINA}
        pagina={filtros.pagina} onPagina={filtros.setPagina} correoActual={correoActual} />
      {dialogo === 'invitar' && <DialogoInvitar onCerrar={() => setDialogo(null)} />}
      {dialogo === 'revision' && revision && <DialogoRevision revision={revision} onCerrar={() => setDialogo(null)} />}
    </>
  )
}

function Encabezado({ onInvitar }: { onInvitar: () => void }) {
  return (
    <div className="pg-h">
      <div><h2>{textos.titulo}</h2><p>{textos.subtitulo}</p></div>
      <div className="acts">
        <button type="button" className="ob pri" onClick={onInvitar}><Icono nombre="plus" tamano={14} />{textos.invitar}</button>
      </div>
    </div>
  )
}

function ErrorConReintento({ onReintentar }: { onReintentar: () => void }) {
  return (
    <div role="alert" className="note bad" style={{ display: 'flex', alignItems: 'center', gap: 'var(--s-13)' }}>
      <p style={{ flex: 1 }}>{textos.errorRed}</p>
      <button type="button" className="ob" onClick={onReintentar}>{textos.reintentar}</button>
    </div>
  )
}

interface PropsTarjeta {
  usuarios: Usuario[]; total: number; tamano: number; pagina: number; onPagina: (pagina: number) => void; correoActual: string
}

function TarjetaUsuarios({ usuarios, total, tamano, pagina, onPagina, correoActual }: PropsTarjeta) {
  return (
    <div className="card">
      {usuarios.length === 0
        ? <p role="status" className="card-b hint">{textos.sinResultados}</p>
        : <TablaUsuarios usuarios={usuarios} correoActual={correoActual} />}
      <PaginadorUsuarios visibles={usuarios.length} total={total} tamano={tamano} pagina={pagina} onCambiar={onPagina} />
    </div>
  )
}
