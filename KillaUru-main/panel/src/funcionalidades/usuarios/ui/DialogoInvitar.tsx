// T057 — DialogoInvitar: invita nuevo usuario; maneja 409 correo_en_uso
import { useState } from 'react'
import type { Rol } from '@/api/dtos/001-identidad'
import { useCrearUsuario } from '../api/useAccionesUsuario'
import { mensajeDeErrorAccion } from '../modelo/erroresAccion'
import { TEXTOS_USUARIOS as textos } from '../textos'
import { FormularioDialogo } from './FormularioDialogo'
import { SelectorRol } from './SelectorRol'

export function DialogoInvitar({ onCerrar }: { onCerrar: () => void }) {
  const [correo, setCorreo] = useState('')
  const [rol, setRol] = useState<Rol>('ConductorRegistrado')
  const [error, setError] = useState('')
  const crear = useCrearUsuario()
  const invitar = () => {
    setError('')
    if (!correo.trim()) return setError('El correo es obligatorio.')
    crear.mutate({ correo: correo.trim(), rol }, { onSuccess: onCerrar, onError: (fallo) => setError(mensajeDeErrorAccion(fallo.codigo)) })
  }
  return (
    <FormularioDialogo titulo={textos.dialogoInvitarTitulo} error={error} ocupado={crear.isPending}
      textoConfirmar={textos.invitar} onEnviar={invitar} onCerrar={onCerrar}>
      <CampoCorreo valor={correo} onCambiar={setCorreo} />
      <SelectorRol valor={rol} onCambiar={setRol} orden={['ConductorRegistrado', 'Administrador']} />
    </FormularioDialogo>
  )
}

function CampoCorreo({ valor, onCambiar }: { valor: string; onCambiar: (correo: string) => void }) {
  return (
    <label>
      <span>{textos.correoLabel}</span>
      <input type="email" aria-label={textos.correoLabel} value={valor} autoComplete="off" onChange={(evento) => onCambiar(evento.target.value)} />
    </label>
  )
}
