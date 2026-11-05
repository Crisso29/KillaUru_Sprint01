// T017 — build raíz del grabador (solo configuración de plugins)
plugins {
    alias(libs.plugins.android.application) apply false
    alias(libs.plugins.kotlin.android) apply false
}
