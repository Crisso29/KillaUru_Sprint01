import { TEXTOS_AUDITORIA } from '../textos'
import { textoDeAccion, textoDeCategoria } from '../modelo/textoAccion'
import type { CategoriaAuditoria, AccionAuditada } from '@/api/dtos/002-auditoria'

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

  // Prototipo `.toolbar`: campos compactos y la nota «Hora en UTC». Las opciones se ven en lenguaje claro, no con su código.
  return (
    <div className="toolbar" role="group" aria-label="Filtros de auditoría">
      <select className="tsel" aria-label={T.categoria} value={valores.categoria} onChange={e => set('categoria', e.target.value)}>
        <option value="">{T.todasCategorias}</option>
        {CATEGORIAS.map(c => <option key={c} value={c}>{textoDeCategoria(c)}</option>)}
      </select>
      <select className="tsel" aria-label={T.accion} value={valores.accion} onChange={e => set('accion', e.target.value)}>
        <option value="">{T.todasAcciones}</option>
        {ACCIONES.map(a => <option key={a} value={a}>{textoDeAccion(a)}</option>)}
      </select>
      <input className="tin" type="search" aria-label={T.actorId} value={valores.actor}
        onChange={e => set('actor', e.target.value)} placeholder="Actor (correo o id)" style={{ width: 200 }} />
      <input className="tin" type="datetime-local" aria-label={T.desde} value={valores.desde} onChange={e => set('desde', e.target.value)} />
      <input className="tin" type="datetime-local" aria-label={T.hasta} value={valores.hasta} onChange={e => set('hasta', e.target.value)} />
      <span className="hint">Hora en UTC</span>
    </div>
  )
}
