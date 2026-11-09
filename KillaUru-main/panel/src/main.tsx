import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import '@/compartido/estilos/tokens.css'
import '@/compartido/estilos/base.css'
import '@/compartido/estilos/fuentes.css'
import { App } from '@/app/App'

const raiz = document.getElementById('raiz')
if (!raiz) throw new Error('Elemento #raiz no encontrado')
async function iniciar() {
  if (__SIMULADO__) {
    const { worker } = await import('../pruebas/msw/navegador')
    await worker.start({ onUnhandledRequest: 'bypass' })
  }
  createRoot(raiz!).render(<StrictMode><App /></StrictMode>)
}

void iniciar()
