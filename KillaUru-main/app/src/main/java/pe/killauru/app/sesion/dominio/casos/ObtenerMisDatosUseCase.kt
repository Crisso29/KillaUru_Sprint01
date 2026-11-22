package pe.killauru.app.sesion.dominio.casos

import pe.killauru.app.sesion.dominio.ApiCuenta
import pe.killauru.app.sesion.dominio.MisDatos
import javax.inject.Inject

/** FR-068, HU-24: todo lo que KillaUru guarda de la cuenta, para «Mi perfil». Null si no se pudo obtener. */
internal class ObtenerMisDatosUseCase @Inject constructor(private val api: ApiCuenta) {
    suspend fun ejecutar(): MisDatos? = api.misDatos()
}
