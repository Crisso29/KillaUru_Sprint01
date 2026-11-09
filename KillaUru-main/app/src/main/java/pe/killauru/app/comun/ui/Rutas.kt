package pe.killauru.app.comun.ui

/** FR-008: rutas de navegación tipadas para NavGrafo. */
internal sealed class Rutas(val ruta: String) {
    data object Bienvenida : Rutas("bienvenida")
    data object Terminos : Rutas("terminos")
    data object Vehiculo : Rutas("vehiculo")
    data object Permisos : Rutas("permisos")
    data object CuentaONo : Rutas("cuenta_o_no")
    data object Mapa : Rutas("mapa")
    data object Detalle : Rutas("detalle/{anomaliaId}") {
        fun con(id: String) = "detalle/$id"
    }
    data object Ajustes : Rutas("ajustes")
    data object Historial : Rutas("historial")
    data object Login : Rutas("login")
    data object Registro : Rutas("registro")
    data object VerificarCorreo : Rutas("verificar_correo")
    data object OlvideContrasena : Rutas("olvide_contrasena")
    data object Perfil : Rutas("perfil")
    data object EliminarCuenta : Rutas("eliminar_cuenta")
    data object DescargaMapa : Rutas("descarga_mapa")
}
