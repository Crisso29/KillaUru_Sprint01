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
    // exponerParaPruebas primero: con serviceWorkers:'block' en Playwright el SW no llega a activarse;
    // si esperásemos a worker.start(), __killauruSimulado nunca se fijaría y abrir() agotaría el tiempo.
    exponerParaPruebas()
    worker.start({ onUnhandledRequest: 'bypass' }).catch(() => {})
  }
  createRoot(raiz!).render(<StrictMode><App /></StrictMode>)
}

void iniciar()
