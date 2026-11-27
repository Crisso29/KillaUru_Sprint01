import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import '@/compartido/estilos/tokens.css'
import '@/compartido/estilos/base.css'
import '@/compartido/estilos/fuentes.css'
import '@/compartido/estilos/componentes.css'
import '@/compartido/estilos/ops.css'
import { App } from '@/app/App'

const raiz = document.getElementById('raiz')
if (!raiz) throw new Error('Elemento #raiz no encontrado')
async function iniciar() {
  if (__SIMULADO__) {
    const { worker, exponerParaPruebas } = await import('../pruebas/msw/navegador')
    await worker.start({ onUnhandledRequest: 'bypass' })
    // Firefox: clients.claim() puede tardar en tomar efecto; sondear con límite de 1s
    // para que el SW controle la página antes de que React haga peticiones API.
    for (let i = 0; i < 20 && !navigator.serviceWorker.controller; i++) {
      await new Promise<void>((r) => setTimeout(r, 50))
    }
    exponerParaPruebas()
  }
  createRoot(raiz!).render(<StrictMode><App /></StrictMode>)
}

void iniciar()
