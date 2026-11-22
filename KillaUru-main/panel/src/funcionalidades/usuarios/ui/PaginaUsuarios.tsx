// T056 — PaginaUsuarios: tabla de usuarios con filtros, paginación y revisión (FR-031-FR-034)
import { useState, useCallback, useRef } from 'react'
import type { Rol, EstadoUsuario } from '@/api/dtos/001-identidad'
import { useUsuarios } from '../api/useUsuarios'
import { useRevision } from '../api/useRevision'
import { aConsulta } from '../modelo/consulta'
import { FiltrosUsuarios } from './FiltrosUsuarios'
import { TablaUsuarios } from './TablaUsuarios'
import { AvisoRevision } from './AvisoRevision'
import { DialogoInvitar } from './DialogoInvitar'
import { DialogoRevision } from './DialogoRevision'
import { TEXTOS_USUARIOS as T } from '../textos'
import { Icono } from '@/compartido/ui'

interface Props { correoActual: string }

export function PaginaUsuarios({ correoActual }: Props) {
  const [texto, setTexto] = useState('')
  const [textoActivo, setTextoActivo] = useState('')
  const [rol, setRol] = useState<Rol | ''>('')
  const [estado, setEstado] = useState<EstadoUsuario | ''>('')
  const [pagina, setPagina] = useState(1)
  const [dialogoAbierto, setDialogoAbierto] = useState<'invitar' | 'revision' | null>(null)
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const consulta = aConsulta(textoActivo, rol, estado, pagina)
  const { data, isError, refetch } = useUsuarios(consulta)
  const { data: revision } = useRevision()

  function aplicarBusqueda() {
    setPagina(1)
    setTextoActivo(texto)
  }

  const handleCampo = useCallback((campo: 'texto' | 'rol' | 'estado', valor: string) => {
    if (campo === 'texto') {
      setTexto(valor)
      if (debounceRef.current) clearTimeout(debounceRef.current)
      debounceRef.current = setTimeout(() => {
        setPagina(1)
        setTextoActivo(valor)
      }, 400)
    } else if (campo === 'rol') {
      setRol(valor as Rol | '')
      setPagina(1)
    } else {
      setEstado(valor as EstadoUsuario | '')
      setPagina(1)
    }
  }, [])

  const elementos = data?.elementos ?? []
  const total = data?.total ?? 0

  if (isError) {
    return (
      <div role="alert" className="note bad" style={{ display: 'flex', alignItems: 'center', gap: 'var(--s-13)' }}>
        <p style={{ flex: 1 }}>{T.errorRed}</p>
        <button type="button" className="ob" onClick={() => void refetch()}>{T.reintentar}</button>
      </div>
    )
  }

  return (
    <>
      <div className="pg-h">
        <div><h2>{T.titulo}</h2><p>{T.subtitulo}</p></div>
        <div className="acts">
          <button type="button" className="ob pri" onClick={() => setDialogoAbierto('invitar')}>
            <Icono nombre="plus" tamano={14} />{T.invitar}
          </button>
        </div>
      </div>

      {revision && <AvisoRevision venceUtc={revision.venceUtc} onAbrir={() => setDialogoAbierto('revision')} />}

      <FiltrosUsuarios texto={texto} rol={rol} estado={estado} onChange={handleCampo} onBuscar={aplicarBusqueda} />

      <div className="card">
        {elementos.length === 0
          ? <p role="status" className="card-b hint">{T.sinResultados}</p>
          : <TablaUsuarios usuarios={elementos} correoActual={correoActual} />}
        <div className="tfoot">
          <span>{elementos.length} {T.de} {total} {T.usuariosLabel}</span>
          {total > (data?.tamano ?? 50) ? (
            <span style={{ display: 'inline-flex', gap: 'var(--s-5)', alignItems: 'center' }}>
              <button type="button" className="ob sm" aria-label="Página anterior" disabled={pagina <= 1} onClick={() => setPagina(p => p - 1)}>‹</button>
              <span>Página {pagina}</span>
              <button type="button" className="ob sm" aria-label="Página siguiente" disabled={pagina * (data?.tamano ?? 50) >= total} onClick={() => setPagina(p => p + 1)}>›</button>
            </span>
          ) : <span>{T.pieTabla}</span>}
        </div>
      </div>

      {dialogoAbierto === 'invitar' && <DialogoInvitar onCerrar={() => setDialogoAbierto(null)} />}
      {dialogoAbierto === 'revision' && revision && (
        <DialogoRevision revision={revision} onCerrar={() => setDialogoAbierto(null)} />
      )}
    </>
  )
}
