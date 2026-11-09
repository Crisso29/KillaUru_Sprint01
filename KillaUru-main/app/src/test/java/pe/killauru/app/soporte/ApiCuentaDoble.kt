package pe.killauru.app.soporte

import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.ResultadoCuenta

internal class ApiCuentaDoble(var resultado: ResultadoCuenta) : ApiCuenta {
    var elimina = true
    var eliminaciones = 0
    override suspend fun registrar(correo: String, contrasena: String) = resultado
    override suspend fun login(correo: String, contrasena: String) = resultado
    override suspend fun solicitarVerificacion(correo: String) = Unit
    override suspend fun eliminarCuenta(usuarioId: String, token: String): Boolean {
        eliminaciones++
        return elimina
    }
}
