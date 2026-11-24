// T049 — a qué pantalla lleva cada búsqueda rápida (FR-040)
import type { IntentoBusqueda } from '@/funcionalidades/mapa'

/** La ruta a abrir, o `null` si el texto no es un id, un km ni un correo. Los valores van codificados en la dirección. */
export function rutaDeBusqueda(intento: IntentoBusqueda): string | null {
  switch (intento.tipo) {
    case 'id': return `/mapa?anomalia=${encodeURIComponent(intento.id)}`
    case 'id-prefijo': return `/mapa?anomalia=${encodeURIComponent(intento.prefijo)}`
    case 'km': return `/mapa?kmVia=${intento.km}`
    case 'correo': return `/usuarios?busqueda=${encodeURIComponent(intento.correo)}`
    case 'sin_resultados': return null
  }
}
