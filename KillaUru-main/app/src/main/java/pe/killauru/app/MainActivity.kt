package pe.killauru.app

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import pe.killauru.app.comun.ui.tema.KillaUruTema
import dagger.hilt.android.AndroidEntryPoint
import pe.killauru.app.comun.ui.NavGrafo

/** T024: punto de entrada Android; Compose + NavGrafo. */
@AndroidEntryPoint
internal class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            KillaUruTema {
                NavGrafo(PantallasReales)
            }
        }
    }
}
