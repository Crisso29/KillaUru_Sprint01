package pe.killauru.app.sesion.datos

import pe.killauru.app.sesion.dominio.PermisoLocal
import pe.killauru.app.sesion.dominio.Rol
import pe.killauru.app.sesion.dominio.Sesion
import pe.killauru.app.sesion.dominio.SolicitudRegistro
import java.time.Instant

/** Traduce entre el dominio de sesión y el JSON del contrato 003; aquí viven las conversiones de fechas y nombres. */
internal object MapeadorSesion {

    fun aDto(solicitud: SolicitudRegistro) = RegistroSolicitudDto(
        correo = solicitud.correo,
        contrasena = solicitud.contrasena,
        versionPoliticaAceptada = solicitud.versionPoliticaAceptada,
        versionTerminosAceptada = solicitud.versionTerminosAceptada,
    )

    /** `sesionVenceUtc` es el vencimiento del permiso local (FR-063, RN-21 a). */
    fun aDominio(dto: SesionAppDto) = Sesion(
        tokenAcceso = dto.tokenAcceso,
        expiraAccesoMs = Instant.parse(dto.expiraAccesoUtc).toEpochMilli(),
        tokenRenovacion = dto.tokenRenovacion,
        permiso = PermisoLocal(venceMs = Instant.parse(dto.sesionVenceUtc).toEpochMilli()),
        rol = Rol.valueOf(dto.rol),
    )
}
