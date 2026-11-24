// T056 — la búsqueda de usuarios espera 400 ms desde la última tecla: no se pide una página por cada letra
import { useCallback, useEffect, useRef, useState } from 'react'

const ESPERA_MS = 400

/** Devuelve el texto que se ve, el que se busca y cómo cambiarlos; `alBuscar` corre cuando cambia lo buscado. */
export function useBusquedaDiferida(alBuscar: () => void) {
  const [texto, setTexto] = useState('')
  const [buscado, setBuscado] = useState('')
  const temporizador = useRef<ReturnType<typeof setTimeout> | null>(null)
  useEffect(() => () => { if (temporizador.current) clearTimeout(temporizador.current) }, [])
  const buscarYa = useCallback((valor: string) => { alBuscar(); setBuscado(valor) }, [alBuscar])
  const escribir = useCallback((valor: string) => {
    setTexto(valor)
    if (temporizador.current) clearTimeout(temporizador.current)
    temporizador.current = setTimeout(() => buscarYa(valor), ESPERA_MS)
  }, [buscarYa])
  return { texto, buscado, escribir, buscarYa: () => buscarYa(texto) }
}
