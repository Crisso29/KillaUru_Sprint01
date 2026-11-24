import { describe, it, expect } from 'vitest'
import { renderizar, screen } from '@/../pruebas/renderizar'
import { ProveedorAvisos, useAvisos } from './Avisos'

// T018 — avisos en aria-live

function ComponenteConAviso({ texto }: { texto: string }) {
  const { agregar } = useAvisos()
  return <button onClick={() => agregar({ tipo: 'info', texto })}>Avisar</button>
}

describe('Avisos', () => {
  it('región aria-live=polite existe', () => {
    // Arrange y Act
    renderizar(<ProveedorAvisos><div /></ProveedorAvisos>)

    // Assert
    expect(document.querySelector('[aria-live="polite"]')).toBeTruthy()
  })

  it('agrega aviso al hacer clic', async () => {
    // Arrange
    const user = (await import('@testing-library/user-event')).default.setup()
    renderizar(
      <ProveedorAvisos>
        <ComponenteConAviso texto="Guardado" />
      </ProveedorAvisos>,
    )

    // Act
    await user.click(screen.getByRole('button', { name: 'Avisar' }))

    // Assert
    expect(screen.getByText('Guardado')).toBeTruthy()
  })

  it('aviso de error tiene role=alert', async () => {
    // Arrange
    function ComponenteError() {
      const { agregar } = useAvisos()
      return <button onClick={() => agregar({ tipo: 'error', texto: 'Fallo' })}>Error</button>
    }
    const user = (await import('@testing-library/user-event')).default.setup()
    renderizar(<ProveedorAvisos><ComponenteError /></ProveedorAvisos>)

    // Act
    await user.click(screen.getByRole('button', { name: 'Error' }))

    // Assert
    expect(screen.getByRole('alert')).toBeTruthy()
  })
})
