// T056 — PaginaUsuarios: tabla de usuarios con filtros, paginación y revisión (FR-031-FR-034)
import { useState, useCallback, useRef } from 'react'
import type { Rol, EstadoUsuario } from '@/api/generado/001-identidad'
import { useUsuarios } from '../api/useUsuarios'
import { useRevision } from '../api/useRevision'
import { aConsulta } from '../modelo/consulta'
import { FiltrosUsuarios } from './FiltrosUsuarios'
import { TablaUsuarios } from './TablaUsuarios'
import { AvisoRevision } from './AvisoRevision'
import { DialogoInvitar } from './DialogoInvitar'
import { DialogoRevision } from './DialogoRevision'
import { TEXTOS_USUARIOS as T } from '../textos'

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
      <div style={{ padding: 'var(--s-34)' }}>
        <p>{T.errorRed}</p>
        <button type="button" onClick={() => refetch()}>{T.reintentar}</button>
      </div>
    )
  }

  return (
    <div style={{ padding: 'var(--s-21) var(--s-34)' }}>
      {revision && (
        <AvisoRevision
          venceUtc={revision.venceUtc}
          onAbrir={() => setDialogoAbierto('revision')}
        />
      )}

      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 'var(--s-21)' }}>
        <h1 style={{ fontSize: 'var(--t-21)', fontWeight: 700, margin: 0 }}>{T.titulo}</h1>
        <button type="button" onClick={() => setDialogoAbierto('invitar')}>{T.invitar}</button>
      </div>

      <FiltrosUsuarios
        texto={texto}
        rol={rol}
        estado={estado}
        onChange={handleCampo}
        onBuscar={aplicarBusqueda}
      />

      <p style={{ fontSize: 'var(--t-13)', color: 'var(--ink-3)', marginBottom: 'var(--s-13)' }}>
        {elementos.length} {T.de} {total} {T.usuariosLabel}
      </p>

      {elementos.length === 0
        ? <p role="status">{T.sinResultados}</p>
        : <TablaUsuarios usuarios={elementos} correoActual={correoActual} />}

      {total > (data?.tamano ?? 50) && (
        <div style={{ display: 'flex', gap: 'var(--s-8)', justifyContent: 'center', marginTop: 'var(--s-21)' }}>
          <button type="button" disabled={pagina <= 1} onClick={() => setPagina(p => p - 1)}>‹</button>
          <span>{pagina}</span>
          <button type="button" disabled={pagina * (data?.tamano ?? 50) >= total} onClick={() => setPagina(p => p + 1)}>›</button>
        </div>
      )}

      {dialogoAbierto === 'invitar' && (
        <DialogoInvitar onCerrar={() => setDialogoAbierto(null)} />
      )}
      {dialogoAbierto === 'revision' && revision && (
        <DialogoRevision revision={revision} onCerrar={() => setDialogoAbierto(null)} />
      )}
    </div>
  )
}
