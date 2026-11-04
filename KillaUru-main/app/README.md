# App Android KillaUru

Implementación del [spec 008](../specs/008-app-android/spec.md), en curso.
El estado verificable y los requisitos pendientes están en
[implementacion.md](../specs/008-app-android/implementacion.md).

## Desarrollo

Requisitos: JDK 17, Android SDK 35 y acceso a Maven Central/Google Maven.
Definir `JAVA_HOME` y `ANDROID_HOME`, o configurar el SDK en un `local.properties`
local (no se versiona). Usar el wrapper Gradle 9.7.1 confirmado por el autor.

Desde esta carpeta:

```powershell
.\gradlew.bat testDebugUnitTest
.\gradlew.bat detekt koverVerify
.\gradlew.bat assembleDebug lint
python -m unittest discover -s herramientas -p 'test_*.py'
python herramientas/verificar_lineas.py
```

En Linux/macOS, usar `bash gradlew` en lugar de `.\gradlew.bat`.
Informes: `build/reports/tests/`, `build/reports/detekt/` y `build/reports/kover/`.
El CI es [app-ci.yml](../../.github/workflows/app-ci.yml).

## Diseño y pruebas

Un módulo Gradle con paquetes por funcionalidad. Presentación y datos dependen
del dominio; el dominio no depende de Android, Room, HTTP ni interfaces de usuario.
Cada regla recibe sus umbrales y tiempo: no consulta el reloj de pared.
Las pruebas siguen Arrange / Act / Assert y citan tareas y requisitos.

La app consume los contratos de Identidad (003), Ingesta (005), Mapa (006) y CDN
(008). `copiarContratos` los lleva a recursos generados del build para verificarlos
sin mantener copias manuales. La app no publica endpoints.

La constitución exige fuentes de menos de 200 líneas, métodos de hasta 20 líneas,
cobertura de dominio de al menos 90 %, persistencia sin migraciones destructivas
y privacidad de los datos locales. No se deben desactivar controles para cerrar
una tarea que todavía no tiene evidencia.
