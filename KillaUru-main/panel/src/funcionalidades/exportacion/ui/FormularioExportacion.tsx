// T084 — FormularioExportacion: selección de formato, rango, tipo y botón Exportar
import { useState } from 'react'
import { construirSolicitud } from '../modelo/solicitud'
import { useExportar } from '../api/useExportar'
import { TEXTOS_EXPORTACION as T } from '../textos'

export function FormularioExportacion() {
  const [formato, setFormato] = useState<'CSV' | 'GeoJSON'>('CSV')
  const [desde, setDesde] = useState('')
  const [hasta, setHasta] = useState('')
  const [tipo, setTipo] = useState('Todos')
  const exportar = useExportar()

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    exportar.mutate(construirSolicitud(formato, desde, hasta, tipo, []))
  }

  return (
    <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 'var(--s-13)' }}>
      <div>
        <label htmlFor="formato">{T.labelFormato}</label>
        <select id="formato" value={formato} onChange={e => setFormato(e.target.value as 'CSV' | 'GeoJSON')}>
          <option value="CSV">CSV</option>
          <option value="GeoJSON">GeoJSON</option>
        </select>
      </div>
      <div>
        <label htmlFor="desde">{T.labelDesde}</label>
        <input id="desde" type="date" value={desde} onChange={e => setDesde(e.target.value)} />
      </div>
      <div>
        <label htmlFor="hasta">{T.labelHasta}</label>
        <input id="hasta" type="date" value={hasta} onChange={e => setHasta(e.target.value)} />
      </div>
      <div>
        <label htmlFor="tipo">{T.labelTipo}</label>
        <select id="tipo" value={tipo} onChange={e => setTipo(e.target.value)}>
          <option value="Todos">{T.todosTipos}</option>
          <option value="Bache">Bache</option>
          <option value="Hundimiento">Hundimiento</option>
        </select>
      </div>
      {exportar.isError && (
        <p role="alert" style={{ color: 'var(--error)' }}>{T.errorExportar}</p>
      )}
      <button type="submit" disabled={exportar.isPending}>
        {exportar.isPending ? T.cargando : T.exportar}
      </button>
    </form>
  )
}
