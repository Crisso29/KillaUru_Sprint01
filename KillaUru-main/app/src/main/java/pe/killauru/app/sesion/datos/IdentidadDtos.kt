package pe.killauru.app.sesion.datos

import kotlinx.serialization.Serializable

/*
 * JSON de cuenta con los nombres exactos de `identidad.openapi.yaml` (contrato 003). Solo forma: la traducción al
 * dominio está en MapeadorSesion. ContratoIdentidadTest valida estas peticiones contra los esquemas del YAML.
 */

/** `RegistroSolicitud`. */
@Serializable
internal data class RegistroSolicitudDto(
    val correo: String,
    val contrasena: String,
    val versionPoliticaAceptada: String,
    val versionTerminosAceptada: String,
)

/**
 * `InicioSesionSolicitud`: la app siempre usa `canal = app` y no envía `turnstileToken` (solo panel).
 * Sin valor por defecto a propósito: kotlinx.serialization no envía los campos que valen su valor por defecto,
 * y `canal` es obligatorio en el contrato (lo detectó ContratoIdentidadTest).
 */
@Serializable
internal data class InicioSesionSolicitudDto(val correo: String, val contrasena: String, val canal: String)

/** `RenovarSolicitud` (solo la app; el panel usa la cookie). */
@Serializable
internal data class RenovarSolicitudDto(val tokenRenovacion: String)

/** Cuerpo de `DELETE /api/v1/me` (FR-067). */
@Serializable
internal data class ConfirmacionContrasenaDto(val contrasena: String)

/** `SesionApp`: fechas en ISO-8601 UTC. */
@Serializable
internal data class SesionAppDto(
    val tokenAcceso: String,
    val expiraAccesoUtc: String,
    val tokenRenovacion: String,
    val sesionVenceUtc: String,
    val rol: String,
)

/** `POST /auth/recuperar` (contrato 003). */
@Serializable
internal data class RecuperacionSolicitudDto(val correo: String)

/** Contrato 003 `MisDatos.aceptaciones[]`. */
@Serializable
internal data class AceptacionDto(val documento: String, val version: String, val aceptadoUtc: String)

/** Contrato 003 `MisDatos` (`GET /api/v1/me`). */
@Serializable
internal data class MisDatosDto(
    val correo: String,
    val rol: String,
    val fechaAltaUtc: String,
    val correoVerificado: Boolean,
    val aceptaciones: List<AceptacionDto>,
)
