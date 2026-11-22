package pe.killauru.app.sesion.datos

import okhttp3.HttpUrl

/**
 * Rutas de `/api/v1/auth/` que son públicas en el contrato 003 (registro, inicio de sesión, renovar, verificar, recuperar).
 * No deben llevar el token de acceso: la API responde 401 a un token vencido incluso en una ruta pública, y eso haría
 * imposible renovarlo. `cerrar-sesion` es la excepción: exige sesión.
 */
internal object RutasAutenticacion {
    private const val PREFIJO = "/api/v1/auth/"
    private const val CERRAR_SESION = "/api/v1/auth/cerrar-sesion"

    fun esPublica(url: HttpUrl): Boolean = url.encodedPath.startsWith(PREFIJO) && url.encodedPath != CERRAR_SESION
}
