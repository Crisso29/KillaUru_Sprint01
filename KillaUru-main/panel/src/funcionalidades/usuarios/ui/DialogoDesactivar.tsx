// T057 — DialogoDesactivar: requiere motivo ≤200; maneja 409 accion_sobre_si_mismo
import { useState } from 'react'
import { useDesactivarUsuario } from '../api/useAccionesUsuario'
import { mensajeDeErrorAccion } from '../modelo/erroresAccion'
import { TEXTOS_USUARIOS as textos } from '../textos'
import { FormularioDialogo } from './FormularioDialogo'

const MAXIMO_MOTIVO = 200

export function DialogoDesactivar({ usuarioId, onCerrar }: { usuarioId: string; onCerrar: () => void }) {
  const [motivo, setMotivo] = useState('')
  const [error, setError] = useState('')
  const desactivar = useDesactivarUsuario(usuarioId)
  const confirmar = () => {
    setError('')
    if (!motivo.trim()) return setError(textos.motivoRequerido)
    if (motivo.length > MAXIMO_MOTIVO) return setError(textos.motivoMaximo)
    desactivar.mutate({ motivo }, { onSuccess: onCerrar, onError: (fallo) => setError(mensajeDeErrorAccion(fallo.codigo)) })
  }
  return (
    <FormularioDialogo titulo={textos.dialogoDesactivarTitulo} error={error} ocupado={desactivar.isPending}
      textoConfirmar={textos.confirmar} onEnviar={confirmar} onCerrar={onCerrar}>
      <CampoMotivo valor={motivo} onCambiar={setMotivo} />
    </FormularioDialogo>
  )
}

function CampoMotivo({ valor, onCambiar }: { valor: string; onCambiar: (motivo: string) => void }) {
  return (
    <label>
      <span>{textos.motivoLabel}</span>
      <textarea aria-label={textos.motivoLabel} value={valor} maxLength={MAXIMO_MOTIVO} rows={3}
        style={{ width: '100%', resize: 'vertical' }} onChange={(evento) => onCambiar(evento.target.value)} />
    </label>
  )
}
