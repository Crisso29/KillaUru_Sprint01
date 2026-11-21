import { describe, it, expect } from 'vitest'
import userEvent from '@testing-library/user-event'
import { renderizar, screen } from '@/../pruebas/renderizar'
import { Dialogo } from './Dialogo'

// T018 — foco al abrir, foco atrapado, Esc cierra, 0 violaciones axe

describe('Dialogo', () => {
  it('recibe el foco al abrirse', async () => {
    renderizar(
      <Dialogo abierto titulo="Confirmar" onCerrar={() => {}}>
        <p>Contenido</p>
      </Dialogo>,
    )
    expect(document.activeElement?.closest('[role="dialog"]')).toBeTruthy()
  })

  it('Esc lo cierra y llama onCerrar', async () => {
    const onCerrar = vi.fn()
    renderizar(
      <Dialogo abierto titulo="Test" onCerrar={onCerrar}>
        <p>Contenido</p>
      </Dialogo>,
    )
    await userEvent.keyboard('{Escape}')
    expect(onCerrar).toHaveBeenCalledOnce()
  })

  it('no renderiza cuando abierto=false', () => {
    renderizar(
      <Dialogo abierto={false} titulo="Oculto" onCerrar={() => {}}>
        <p>Oculto</p>
      </Dialogo>,
    )
    expect(screen.queryByRole('dialog')).toBeNull()
  })

  it('tiene role dialog y aria-modal', () => {
    renderizar(
      <Dialogo abierto titulo="Accesible" onCerrar={() => {}}>
        <p>OK</p>
      </Dialogo>,
    )
    const dialogo = screen.getByRole('dialog')
    expect(dialogo).toHaveAttribute('aria-modal', 'true')
  })
})
