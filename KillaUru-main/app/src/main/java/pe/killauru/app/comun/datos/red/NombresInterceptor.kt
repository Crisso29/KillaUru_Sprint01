package pe.killauru.app.comun.datos.red

/**
 * Nombres (`@Named`) con que cada funcionalidad entrega su interceptor a `RedModulo`. Así `comun` arma el cliente
 * HTTP sin depender de sesión ni de seudónimo. Constantes para no repetir el texto (y sus erratas) en cada módulo.
 */
internal object NombresInterceptor {
    const val TOKEN = "interceptor_token"
    const val SEUDONIMO = "interceptor_seudonimo"
    const val RENOVADOR = "interceptor_renovador"
}
