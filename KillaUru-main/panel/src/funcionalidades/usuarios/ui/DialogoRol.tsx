// T057 — DialogoRol: cambia rol de un usuario; maneja 409 ultimo_administrador
import { useState } from 'react'
import type { Rol } from '@/api/dtos/001-identidad'
import { useCambiarRol } from '../api/useAccionesUsuario'
import { mensajeDeErrorAccion } from '../modelo/erroresAccion'
import { TEXTOS_USUARIOS as textos } from '../textos'
import { FormularioDialogo } from './FormularioDialogo'
import { SelectorRol } from './SelectorRol'

interface Props { usuarioId: string; rolActual: Rol; onCerrar: () => void }

export function DialogoRol({ usuarioId, rolActual, onCerrar }: Props) {
  // Se propone el otro rol: abrir el diálogo es querer cambiarlo
  const [rol, setRol] = useState<Rol>(rolActual === 'Administrador' ? 'ConductorRegistrado' : 'Administrador')
  const [error, setError] = useState('')
  const cambiar = useCambiarRol(usuarioId)
  const confirmar = () => {
    setError('')
    cambiar.mutate({ rol }, { onSuccess: onCerrar, onError: (fallo) => setError(mensajeDeErrorAccion(fallo.codigo)) })
  }
  return (
    <FormularioDialogo titulo={textos.dialogoCambiarRolTitulo} error={error} ocupado={cambiar.isPending}
      textoConfirmar={textos.confirmar} onEnviar={confirmar} onCerrar={onCerrar}>
      <SelectorRol valor={rol} onCambiar={setRol} orden={['Administrador', 'ConductorRegistrado']} />
    </FormularioDialogo>
  )
}
