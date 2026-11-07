// T057 — AvisoRevision: banner que avisa días restantes o vencimiento
import { TEXTOS_USUARIOS as T } from '../textos'
import { diasRestantes } from '../modelo/revision'

interface Props {
  venceUtc: string
  onAbrir: () => void
}

export function AvisoRevision({ venceUtc, onAbrir }: Props) {
  const dias = diasRestantes(venceUtc, new Date())
  const vencida = dias < 0

  return (
    <div
      role="alert"
      style={{
        padding: 'var(--s-13) var(--s-21)',
        backgroundColor: vencida ? 'var(--error-bg, #fff0f0)' : 'var(--warn-bg, #fffbe6)',
        border: `1px solid ${vencida ? 'var(--error)' : 'var(--warn, #d4b200)'}`,
        borderRadius: 'var(--r-8)',
        marginBottom: 'var(--s-21)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: 'var(--s-13)',
      }}
    >
      <span>
        {vencida
          ? T.revisionVencida
          : `${T.revisionAvisoVence} ${dias} ${T.revisionDias}.`}
      </span>
      <button type="button" onClick={onAbrir} style={{ whiteSpace: 'nowrap' }}>
        {T.dialogoRevisionTitulo}
      </button>
    </div>
  )
}
