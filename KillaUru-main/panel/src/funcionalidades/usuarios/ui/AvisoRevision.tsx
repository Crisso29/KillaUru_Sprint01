// T057 — AvisoRevision: aviso con los días que faltan para la revisión de privilegios (prototipo `.banner-ops`)
import { Icono } from '@/compartido/ui'
import { formatearFechaLima } from '@/compartido/formato/fechas'
import { TEXTOS_USUARIOS as T } from '../textos'
import { diasRestantes } from '../modelo/revision'

interface Props {
  venceUtc: string
  onAbrir: () => void
}

/** En rojo si ya venció; si no, informativo con la fecha y los días que faltan. */
export function AvisoRevision({ venceUtc, onAbrir }: Props) {
  const dias = diasRestantes(venceUtc, new Date())
  const vencida = dias < 0
  return (
    <div role="alert" className="banner-ops" style={vencida ? { background: 'var(--bad-soft)', borderColor: 'var(--bad)' } : undefined}>
      <Icono nombre="shield" />
      <p>
        <b>{T.dialogoRevisionTitulo}</b>
        {vencida
          ? ` · ${T.revisionVencida}`
          : ` · vence el ${formatearFechaLima(venceUtc)} (en ${dias} ${T.revisionDias}). ${T.revisionConfirmar}`}
      </p>
      <button type="button" className="ob" onClick={onAbrir}>Hacer revisión</button>
    </div>
  )
}
