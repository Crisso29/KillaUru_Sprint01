// Una sola instancia del simulador compartida por los manejadores del mapa, el detalle y la moderación.
// En el navegador (modo simulado) dura hasta recargar la página; en Vitest se reinicia después de cada prueba.
import { crearSimulador } from './anomaliasSimuladas'

export let simulador = crearSimulador()

export function reiniciarSimulador() {
  simulador = crearSimulador()
}
