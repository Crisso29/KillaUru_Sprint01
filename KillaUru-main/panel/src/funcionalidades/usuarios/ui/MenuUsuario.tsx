// T056 — MenuUsuario: acciones por estado; fila propia sin Cambiar rol ni Desactivar
import { useState } from 'react'
import type { Usuario } from '@/api/dtos/001-identidad'
import { useActivarUsuario, useReenviarCorreo } from '../api/useAccionesUsuario'
import { DialogoRol } from './DialogoRol'
import { DialogoDesactivar } from './DialogoDesactivar'
import { TEXTOS_USUARIOS as textos } from '../textos'

type Dialogo = 'rol' | 'desactivar' | null

interface Opcion { texto: string; alElegir: () => void }

interface Props {
  usuario: Usuario
  correoActual: string
}

const ESTILO_LISTA: React.CSSProperties = {
  position: 'absolute', right: 0, top: '100%', zIndex: 10, background: 'var(--surface)', border: '1px solid var(--line)',
  borderRadius: '8px', listStyle: 'none', padding: 'var(--s-5)', margin: 0, minWidth: 180, boxShadow: '0 2px 8px rgba(0,0,0,.12)',
}
const ESTILO_OPCION: React.CSSProperties = { padding: 'var(--s-8) var(--s-13)', cursor: 'pointer' }

export function MenuUsuario({ usuario, correoActual }: Props) {
  const [abierto, setAbierto] = useState(false)
  const [dialogo, setDialogo] = useState<Dialogo>(null)
  const activar = useActivarUsuario(usuario.id)
  const reenviar = useReenviarCorreo(usuario.id)
  const opciones = opcionesDelMenu(usuario, usuario.correo === correoActual, {
    abrir: setDialogo, activar: () => activar.mutate(), reenviar: () => reenviar.mutate(),
  })
  const cerrarTodo = () => { setDialogo(null); setAbierto(false) }
  return (
    <div style={{ position: 'relative' }}>
      <button type="button" aria-label={textos.accionesLabel} aria-haspopup="menu" aria-expanded={abierto}
        onClick={() => setAbierto((estaAbierto) => !estaAbierto)} style={{ padding: 'var(--s-5) var(--s-8)', cursor: 'pointer' }}>
        ⋮
      </button>
      {abierto && <ListaOpciones opciones={opciones} onElegida={() => setAbierto(false)} />}
      <DialogoElegido dialogo={dialogo} usuario={usuario} onCerrar={cerrarTodo} />
    </div>
  )
}

function DialogoElegido({ dialogo, usuario, onCerrar }: { dialogo: Dialogo; usuario: Usuario; onCerrar: () => void }) {
  if (dialogo === 'rol') return <DialogoRol usuarioId={usuario.id} rolActual={usuario.rol} onCerrar={onCerrar} />
  if (dialogo === 'desactivar') return <DialogoDesactivar usuarioId={usuario.id} onCerrar={onCerrar} />
  return null
}

function ListaOpciones({ opciones, onElegida }: { opciones: Opcion[]; onElegida: () => void }) {
  return (
    <ul role="menu" style={ESTILO_LISTA}>
      {opciones.map((opcion) => (
        <li key={opcion.texto} role="menuitem" style={ESTILO_OPCION} onClick={() => { onElegida(); opcion.alElegir() }}>
          {opcion.texto}
        </li>
      ))}
    </ul>
  )
}

/** RF-19: nadie cambia su propio rol ni se desactiva a sí mismo; una cuenta desactivada solo se reactiva. */
function opcionesDelMenu(usuario: Usuario, esPropia: boolean,
  acciones: { abrir: (dialogo: Dialogo) => void; activar: () => void; reenviar: () => void }): Opcion[] {
  const desactivada = usuario.estado === 'Desactivada'
  const pendiente = usuario.estado === 'InvitacionPendiente'
  const opciones: Opcion[] = []
  if (!esPropia && !desactivada) {
    opciones.push({ texto: textos.cambiarRol, alElegir: () => acciones.abrir('rol') },
      { texto: textos.desactivar, alElegir: () => acciones.abrir('desactivar') })
  }
  if (desactivada) opciones.push({ texto: textos.activar, alElegir: acciones.activar })
  if (!usuario.correoVerificado || pendiente) {
    opciones.push({ texto: pendiente ? textos.reenviarInvitacion : textos.reenviarVerificacion, alElegir: acciones.reenviar })
  }
  return opciones
}
