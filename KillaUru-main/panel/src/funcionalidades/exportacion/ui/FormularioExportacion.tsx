// T084 — FormularioExportacion: rango, tipo, estado y formato (FR-034…; prototipo «Nueva exportación»)
import { useState } from 'react'
import { construirSolicitud } from '../modelo/solicitud'
import { useExportar } from '../api/useExportar'
import { TEXTOS_EXPORTACION as textos } from '../textos'
import { Icono } from '@/compartido/ui'
import { ColumnasExportadas } from './ColumnasExportadas'
import { CamposFiltro, type Filtro } from './CamposFiltro'

type Formato = 'CSV' | 'GeoJSON'

const FORMATOS: { valor: Formato; nota: string }[] = [
  { valor: 'CSV', nota: 'Excel, Google Sheets' },
  { valor: 'GeoJSON', nota: 'QGIS u otro SIG' },
]

export function FormularioExportacion() {
  const [formato, setFormato] = useState<Formato>('CSV')
  const [filtro, setFiltro] = useState<Filtro>({ desde: '', hasta: '', tipo: 'Todos', estado: '' })
  const exportar = useExportar()
  const enviar = (evento: React.FormEvent) => { evento.preventDefault(); exportar.mutate(solicitudDe(formato, filtro)) }
  return (
    <form onSubmit={enviar} className="card" aria-labelledby="titulo-nueva">
      <div className="card-h"><h3 id="titulo-nueva">{textos.nueva}</h3></div>
      <div className="card-b" style={{ display: 'grid', gap: 'var(--s-13)' }}>
        <CamposFiltro filtro={filtro} onCambiar={setFiltro} />
        <SelectorFormato valor={formato} onCambiar={setFormato} />
        <ColumnasExportadas />
        {exportar.isError && <p role="alert" className="note bad">{textos.errorExportar}</p>}
        <PieEnviar ocupado={exportar.isPending} />
      </div>
    </form>
  )
}

function SelectorFormato({ valor, onCambiar }: { valor: Formato; onCambiar: (formato: Formato) => void }) {
  return (
    <fieldset className="fmt" style={{ border: 0, padding: 0 }}>
      <legend className="sr-only">{textos.labelFormato}</legend>
      {FORMATOS.map((opcion) => (
        <label key={opcion.valor}>
          <input type="radio" name="formato" value={opcion.valor} checked={valor === opcion.valor} onChange={() => onCambiar(opcion.valor)} />
          <span><b>{opcion.valor}</b><small>{opcion.nota}</small></span>
        </label>
      ))}
    </fieldset>
  )
}

/** El archivo nunca lleva seudónimos (RNF-10): se recuerda junto al botón. */
function PieEnviar({ ocupado }: { ocupado: boolean }) {
  return (
    <div className="row" style={{ borderTop: '1px solid var(--line)', paddingTop: 'var(--s-13)' }}>
      <span className="hint" style={{ display: 'inline-flex', gap: 4, alignItems: 'center' }}>
        <Icono nombre="shield" tamano={12} />{textos.seudonimoNota}
      </span>
      <button type="submit" className="ob pri" disabled={ocupado} style={{ height: 36 }}>
        <Icono nombre="down" tamano={14} />{ocupado ? textos.cargando : textos.exportar}
      </button>
    </div>
  )
}

const solicitudDe = (formato: Formato, filtro: Filtro) =>
  construirSolicitud(formato, filtro.desde, filtro.hasta, filtro.tipo, filtro.estado ? [filtro.estado] : [])
