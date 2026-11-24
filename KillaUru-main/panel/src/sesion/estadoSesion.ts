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
    case 'login_ok': {
      // El correo llega aparte (GET /usuarios/yo): renovar conserva el que ya se conocía
      const { expiraAccesoUtc, sesionVenceUtc, desfaseMs } = accion
      return { tipo: 'activa', expiraAccesoUtc, sesionVenceUtc, desfaseMs, correo: estado.tipo === 'activa' ? estado.correo : undefined }
    }
    case 'renovar_401':
      // «sesion» solo si había una sesión que se perdió (el login lo explica); al abrir sin cookie no hay motivo.
      // Ya sin sesión, un segundo 401 (la renovación y el reintento avisan los dos) no cambia nada ni borra el motivo.
      if (estado.tipo === 'anonima') return estado
      return { tipo: 'anonima', motivo: estado.tipo === 'activa' ? 'sesion' : undefined }
    case 'salir':
      return { tipo: 'anonima', motivo: 'salida' }
    case 'correo_ok':
      return estado.tipo === 'activa' ? { ...estado, correo: accion.correo } : estado
  }
}
