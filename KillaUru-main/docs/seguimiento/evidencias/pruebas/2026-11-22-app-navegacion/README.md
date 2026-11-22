# App: primer uso y navegación en un emulador

> Recorrido manual de la app instalada, como un conductor nuevo. [Volver a evidencias](../../README.md).

| Dato | Valor |
|---|---|
| Fecha | 2026-11-22 |
| Entorno | Emulador `pixel_7_-_api_35` (Android 15), APK `KillaUru-debug.apk` instalado limpio (`pm clear`) |
| Código | Sobre `108dda3`, con la parte 1 de T024 (spec 008) |
| Criterio | FR-001, FR-004, FR-006, FR-008: el primer uso lleva al mapa; los permisos se piden al sistema; la barra cambia de pestaña |
| Pruebas automáticas del mismo comportamiento | `NavGrafoTest`, `PrimerUsoViewModelTest`, `PermisosUbicacionTest` (ver [pruebas de la spec 008](../../../../../specs/008-app-android/pruebas.md)) |

## Recorrido

| Paso | Captura | Resultado |
|---|---|---|
| 1. Abrir la app por primera vez | [01-bienvenida.png](01-bienvenida.png) | Bienvenida real (antes del cambio solo se veía el texto «KillaUru — Bienvenida») |
| 2. Términos, marcar las dos casillas | — | El botón se activa solo con las dos marcadas |
| 3. Vehículo | [02-vehiculo.png](02-vehiculo.png) | Se elige Liviano |
| 4. Ubicación, primer «Conceder» | [03-ubicacion-precisa.png](03-ubicacion-precisa.png) | Diálogo del sistema para la ubicación precisa |
| 5. Ubicación, segundo «Conceder» | [04-ubicacion-todo-el-tiempo.png](04-ubicacion-todo-el-tiempo.png) | Android 11+ lleva a sus ajustes para «todo el tiempo»; al volver la app muestra «Concedido» (`ACCESS_BACKGROUND_LOCATION: granted=true`) |
| 6. Notificaciones y batería | [05-bateria.png](05-bateria.png) | Diálogos del sistema; las tres filas quedan en «Concedido» |
| 7. «Continuar sin cuenta» | [06-mapa-con-barra.png](06-mapa-con-barra.png) | Mapa con barra inferior Mapa · Mis viajes · Ajustes |
| 8. Pestañas | [07-mis-viajes.png](07-mis-viajes.png), [08-ajustes.png](08-ajustes.png) | Cada una en 1 toque (FR-008) |
| 9. Atrás desde el mapa y volver a abrir | — | Atrás cierra la app (no vuelve a los términos); al abrir de nuevo, en frío o en caliente, va directo al mapa |

## Lo que se vio

| Diferencia con la spec y el prototipo | Estado |
|---|---|
| Tocar el texto de una casilla (términos) o de una opción (vehículo) no la marcaba | ✅ Corregida: filas pulsables enteras (`FilaMarcable`); captura 10 y 11 marcadas tocando el texto |
| Vehículo con nombres internos (LIVIANO…) y «ajusta la sensibilidad»; sin guía del soporte y del cargador | ✅ Corregida con los textos del prototipo «¿Qué manejas?» (RN-18, FR-004); captura 11 |
| Botón de términos «Continuar» en vez de «Acepto y continúo», sin aviso de versión | ✅ Corregida; captura 10 |
| Tema por defecto de Material (morado) | ✅ Tema de KillaUru (paleta del panel, Overpass y Source Sans 3), claro y oscuro; capturas 09 a 12 |
| El mapa sale vacío (estilo de demostración, sin centrar en la PE-28A) y los marcadores son todos rojos | Pendiente: paso de la CDN (T096) |
| «Crear cuenta» continúa sin cuenta | Pendiente: parte 2 de T024 |

La captura 12 (modo oscuro) muestra además que la app retoma en el vehículo cuando los términos ya estaban aceptados.
