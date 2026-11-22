package pe.killauru.app.comun.ui

import android.net.Uri

/**
 * FR-008: rutas de navegación de NavGrafo. Los pasos del primer uso (bienvenida, términos, vehículo, permisos, cuenta)
 * no son rutas: son estados de `PrimerUsoViewModel`, que decide el paso según lo ya guardado.
 */
internal sealed class Rutas(val ruta: String) {
    data object PrimerUso : Rutas("primer_uso")
    data object Mapa : Rutas("mapa")
    data object Detalle : Rutas("detalle/{$ARG_ANOMALIA}") {
        fun con(id: String) = "detalle/$id"
    }
    data object Historial : Rutas("historial")
    data object Ajustes : Rutas("ajustes")

    // Cuenta (FR-060…FR-069): pantallas encima de las pestañas, sin barra inferior.
    data object Login : Rutas("login")
    data object Registro : Rutas("registro")
    data object VerificarCorreo : Rutas("verificar_correo/{$ARG_CORREO}") {
        /** El correo va codificado: un «+» o una «/» no deben romper ni cambiar la ruta. */
        fun con(correo: String) = "verificar_correo/${Uri.encode(correo)}"
    }
    data object OlvideContrasena : Rutas("olvide_contrasena")
    data object Perfil : Rutas("perfil")
    data object CerrarSesion : Rutas("cerrar_sesion")
    data object EliminarCuenta : Rutas("eliminar_cuenta")

    companion object {
        const val ARG_ANOMALIA = "anomaliaId"
        const val ARG_CORREO = "correo"
    }
}

/** A dónde ir al terminar el primer uso, según lo que eligió el conductor en «¿Crear una cuenta?». */
internal enum class TrasPrimerUso { MAPA, CREAR_CUENTA, INICIAR_SESION }
