// T078 — textoDeAccion y textoDeCategoria: cada código del contrato 002 en lenguaje claro
import { describe, it, expect } from 'vitest'
import { textoDeAccion, textoDeCategoria } from './textoAccion'
import type { AccionAuditada, CategoriaAuditoria } from '@/api/dtos/002-auditoria'

describe('textoDeAccion — 14 acciones', () => {
  it.each<[AccionAuditada, string]>([
    ['UsuarioCreado', 'Usuario creado'],
    ['RolCambiado', 'Rol cambiado'],
    ['CuentaDesactivada', 'Cuenta desactivada'],
    ['CuentaActivada', 'Cuenta activada'],
    ['CuentaEliminada', 'Cuenta eliminada'],
    ['InicioSesionFallido', 'Inicio de sesión fallido'],
    ['CuentaBloqueada', 'Cuenta bloqueada'],
    ['InicioSesionAdministrador', 'Inicio de sesión (administrador)'],
    ['RecuperacionSolicitada', 'Recuperación solicitada'],
    ['ContrasenaCambiada', 'Contraseña cambiada'],
    ['RevisionPrivilegios', 'Revisión de privilegios'],
    ['AnomaliaModerada', 'Anomalía moderada'],
    ['DatasetExportado', 'Dataset exportado'],
    ['AccesoAdministrativoDatos', 'Acceso administrativo a datos'],
  ])('%s → «%s»', (accion, esperado) => {
    // Arrange y Act
    const texto = textoDeAccion(accion)

    // Assert
    expect(texto).toBe(esperado)
  })

  it.each([
    ['MarcarReparada', 'Anomalía marcada como reparada'],
    ['Descartar', 'Anomalía descartada'],
    ['Reabrir', 'Anomalía reabierta'],
    ['MantenerDescartada', 'Anomalía mantenida como descartada'],
  ])('AnomaliaModerada con contexto.accion=%s → «%s»', (accionModeracion, esperado) => {
    // Arrange
    const contexto = { accion: accionModeracion }

    // Act
    const texto = textoDeAccion('AnomaliaModerada', contexto)

    // Assert
    expect(texto).toBe(esperado)
  })
})

describe('textoDeCategoria — 6 categorías', () => {
  it.each<[CategoriaAuditoria, string]>([
    ['Usuarios', 'Usuarios'],
    ['Sesion', 'Sesión'],
    ['CuentaDelTitular', 'Cuenta del titular'],
    ['Moderacion', 'Moderación'],
    ['Exportacion', 'Exportación'],
    ['AccesoADatos', 'Acceso a datos'],
  ])('%s → «%s»', (categoria, esperado) => {
    // Arrange y Act
    const texto = textoDeCategoria(categoria)

    // Assert
    expect(texto).toBe(esperado)
  })
})
