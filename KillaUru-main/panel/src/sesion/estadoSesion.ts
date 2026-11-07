// T023 — máquina de estados de sesión (data-model.md §1)

export type EstadoSesion =
  | { tipo: 'desconocida' }
  | { tipo: 'anonima'; motivo?: 'sesion' | 'salida' }
  | { tipo: 'activa'; expiraAccesoUtc: Date; sesionVenceUtc: Date; desfaseMs: number; correo?: string }

export type AccionSesion =
  | { tipo: 'renovar_ok'; expiraAccesoUtc: Date; sesionVenceUtc: Date; desfaseMs: number }
  | { tipo: 'renovar_401' }
  | { tipo: 'login_ok'; expiraAccesoUtc: Date; sesionVenceUtc: Date; desfaseMs: number }
  | { tipo: 'salir' }
  | { tipo: 'correo_ok'; correo: string }

export function reducirSesion(estado: EstadoSesion, accion: AccionSesion): EstadoSesion {
  switch (accion.tipo) {
    case 'renovar_ok':
    case 'login_ok':
      return {
        tipo: 'activa',
        expiraAccesoUtc: accion.expiraAccesoUtc,
        sesionVenceUtc: accion.sesionVenceUtc,
        desfaseMs: accion.desfaseMs,
        correo: estado.tipo === 'activa' ? estado.correo : undefined,
      }
    case 'renovar_401':
      return {
        tipo: 'anonima',
        motivo: estado.tipo === 'activa' ? 'sesion' : undefined,
      }
    case 'salir':
      return { tipo: 'anonima', motivo: 'salida' }
    case 'correo_ok':
      if (estado.tipo !== 'activa') return estado
      return { ...estado, correo: accion.correo }
  }
}
