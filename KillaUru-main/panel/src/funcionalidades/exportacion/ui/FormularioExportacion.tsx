// T084 — FormularioExportacion: rango, tipo, estado y formato (FR-034…; prototipo «Nueva exportación»)
import { useState } from 'react'
import type { EstadoAnomalia } from '@/api/dtos/002-mapa'
import { construirSolicitud } from '../modelo/solicitud'
import { useExportar } from '../api/useExportar'
import { TEXTOS_EXPORTACION as T } from '../textos'
import { Icono } from '@/compartido/ui'
import { textoEstado } from '@/compartido/textos/estados'
import { ColumnasExportadas } from './ColumnasExportadas'

const ESTADOS: EstadoAnomalia[] = ['Activa', 'PendienteConfirmacion', 'Expirada', 'Reparada', 'Descartada']
const FORMATOS = [
  { valor: 'CSV', nota: 'Excel, Google Sheets' },
  { valor: 'GeoJSON', nota: 'QGIS u otro SIG' },
] as const

export function FormularioExportacion() {
  const [formato, setFormato] = useState<'CSV' | 'GeoJSON'>('CSV')
  const [desde, setDesde] = useState('')
  const [hasta, setHasta] = useState('')
  const [tipo, setTipo] = useState('Todos')
  const [estado, setEstado] = useState<EstadoAnomalia | ''>('')
  const exportar = useExportar()

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    exportar.mutate(construirSolicitud(formato, desde, hasta, tipo, estado ? [estado] : []))
  }

  return (
    <form onSubmit={handleSubmit} className="card" aria-labelledby="titulo-nueva">
      <div className="card-h"><h3 id="titulo-nueva">{T.nueva}</h3></div>
      <div className="card-b" style={{ display: 'grid', gap: 'var(--s-13)' }}>
        <div className="rejilla-2">
          <div className="field"><label htmlFor="desde">{T.labelDesde}</label>
            <input id="desde" type="date" value={desde} onChange={e => setDesde(e.target.value)} /></div>
          <div className="field"><label htmlFor="hasta">{T.labelHasta}</label>
            <input id="hasta" type="date" value={hasta} onChange={e => setHasta(e.target.value)} /></div>
          <div className="field"><label htmlFor="tipo">{T.labelTipo}</label>
            <select id="tipo" value={tipo} onChange={e => setTipo(e.target.value)}>
              <option value="Todos">{T.todosTipos}</option>
              <option value="Bache">Bache</option>
              <option value="Hundimiento">Hundimiento</option>
            </select></div>
          <div className="field"><label htmlFor="estado">{T.labelEstados}</label>
            <select id="estado" value={estado} onChange={e => setEstado(e.target.value as EstadoAnomalia | '')}>
              <option value="">{T.todosEstados}</option>
              {ESTADOS.map(s => <option key={s} value={s}>{textoEstado(s)}</option>)}
            </select></div>
        </div>
        <fieldset className="fmt" style={{ border: 0, padding: 0 }}>
          <legend className="sr-only">{T.labelFormato}</legend>
          {FORMATOS.map(f => (
            <label key={f.valor}>
              <input type="radio" name="formato" value={f.valor} checked={formato === f.valor} onChange={() => setFormato(f.valor)} />
              <span><b>{f.valor}</b><small>{f.nota}</small></span>
            </label>
          ))}
        </fieldset>
        <ColumnasExportadas />
        {exportar.isError && <p role="alert" className="note bad">{T.errorExportar}</p>}
        <div className="row" style={{ borderTop: '1px solid var(--line)', paddingTop: 'var(--s-13)' }}>
          <span className="hint" style={{ display: 'inline-flex', gap: 4, alignItems: 'center' }}>
            <Icono nombre="shield" tamano={12} />{T.seudonimoNota}
          </span>
          <button type="submit" className="ob pri" disabled={exportar.isPending} style={{ height: 36 }}>
            <Icono nombre="down" tamano={14} />{exportar.isPending ? T.cargando : T.exportar}
          </button>
        </div>
      </div>
    </form>
  )
}
