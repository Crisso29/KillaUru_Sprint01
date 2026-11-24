// T056 — estado de los filtros de Usuarios: búsqueda diferida, rol, estado y página. Cambiar un filtro vuelve a la 1.
import { useCallback, useState } from 'react'
import type { Rol, EstadoUsuario } from '@/api/dtos/001-identidad'
import { aConsulta } from '../modelo/consulta'
import { useBusquedaDiferida } from './useBusquedaDiferida'

export function useFiltrosUsuarios() {
  const [rol, setRol] = useState<Rol | ''>('')
  const [estado, setEstado] = useState<EstadoUsuario | ''>('')
  const [pagina, setPagina] = useState(1)
  const volverAlInicio = useCallback(() => setPagina(1), [])
  const busqueda = useBusquedaDiferida(volverAlInicio)
  const cambiarCampo = (campo: 'texto' | 'rol' | 'estado', valor: string) => {
    if (campo === 'texto') return busqueda.escribir(valor)
    setPagina(1)
    if (campo === 'rol') setRol(valor as Rol | '')
    else setEstado(valor as EstadoUsuario | '')
  }
  return { rol, estado, pagina, setPagina, busqueda, cambiarCampo, consulta: aConsulta(busqueda.buscado, rol, estado, pagina) }
}
