import { useState } from 'react'
import { describe, it, expect } from 'vitest'
import userEvent from '@testing-library/user-event'
import { renderizar, screen } from '@/../pruebas/renderizar'
import { Dialogo } from './Dialogo'

// T018 — foco al abrir, foco atrapado, Esc cierra, 0 violaciones axe

describe('Dialogo', () => {
  it('recibe el foco al abrirse', async () => {
    // Arrange y Act
    renderizar(
      <Dialogo abierto titulo="Confirmar" onCerrar={() => {}}>
        <p>Contenido</p>
      </Dialogo>,
    )

    // Assert
    expect(document.activeElement?.closest('[role="dialog"]')).toBeTruthy()
  })

  it('Esc lo cierra y llama onCerrar', async () => {
    // Arrange
    const onCerrar = vi.fn()
    renderizar(
      <Dialogo abierto titulo="Test" onCerrar={onCerrar}>
        <p>Contenido</p>
      </Dialogo>,
    )

    // Act
    await userEvent.keyboard('{Escape}')

    // Assert
    expect(onCerrar).toHaveBeenCalledOnce()
  })

  it('no renderiza cuando abierto=false', () => {
    // Arrange y Act
    renderizar(
      <Dialogo abierto={false} titulo="Oculto" onCerrar={() => {}}>
        <p>Oculto</p>
      </Dialogo>,
    )

    // Assert
    expect(screen.queryByRole('dialog')).toBeNull()
  })

  it('tiene role dialog y aria-modal', () => {
    // Arrange y Act
    renderizar(
      <Dialogo abierto titulo="Accesible" onCerrar={() => {}}>
        <p>OK</p>
      </Dialogo>,
    )

    // Assert
    const dialogo = screen.getByRole('dialog')
    expect(dialogo).toHaveAttribute('aria-modal', 'true')
  })

  it('escribir en un campo del diálogo no le quita el foco, aunque el padre cree onCerrar en cada render', async () => {
    // Arrange — un formulario controlado: cada tecla vuelve a dibujar al padre y crea otro onCerrar
    function Formulario() {
      const [texto, setTexto] = useState('')
      return (
        <Dialogo abierto titulo="Invitar" onCerrar={() => {}}>
          <input aria-label="Correo" value={texto} onChange={(evento) => setTexto(evento.target.value)} />
        </Dialogo>
      )
    }
    renderizar(<Formulario />)
    const campo = screen.getByLabelText('Correo')
    campo.focus()

    // Act
    await userEvent.keyboard('ops@killauru.pe')

    // Assert
    expect(campo).toHaveValue('ops@killauru.pe')
    expect(campo).toHaveFocus()
  })
})
