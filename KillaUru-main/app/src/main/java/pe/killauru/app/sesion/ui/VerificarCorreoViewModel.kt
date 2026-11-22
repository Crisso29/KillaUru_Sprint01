package pe.killauru.app.sesion.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import pe.killauru.app.sesion.dominio.ApiCuenta
import javax.inject.Inject

/**
 * FR-064 (prototipo «Revisa tu correo»): «Reenviar el correo» pide otro enlace una sola vez por pantalla, para no
 * gastar el límite del servidor con toques repetidos.
 */
@HiltViewModel
internal class VerificarCorreoViewModel @Inject constructor(private val api: ApiCuenta) : ViewModel() {

    private val _reenviado = MutableStateFlow(false)
    val reenviado: StateFlow<Boolean> = _reenviado

    fun reenviar() {
        if (_reenviado.value) return
        _reenviado.value = true
        viewModelScope.launch { if (!api.reenviarVerificacion()) _reenviado.value = false }
    }
}
