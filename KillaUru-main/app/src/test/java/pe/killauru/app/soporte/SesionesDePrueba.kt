package pe.killauru.app.soporte

import pe.killauru.app.sesion.dominio.PermisoLocal
import pe.killauru.app.sesion.dominio.Rol
import pe.killauru.app.sesion.dominio.Sesion

/** Sesiones con la forma de `SesionApp` del contrato 003, para no repetir los cinco campos en cada prueba. */
internal object SesionesDePrueba {
    fun de(rol: Rol, tokenAcceso: String = "acceso-1") = Sesion(
        tokenAcceso = tokenAcceso,
        expiraAccesoMs = 1_000L,
        tokenRenovacion = "renovacion-1",
        permiso = PermisoLocal(venceMs = 2_000L),
        rol = rol,
    )
}
