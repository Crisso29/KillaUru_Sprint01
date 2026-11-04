package pe.killauru.app.sesion.dominio

/** FR-060: validación local de longitud; el servidor sigue decidiendo el registro. */
internal class ValidadorContrasena(private val minimo: Int, private val maximo: Int) {
    fun validar(contrasena: String): Boolean = contrasena.length in minimo..maximo
}
