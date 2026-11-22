package pe.killauru.app.sesion.dominio

/** FR-060, HU-22, HU-27: datos del registro, con las versiones legales que el usuario aceptó en la app. */
internal data class SolicitudRegistro(
    val correo: String,
    val contrasena: String,
    val versionPoliticaAceptada: String,
    val versionTerminosAceptada: String,
)
