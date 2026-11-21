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
    renderizar(<ProveedorAvisos><div /></ProveedorAvisos>)
    expect(document.querySelector('[aria-live="polite"]')).toBeTruthy()
  })

  it('agrega aviso al hacer clic', async () => {
    const user = (await import('@testing-library/user-event')).default.setup()
    renderizar(
      <ProveedorAvisos>
        <ComponenteConAviso texto="Guardado" />
      </ProveedorAvisos>,
    )
    await user.click(screen.getByRole('button', { name: 'Avisar' }))
    expect(screen.getByText('Guardado')).toBeTruthy()
  })

  it('aviso de error tiene role=alert', async () => {
    function ComponenteError() {
      const { agregar } = useAvisos()
      return <button onClick={() => agregar({ tipo: 'error', texto: 'Fallo' })}>Error</button>
    }
    const user = (await import('@testing-library/user-event')).default.setup()
    renderizar(<ProveedorAvisos><ComponenteError /></ProveedorAvisos>)
    await user.click(screen.getByRole('button', { name: 'Error' }))
    expect(screen.getByRole('alert')).toBeTruthy()
  })
})
