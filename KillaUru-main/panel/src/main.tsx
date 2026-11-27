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
    // Firefox: clients.claim() puede tardar; esperar que el SW controle la página
    // antes de exponer el simulador (y que React empiece a hacer peticiones).
    if (!navigator.serviceWorker.controller) {
      await new Promise<void>((resolve) => {
        navigator.serviceWorker.addEventListener('controllerchange', () => resolve(), { once: true })
      })
    }
    exponerParaPruebas()
  }
  createRoot(raiz!).render(<StrictMode><App /></StrictMode>)
}

void iniciar()
