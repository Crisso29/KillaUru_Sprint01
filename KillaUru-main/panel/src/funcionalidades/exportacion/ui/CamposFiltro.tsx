// T084 — qué anomalías exportar: rango de fechas, tipo y estado
import type { EstadoAnomalia } from '@/api/dtos/002-mapa'
import { textoEstado } from '@/compartido/textos/estados'
import { TEXTOS_EXPORTACION as textos } from '../textos'

export interface Filtro { desde: string; hasta: string; tipo: string; estado: EstadoAnomalia | '' }

const ESTADOS: EstadoAnomalia[] = ['Activa', 'PendienteConfirmacion', 'Expirada', 'Reparada', 'Descartada']

export function CamposFiltro({ filtro, onCambiar }: { filtro: Filtro; onCambiar: (filtro: Filtro) => void }) {
  const poner = (campo: keyof Filtro) => (evento: { target: { value: string } }) => onCambiar({ ...filtro, [campo]: evento.target.value })
  return (
    <div className="rejilla-2">
      <CampoFecha id="desde" etiqueta={textos.labelDesde} valor={filtro.desde} onChange={poner('desde')} />
      <CampoFecha id="hasta" etiqueta={textos.labelHasta} valor={filtro.hasta} onChange={poner('hasta')} />
      <div className="field"><label htmlFor="tipo">{textos.labelTipo}</label>
        <select id="tipo" value={filtro.tipo} onChange={poner('tipo')}>
          <option value="Todos">{textos.todosTipos}</option>
          <option value="Bache">Bache</option>
          <option value="Hundimiento">Hundimiento</option>
        </select></div>
      <div className="field"><label htmlFor="estado">{textos.labelEstados}</label>
        <select id="estado" value={filtro.estado} onChange={poner('estado')}>
          <option value="">{textos.todosEstados}</option>
          {ESTADOS.map((estado) => <option key={estado} value={estado}>{textoEstado(estado)}</option>)}
        </select></div>
    </div>
  )
}

interface PropsFecha { id: string; etiqueta: string; valor: string; onChange: (evento: { target: { value: string } }) => void }

function CampoFecha({ id, etiqueta, valor, onChange }: PropsFecha) {
  return <div className="field"><label htmlFor={id}>{etiqueta}</label><input id={id} type="date" value={valor} onChange={onChange} /></div>
}
