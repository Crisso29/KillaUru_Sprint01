// T084 — PaginaExportacion: formulario (61,8 %) y exportaciones recientes (38,2 %), como el prototipo
import { FormularioExportacion } from './FormularioExportacion'
import { ExportacionesRecientes } from './ExportacionesRecientes'
import { TEXTOS_EXPORTACION as textos } from '../textos'

interface Props { correoActual: string }

export function PaginaExportacion({ correoActual }: Props) {
  return (
    <>
      <div className="pg-h">
        <div><h2>{textos.titulo}</h2><p>{textos.subtitulo}</p></div>
      </div>
      <div className="g61" style={{ alignItems: 'start' }}>
        <FormularioExportacion />
        <section className="card" aria-labelledby="titulo-recientes">
          <div className="card-h"><h3 id="titulo-recientes">{textos.recientesTitulo}</h3></div>
          <ExportacionesRecientes correoActual={correoActual} />
        </section>
      </div>
    </>
  )
}
