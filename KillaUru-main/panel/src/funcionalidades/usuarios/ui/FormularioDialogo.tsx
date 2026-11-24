// T057 — marco común de los diálogos de Usuarios: título, campos, error y Cancelar / Confirmar. Usa el diálogo común
// del panel: el foco entra al abrirse (WCAG 2.4.3), Escape cancela y el título le da nombre accesible.
import { useId } from 'react'
import { Boton, Dialogo } from '@/compartido/ui'
import { TEXTOS_USUARIOS as textos } from '../textos'

interface Props {
  titulo: string
  error: string
  ocupado: boolean
  textoConfirmar: string
  onEnviar: () => void
  onCerrar: () => void
  children: React.ReactNode
}

export function FormularioDialogo({ titulo, error, ocupado, textoConfirmar, onEnviar, onCerrar, children }: Props) {
  const idFormulario = useId() // los botones van en el pie del diálogo, fuera del <form>: `form=` los une a él
  const enviar = (evento: React.FormEvent) => { evento.preventDefault(); onEnviar() }
  const acciones = (
    <>
      <Boton variante="secundario" onClick={onCerrar}>{textos.cancelar}</Boton>
      <Boton variante="primario" type="submit" form={idFormulario} disabled={ocupado}>{textoConfirmar}</Boton>
    </>
  )
  return (
    <Dialogo abierto titulo={titulo} onCerrar={onCerrar} acciones={acciones}>
      <form id={idFormulario} onSubmit={enviar}>
        {children}
        {error && <p role="alert" className="note bad">{error}</p>}
      </form>
    </Dialogo>
  )
}
