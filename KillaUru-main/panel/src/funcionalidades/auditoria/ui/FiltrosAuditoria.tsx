import { useState } from 'react'
import { TEXTOS_AUDITORIA } from '../textos'
import type { CategoriaAuditoria, AccionAuditada } from '@/api/generado/002-auditoria'

const CATEGORIAS: CategoriaAuditoria[] = [
  'Usuarios', 'Sesion', 'CuentaDelTitular', 'Moderacion', 'Exportacion', 'AccesoADatos',
]

const ACCIONES: AccionAuditada[] = [
  'UsuarioCreado', 'RolCambiado', 'CuentaDesactivada', 'CuentaActivada', 'CuentaEliminada',
  'InicioSesionFallido', 'CuentaBloqueada', 'InicioSesionAdministrador',
  'RecuperacionSolicitada', 'ContrasenaCambiada', 'RevisionPrivilegios',
  'AnomaliaModerada', 'DatasetExportado', 'AccesoAdministrativoDatos',
]

export interface ValoresFiltros {
  desde: string
  hasta: string
  categoria: string
  accion: string
  actor: string
}

interface Props {
  valores: ValoresFiltros
  onChange: (v: ValoresFiltros) => void
}

export function FiltrosAuditoria({ valores, onChange }: Props) {
  const T = TEXTOS_AUDITORIA.filtros

  function set(campo: keyof ValoresFiltros, valor: string) {
    onChange({ ...valores, [campo]: valor })
  }

  return (
    <fieldset>
      <legend>{TEXTOS_AUDITORIA.titulo}</legend>

      <label>
        {T.desde}
        <input
          type="datetime-local"
          aria-label={T.desde}
          value={valores.desde}
          onChange={e => set('desde', e.target.value)}
        />
      </label>

      <label>
        {T.hasta}
        <input
          type="datetime-local"
          aria-label={T.hasta}
          value={valores.hasta}
          onChange={e => set('hasta', e.target.value)}
        />
      </label>

      <label>
        {T.categoria}
        <select
          aria-label={T.categoria}
          value={valores.categoria}
          onChange={e => set('categoria', e.target.value)}
        >
          <option value="">{T.todasCategorias}</option>
          {CATEGORIAS.map(c => <option key={c} value={c}>{c}</option>)}
        </select>
      </label>

      <label>
        {T.accion}
        <select
          aria-label={T.accion}
          value={valores.accion}
          onChange={e => set('accion', e.target.value)}
        >
          <option value="">{T.todasAcciones}</option>
          {ACCIONES.map(a => <option key={a} value={a}>{a}</option>)}
        </select>
      </label>

      <label>
        {T.actorId}
        <input
          type="search"
          aria-label={T.actorId}
          value={valores.actor}
          onChange={e => set('actor', e.target.value)}
          placeholder="correo o UUID"
        />
      </label>
    </fieldset>
  )
}
