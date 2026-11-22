package pe.killauru.app.comun.ui

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.autofill.ContentType
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.semantics.contentType
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.input.VisualTransformation
import androidx.compose.ui.unit.dp
import androidx.compose.runtime.remember
import androidx.compose.ui.focus.FocusRequester
import androidx.compose.ui.focus.focusRequester
import androidx.compose.ui.semantics.clearAndSetSemantics
import androidx.compose.ui.semantics.testTag
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.foundation.gestures.detectTapGestures
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.ui.unit.sp

/** Tipo de campo: decide teclado, ocultación y autocompletado (gestores de contraseñas). */
internal enum class TipoCampo(
    val oculto: Boolean,
    val teclado: KeyboardType,
    val accion: ImeAction,
) {
    CORREO(oculto = false, KeyboardType.Email, ImeAction.Next),
    CONTRASENA(oculto = true, KeyboardType.Password, ImeAction.Done),
    CONTRASENA_NUEVA(oculto = true, KeyboardType.Password, ImeAction.Done),
}

/** Qué ofrece el autocompletado de Android: la contraseña guardada al entrar; una nueva y segura al crear la cuenta. */
private fun TipoCampo.autocompletar(): ContentType = when (this) {
    TipoCampo.CORREO -> ContentType.EmailAddress + ContentType.Username
    TipoCampo.CONTRASENA -> ContentType.Password
    TipoCampo.CONTRASENA_NUEVA -> ContentType.NewPassword
}

/**
 * Campo del prototipo (`.field`): etiqueta encima, borde redondeado y una ayuda opcional debajo. La etiqueta visible no
 * se asocia sola al campo, así que se le da como nombre accesible (`contentDescription`): TalkBack lee «Correo, cuadro de texto»
 * una sola vez (la etiqueta no se lee aparte) y, como `<label for>` en la web, tocar la etiqueta enfoca el campo.
 */
@Composable
internal fun CampoFormulario(
    etiqueta: String,
    valor: String,
    alCambiar: (String) -> Unit,
    tipo: TipoCampo,
    modifier: Modifier = Modifier,
    ayuda: String? = null,
    alConfirmar: (() -> Unit)? = null,
) = Column(modifier.fillMaxWidth(), verticalArrangement = Arrangement.spacedBy(5.dp)) {
    val foco = remember { FocusRequester() }
    Etiqueta(etiqueta) { foco.requestFocus() }
    OutlinedTextField(
        value = valor,
        onValueChange = alCambiar,
        singleLine = true,
        shape = RoundedCornerShape(8.dp),
        visualTransformation = if (tipo.oculto) PasswordVisualTransformation() else VisualTransformation.None,
        // Con `alConfirmar`, la tecla ✓ del teclado envía el formulario (no hay que buscar el botón bajo el teclado).
        keyboardOptions = KeyboardOptions(keyboardType = tipo.teclado, imeAction = if (alConfirmar != null) ImeAction.Done else tipo.accion),
        keyboardActions = if (alConfirmar != null) KeyboardActions(onDone = { alConfirmar() }) else KeyboardActions.Default,
        modifier = Modifier.fillMaxWidth().focusRequester(foco)
            .semantics { contentType = tipo.autocompletar(); contentDescription = etiqueta },
    )
    if (ayuda != null) {
        Text(ayuda, style = MaterialTheme.typography.bodySmall, color = MaterialTheme.colorScheme.onSurfaceVariant)
    }
}

/** Texto visible de la etiqueta: fuera de la semántica (el campo ya lleva ese nombre) y, al tocarlo, enfoca el campo. */
@Composable
private fun Etiqueta(texto: String, alTocar: () -> Unit) = Text(
    texto,
    fontSize = 14.sp,
    fontWeight = FontWeight.SemiBold,
    color = MaterialTheme.colorScheme.onSurfaceVariant,
    modifier = Modifier.clearAndSetSemantics { testTag = "etiqueta:$texto" }
        .pointerInput(Unit) { detectTapGestures { alTocar() } },
)
