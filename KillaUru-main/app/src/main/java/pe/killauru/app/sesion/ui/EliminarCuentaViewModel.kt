package pe.killauru.app.sesion.ui

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.launch
import pe.killauru.app.sesion.dominio.ResultadoCuenta
import pe.killauru.app.sesion.dominio.casos.EliminarCuentaUseCase
import javax.inject.Inject

@HiltViewModel
internal class EliminarCuentaViewModel @Inject constructor(
    private val eliminar: EliminarCuentaUseCase,
) : ViewModel() {
    fun eliminar(contrasena: String, onEliminada: () -> Unit) {
        viewModelScope.launch {
            if (eliminar.ejecutar(contrasena) is ResultadoCuenta.Exitoso) onEliminada()
        }
    }
}
