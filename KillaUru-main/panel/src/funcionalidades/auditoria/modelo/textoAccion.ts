import type { AccionAuditada, CategoriaAuditoria } from '@/api/generado/002-auditoria'

const TEXTOS_ACCION: Record<AccionAuditada, string> = {
  UsuarioCreado: 'Usuario creado',
  RolCambiado: 'Rol cambiado',
  CuentaDesactivada: 'Cuenta desactivada',
  CuentaActivada: 'Cuenta activada',
  CuentaEliminada: 'Cuenta eliminada',
  InicioSesionFallido: 'Inicio de sesión fallido',
  CuentaBloqueada: 'Cuenta bloqueada',
  InicioSesionAdministrador: 'Inicio de sesión (administrador)',
  RecuperacionSolicitada: 'Recuperación solicitada',
  ContrasenaCambiada: 'Contraseña cambiada',
  RevisionPrivilegios: 'Revisión de privilegios',
  AnomaliaModerada: 'Anomalía moderada',
  DatasetExportado: 'Dataset exportado',
  AccesoAdministrativoDatos: 'Acceso administrativo a datos',
}

const TEXTOS_ANOMALIA: Record<string, string> = {
  MarcarReparada: 'Anomalía marcada como reparada',
  Descartar: 'Anomalía descartada',
  Reabrir: 'Anomalía reabierta',
  MantenerDescartada: 'Anomalía mantenida como descartada',
}

const TEXTOS_CATEGORIA: Record<CategoriaAuditoria, string> = {
  Usuarios: 'Usuarios',
  Sesion: 'Sesión',
  CuentaDelTitular: 'Cuenta del titular',
  Moderacion: 'Moderación',
  Exportacion: 'Exportación',
  AccesoADatos: 'Acceso a datos',
}

export function textoDeAccion(
  accion: AccionAuditada,
  contexto?: Record<string, unknown>,
): string {
  if (accion === 'AnomaliaModerada' && contexto?.accion) {
    const texto = TEXTOS_ANOMALIA[contexto.accion as string]
    if (texto) return texto
  }
  return TEXTOS_ACCION[accion] ?? accion
}

export function textoDeCategoria(categoria: CategoriaAuditoria): string {
  return TEXTOS_CATEGORIA[categoria] ?? categoria
}
