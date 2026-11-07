import { type ReactElement } from 'react'
import { render, type RenderOptions } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router'

// Proveedor mínimo para pruebas de componentes
function ProveedorPrueba({ children }: { children: React.ReactNode }) {
  const cliente = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })
  return (
    <QueryClientProvider client={cliente}>
      <MemoryRouter>{children}</MemoryRouter>
    </QueryClientProvider>
  )
}

function renderizar(ui: ReactElement, opciones?: Omit<RenderOptions, 'wrapper'>) {
  return render(ui, { wrapper: ProveedorPrueba, ...opciones })
}

export * from '@testing-library/react'
export { renderizar }
