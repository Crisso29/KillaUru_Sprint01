package pe.killauru.app.sesion.dominio

/** FR-060: validación local de longitud mínima; el servidor sigue decidiendo el registro. */
internal object ValidadorContrasena {
    fun esValida(contrasena: String): Boolean = contrasena.length >= 8
}
