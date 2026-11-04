package pe.killauru.app.sesion.dominio

/** FR-060, D13: validación local de longitud (10–128); el servidor decide el registro final. */
internal class ValidadorContrasena(
    private val minimo: Int = 10,
    private val maximo: Int = 128,
) {
    fun validar(contrasena: String): Boolean = contrasena.length in minimo..maximo

    companion object {
        fun esValida(contrasena: String): Boolean = ValidadorContrasena().validar(contrasena)
    }
}
