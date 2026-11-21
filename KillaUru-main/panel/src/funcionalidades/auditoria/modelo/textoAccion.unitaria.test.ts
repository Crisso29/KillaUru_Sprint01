// T078 — textoDeAccion y textoDeCategoria
import { describe, it, expect } from 'vitest'
import { textoDeAccion, textoDeCategoria } from './textoAccion'

describe('textoDeAccion — 14 acciones', () => {
  it('UsuarioCreado', () => expect(textoDeAccion('UsuarioCreado')).toBe('Usuario creado'))
  it('RolCambiado', () => expect(textoDeAccion('RolCambiado')).toBe('Rol cambiado'))
  it('CuentaDesactivada', () => expect(textoDeAccion('CuentaDesactivada')).toBe('Cuenta desactivada'))
  it('CuentaActivada', () => expect(textoDeAccion('CuentaActivada')).toBe('Cuenta activada'))
  it('CuentaEliminada', () => expect(textoDeAccion('CuentaEliminada')).toBe('Cuenta eliminada'))
  it('InicioSesionFallido', () => expect(textoDeAccion('InicioSesionFallido')).toBe('Inicio de sesión fallido'))
  it('CuentaBloqueada', () => expect(textoDeAccion('CuentaBloqueada')).toBe('Cuenta bloqueada'))
  it('InicioSesionAdministrador', () => expect(textoDeAccion('InicioSesionAdministrador')).toBe('Inicio de sesión (administrador)'))
  it('RecuperacionSolicitada', () => expect(textoDeAccion('RecuperacionSolicitada')).toBe('Recuperación solicitada'))
  it('ContrasenaCambiada', () => expect(textoDeAccion('ContrasenaCambiada')).toBe('Contraseña cambiada'))
  it('RevisionPrivilegios', () => expect(textoDeAccion('RevisionPrivilegios')).toBe('Revisión de privilegios'))
  it('DatasetExportado', () => expect(textoDeAccion('DatasetExportado')).toBe('Dataset exportado'))
  it('AccesoAdministrativoDatos', () => expect(textoDeAccion('AccesoAdministrativoDatos')).toBe('Acceso administrativo a datos'))

  it('AnomaliaModerada sin contexto → texto genérico', () => {
    expect(textoDeAccion('AnomaliaModerada')).toBe('Anomalía moderada')
  })

  it('AnomaliaModerada contexto.accion=MarcarReparada', () => {
    expect(textoDeAccion('AnomaliaModerada', { accion: 'MarcarReparada' }))
      .toBe('Anomalía marcada como reparada')
  })

  it('AnomaliaModerada contexto.accion=Descartar', () => {
    expect(textoDeAccion('AnomaliaModerada', { accion: 'Descartar' }))
      .toBe('Anomalía descartada')
  })

  it('AnomaliaModerada contexto.accion=Reabrir', () => {
    expect(textoDeAccion('AnomaliaModerada', { accion: 'Reabrir' }))
      .toBe('Anomalía reabierta')
  })

  it('AnomaliaModerada contexto.accion=MantenerDescartada', () => {
    expect(textoDeAccion('AnomaliaModerada', { accion: 'MantenerDescartada' }))
      .toBe('Anomalía mantenida como descartada')
  })
})

describe('textoDeCategoria — 6 categorías', () => {
  it('Usuarios', () => expect(textoDeCategoria('Usuarios')).toBe('Usuarios'))
  it('Sesion', () => expect(textoDeCategoria('Sesion')).toBe('Sesión'))
  it('CuentaDelTitular', () => expect(textoDeCategoria('CuentaDelTitular')).toBe('Cuenta del titular'))
  it('Moderacion', () => expect(textoDeCategoria('Moderacion')).toBe('Moderación'))
  it('Exportacion', () => expect(textoDeCategoria('Exportacion')).toBe('Exportación'))
  it('AccesoADatos', () => expect(textoDeCategoria('AccesoADatos')).toBe('Acceso a datos'))
})
