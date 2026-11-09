package pe.killauru.app.sincronizacion.datos

import android.content.Context
import android.net.ConnectivityManager
import android.net.NetworkCapabilities
import javax.inject.Inject

/** FR-036: comprueba conectividad antes de intentar sincronización. */
internal class VigiaRed @Inject constructor(@dagger.hilt.android.qualifiers.ApplicationContext private val context: Context) {

    fun hayConexion(): Boolean {
        val cm = context.getSystemService(ConnectivityManager::class.java) ?: return false
        val red = cm.activeNetwork ?: return false
        val caps = cm.getNetworkCapabilities(red) ?: return false
        return caps.hasCapability(NetworkCapabilities.NET_CAPABILITY_INTERNET)
    }
}
