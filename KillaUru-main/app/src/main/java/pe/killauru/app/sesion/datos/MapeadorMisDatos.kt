package pe.killauru.app.sesion.datos

import pe.killauru.app.sesion.dominio.Aceptacion
import pe.killauru.app.sesion.dominio.DocumentoLegal
import pe.killauru.app.sesion.dominio.MisDatos
import pe.killauru.app.sesion.dominio.Rol
import java.time.Instant

/** `MisDatosDto` (contrato 003) → [MisDatos]. Un documento que la app no conoce se omite en vez de romper. */
internal object MapeadorMisDatos {
    fun aDominio(dto: MisDatosDto) = MisDatos(
        correo = dto.correo,
        rol = Rol.valueOf(dto.rol),
        fechaAltaUtcMs = Instant.parse(dto.fechaAltaUtc).toEpochMilli(),
        correoVerificado = dto.correoVerificado,
        aceptaciones = dto.aceptaciones.mapNotNull { aceptacion ->
            val documento = DocumentoLegal.entries.firstOrNull { it.name == aceptacion.documento } ?: return@mapNotNull null
            Aceptacion(documento, aceptacion.version, Instant.parse(aceptacion.aceptadoUtc).toEpochMilli())
        },
    )
}
