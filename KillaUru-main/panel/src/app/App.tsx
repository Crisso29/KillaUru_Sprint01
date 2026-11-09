import { BrowserRouter } from 'react-router'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ProveedorSesion } from '@/sesion/ProveedorSesion'
import { ProveedorAvisos } from '@/compartido/ui'
import { Rutas } from './rutas'

const queryClient = new QueryClient()

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ProveedorSesion>
        <ProveedorAvisos>
          <BrowserRouter>
            <Rutas />
          </BrowserRouter>
        </ProveedorAvisos>
      </ProveedorSesion>
    </QueryClientProvider>
  )
}
