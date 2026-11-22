package pe.killauru.app.sesion.ui

import android.Manifest.permission.ACCESS_BACKGROUND_LOCATION
import android.Manifest.permission.ACCESS_COARSE_LOCATION
import android.Manifest.permission.ACCESS_FINE_LOCATION
import android.annotation.SuppressLint

/**
 * FR-006: regla de la ubicación en dos pasos. Desde Android 11 el sistema ignora «todo el tiempo» si se pide junto con
 * la precisa: primero la precisa y, ya concedida, la de segundo plano, que existe desde Android 10 (API 29).
 */
internal object PermisosUbicacion {
    private const val API_SEGUNDO_PLANO = 29

    // InlinedApi: el permiso de segundo plano solo se devuelve si sdk >= 29 (lo garantiza necesitaFondo y su prueba).
    @SuppressLint("InlinedApi")
    fun siguientes(tienePrecisa: Boolean, tieneFondo: Boolean, sdk: Int): Array<String> = when {
        !tienePrecisa -> arrayOf(ACCESS_FINE_LOCATION, ACCESS_COARSE_LOCATION)
        necesitaFondo(tieneFondo, sdk) -> arrayOf(ACCESS_BACKGROUND_LOCATION)
        else -> emptyArray()
    }

    fun concedida(tienePrecisa: Boolean, tieneFondo: Boolean, sdk: Int) = tienePrecisa && !necesitaFondo(tieneFondo, sdk)

    private fun necesitaFondo(tieneFondo: Boolean, sdk: Int) = sdk >= API_SEGUNDO_PLANO && !tieneFondo
}
