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
    // Firefox CI: CDP no establece controller; location.assign crea navegación nativa
    // que el SW sí intercepta. sessionStorage evita bucle si tampoco funciona la segunda vez.
    if (!navigator.serviceWorker.controller && !sessionStorage.getItem('sw_init')) {
      sessionStorage.setItem('sw_init', '1')
      location.assign(location.href)
      return
    }
    sessionStorage.removeItem('sw_init')
    exponerParaPruebas()
  }
  createRoot(raiz!).render(<StrictMode><App /></StrictMode>)
}

void iniciar()
