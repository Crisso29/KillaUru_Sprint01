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
  onChange: (valores: ValoresFiltros) => void
}

export function FiltrosAuditoria({ valores, onChange }: Props) {
  const textos = TEXTOS_AUDITORIA.filtros
  const poner = (campo: keyof ValoresFiltros) => (evento: { target: { value: string } }) => onChange({ ...valores, [campo]: evento.target.value })
  // Prototipo `.toolbar`: campos compactos y la nota «Hora en UTC». Las opciones se ven en lenguaje claro, no con su código.
  return (
    <div className="toolbar" role="group" aria-label="Filtros de auditoría">
      <SelectorCodigo etiqueta={textos.categoria} todas={textos.todasCategorias} valor={valores.categoria}
        codigos={CATEGORIAS} texto={(codigo) => textoDeCategoria(codigo as CategoriaAuditoria)} onChange={poner('categoria')} />
      <SelectorCodigo etiqueta={textos.accion} todas={textos.todasAcciones} valor={valores.accion}
        codigos={ACCIONES} texto={(codigo) => textoDeAccion(codigo as AccionAuditada)} onChange={poner('accion')} />
      <input className="tin" type="search" aria-label={textos.actorId} value={valores.actor} onChange={poner('actor')}
        placeholder="Actor (correo o id)" style={{ width: 200 }} />
      <input className="tin" type="datetime-local" aria-label={textos.desde} value={valores.desde} onChange={poner('desde')} />
      <input className="tin" type="datetime-local" aria-label={textos.hasta} value={valores.hasta} onChange={poner('hasta')} />
      <span className="hint">Hora en UTC</span>
    </div>
  )
}

interface PropsSelector {
  etiqueta: string; todas: string; valor: string; codigos: readonly string[]
  texto: (codigo: string) => string; onChange: (evento: { target: { value: string } }) => void
}

/** Un desplegable de códigos del contrato (002) que se muestran en lenguaje claro; vacío = «todas». */
function SelectorCodigo({ etiqueta, todas, valor, codigos, texto, onChange }: PropsSelector) {
  return (
    <select className="tsel" aria-label={etiqueta} value={valor} onChange={onChange}>
      <option value="">{todas}</option>
      {codigos.map((codigo) => <option key={codigo} value={codigo}>{texto(codigo)}</option>)}
    </select>
  )
}
