package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import javax.inject.Inject

/** FR-060: registra una nueva cuenta y retorna el resultado. */
internal class RegistrarCuentaUseCase @Inject constructor(
    private val api: ApiCuenta,
) {
    suspend fun ejecutar(correo: String, contrasena: String): ResultadoCuenta =
        api.registrar(correo, contrasena)
}
