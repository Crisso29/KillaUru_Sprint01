# Feature Specification: App Android KillaUru

**Feature Branch**: `008-app-android`

**Created**: 2026-10-22

**Status**: Draft

**Input**: User description: "App Android de KillaUru: detectar baches sin tocar el celular, guardarlos en SQLite/Room sin señal, sincronizarlos con el contrato del lote sin perder ni duplicar, mostrar el mapa offline según el rol, alertar a lo largo de la vía, gestionar la cuenta y el consentimiento, y publicar en R2 la polilínea, el mapa base y sus manifiestos (D16)."

---

## 0. Ficha de la feature

| Campo | Valor |
|---|---|
| Qué es | La **App KillaUru** (contenedor C2): un APK Kotlin, *offline-first*, que detecta con LiteRT, guarda en Room, sincroniza por lotes, muestra el mapa MapLibre sin señal y alerta a lo largo de la vía. Incluye los **scripts y el workflow** que publican en R2 los datos del corredor (D16) |
| Qué **no** es | No decide si un evento es bueno ni qué ve cada rol (lo decide el servidor: 004, 005, 006, RNF-27); no entrena el modelo (009); no es el panel (010); no es frontera de seguridad (constitución VI) |
| Componentes C3 | Sesión y primer uso · Sensores · Detector · Seudónimo · Buffer · Sincronizador · Mapa · Corredor · Proximidad y Alertas · Historial · Actualizador (`Nivel3-diagrama-de-componentes.md` §3) |
| Base local | Room `killauru.db`: `evento`, `anomalia`, `estado_copia`, `recorrido`, `contador_local` (`data-model.md`) |
| Consume | 003 (`identidad.openapi.yaml`), 005 (`ingesta.openapi.yaml`), 006 (`mapa.openapi.yaml`), convenciones de 001; `cdn.killauru.pe` (`contracts/cdn.openapi.yaml`); manifiesto del modelo de 009 |
| Publica | `cdn.killauru.pe/mapa/…` (PMTiles, polilínea con km, polígono demo, estilo y su manifiesto) con un workflow manual (D16) |
| Pantallas del prototipo | Las 24 de «App del conductor»: primer uso (6), en la ruta (7), mis cosas (3), cuenta (7) y «¿Qué manejas?» desde Ajustes |
| Prioridad | *Must*: HU-01, HU-02, HU-03, HU-04, HU-07, HU-08, HU-09, HU-26 y la parte app de HU-18, HU-18b, HU-21, HU-22, HU-23, HU-25, HU-27 · *Should*: HU-24 (no recortable), RF-33 (recortable, 3.º) · *Could*: HU-10 (recortable, 2.º) |

## 1. Fuentes en `referencia/docs`

| Tipo | IDs | Documento |
|---|---|---|
| Historias | HU-01, HU-02, HU-03, HU-04, HU-07, HU-08, HU-09, HU-10, HU-26 · parte app de HU-18, HU-18b, HU-21, HU-22, HU-23, HU-24, HU-25, HU-27 | `02-historias-de-usuario.md` |
| Actores | Anónimo y Conductor Registrado usan el mismo APK; el administrador usa el panel | `01-actores.md` §1, §4 |
| Requisitos funcionales | RF-01…RF-10, RF-25, RF-26, RF-27, RF-33…RF-38 · parte app de RF-11, RF-14, RF-21, RF-22, RF-29…RF-32 · contrato del lote y estados locales | `03-requisitos-funcionales.md` |
| Atributos de calidad | RNF-04, RNF-10, RNF-11, RNF-12, RNF-14, RNF-15, RNF-26, RNF-33, RNF-34, RNF-35 (lo verifica 009) | `04-atributos-de-calidad.md` |
| Reglas de negocio | RN-01…RN-04, RN-07, RN-08, RN-09, RN-11, RN-16, RN-18, RN-19, RN-20, RN-21, RN-22 | `09-reglas-de-negocio.md` |
| Decisiones | ADR-003, ADR-004, ADR-007, ADR-009, ADR-011 | `07-decisiones-arquitectonicas.md` |
| Escenarios | EQ-01, EQ-02 (lado app), EQ-03, EQ-08, EQ-11 (cliente anónimo) | `08-escenarios-y-atributos-de-calidad.md` |
| Restricciones | R-01, R-08, S-02, S-06, S-07, RS-06, RS-08 | `05-restricciones.md` |
| Diseño | Tres capas y principios de la app; componentes; stack | `enfoque-arquitectonico.md` §2, §9 · `componentes-arquitectonicos.md` §5 · `estilo-arquitectonico.md` (stack) |
| Diseño | Estructura, Room, alertas a lo largo de la vía, batería, sincronización | `diseño-interno-de-modulos.md` §7 · `Nivel4-diagrama-de-codigo.md` §7 |
| Decisiones del autor (2026-10-21) | D2 (un lote = un seudónimo), D3 (`loteId` del servidor), D5 (`id: null` + `indice`), D8 (correo verificado), D13 (contraseña 10, mensaje genérico, eliminar con contraseña, `kmVia`, «permanente»), D16 (R2), D17 | `specs/revision/decisiones.md` |
| Hallazgos de la revisión | BD-A1…BD-A9, BD-15, BD-19 (DBA) · §7 (QA) · PO-16, PO-17 (producto) · 005 R-002 y docs01 (413) · 006 R-007 | `specs/revision/` |

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Primer uso: términos, privacidad, vehículo y permisos (Priority: P1)

Como conductor, con o sin cuenta, quiero aceptar los términos y el aviso de privacidad, decir qué manejo y dar los permisos sabiendo para qué sirve cada uno, para usar la app con reglas claras (HU-27, HU-22, HU-26, RF-35).

**Why this priority**: Sin aceptación no hay mapa ni detección (HU-27); sin categoría no se sabe si detectar (RN-18).

**Independent Test**: Instalar el APK en un emulador limpio y recorrer el primer uso con Compose testing; negar cada permiso y comprobar el indicador.

**Acceptance Scenarios**:

1. **Given** el primer uso, **When** no marco las dos casillas (términos y aviso de privacidad), **Then** el botón «Acepto y continúo» queda inactivo y la app no muestra el mapa ni detecta (HU-27, RF-37, RF-29).
2. **Given** que acepto sin cuenta, **When** continúo, **Then** se guardan en el celular la versión y la fecha de cada documento (RF-29, RF-37).
3. **Given** que elijo Liviano, **When** continúo, **Then** veo la guía del soporte y del cargador y la detección queda habilitada; con Pesado o Moto, la detección y el envío quedan apagados y el indicador dice «Detección apagada (vehículo Pesado o Moto)» (HU-26, RN-18, RF-36).
4. **Given** que niego la ubicación, **When** abro el mapa, **Then** lo veo, no se detecta ni se alerta y el indicador dice «Falta un permiso» con el motivo (RF-35).
5. **Given** que cambió la versión vigente de los términos, **When** abro la app con señal, **Then** se me pide aceptarla otra vez antes de empezar una nueva detección o mostrar el mapa (HU-27; R-024).
6. **Given** «Solo ver el mapa, no enviar datos» activado en Ajustes, **When** manejo un Liviano, **Then** no se detecta ni se envía, la categoría no cambia y el indicador dice «Envío desactivado por el usuario» (RF-29).

---

### User Story 2 - Detectar baches sin tocar el celular (Priority: P1)

Como conductor de un vehículo liviano, quiero que la app detecte y clasifique los baches sola, para no distraerme (HU-01, HU-02, EQ-08).

**Why this priority**: Es la fuente de todos los datos del sistema.

**Independent Test**: Reproducir lecturas grabadas de sensores y GPS (reproductor de 5 h, EQ-01) contra el servicio y contar los eventos guardados.

**Acceptance Scenarios**:

1. **Given** velocidad GPS de 6 km/h durante 10 s seguidos y categoría Liviano, **When** pasa un bache, **Then** se guarda un evento `PENDING` sin ningún toque y el indicador muestra «Detectando» (HU-01, RN-03).
2. **Given** velocidad de 6 km/h durante solo 9 s, o de 5,0 km/h, **When** pasa un bache, **Then** los sensores siguen apagados y el indicador dice «En pausa: velocidad baja» (RN-03).
3. **Given** una ventana de 500 ms, **When** el modelo la clasifica, **Then** devuelve tipo, severidad 1–5 y confidence en < 50 ms p95 (HU-02, RNF-04).
4. **Given** confidence 0.74, **When** se clasifica Bache, **Then** no se guarda; con 0.75 sí; la clase `Normal` nunca se guarda (RN-04, RN-01).
5. **Given** un evento guardado, **When** se revisa, **Then** lleva `eventoId` UUIDv7, hora de detección del GPS (no del reloj del celular), coordenadas y precisión del fix, seudónimo vigente, tipo, severidad, confidence, `modeloVersion` y estado `PENDING` (RF-03).
6. **Given** batería al 14 % sin cargar, **When** manejo, **Then** se pausan la detección y el envío, las alertas siguen y el indicador dice «En pausa: batería baja»; al cargar o pasar el 15 % se reanuda sola (RN-22).
7. **Given** dos ventanas seguidas positivas por el mismo golpe, **When** se guardan, **Then** queda un solo evento (R-018).

---

### User Story 3 - Guardar sin señal y sincronizar sin perder ni duplicar (Priority: P1)

Como conductor, quiero que mis detecciones esperen en el celular mientras no hay señal y lleguen al servidor una sola vez, para que ningún dato se pierda (HU-03, HU-04, EQ-01, EQ-02).

**Why this priority**: Más del 50 % del corredor no tiene señal (ADR-003).

**Independent Test**: Room en memoria + MockWebServer con las respuestas del contrato del lote; WorkManager con `TestDriver`; modo avión de 5 h en campo.

**Acceptance Scenarios**:

1. **Given** 5 h sin conexión, **When** detecto, **Then** el 100 % de los eventos con confidence ≥ 0.75 queda `PENDING` y el mapa y las alertas siguen (HU-03, RNF-33).
2. **Given** eventos pendientes, **When** vuelve la señal, **Then** el envío empieza en ≤ 60 s y el *backoff* vuelve a 30 s (HU-04, RF-04).
3. **Given** 120 pendientes de un mismo seudónimo, **When** se sincroniza, **Then** salen en lotes de 50, 50 y 20, primero los más antiguos, cada uno marcado `ENVIANDO` en una transacción (RF-06; BD-A6).
4. **Given** pendientes de dos seudónimos (rotó a mitad), **When** se arma un lote, **Then** cada lote lleva un solo seudónimo (D2).
5. **Given** un 200 con aceptados, duplicados y rechazados, **When** llega, **Then** todos los ids nombrados pasan a `SYNCED`, el rechazado con `id: null` se marca `REJECTED_LOCAL` por su `indice` y los no mencionados vuelven a `PENDING` (contrato del lote; D5).
6. **Given** un 400, **When** llega, **Then** el lote se parte en mitades y se reintenta; un evento que falla **solo** queda `REJECTED_LOCAL` (RF-06).
7. **Given** un 401, **When** llega, **Then** la app renueva el token y reenvía **el mismo** lote; si la sesión ya no vale, borra el permiso local y lo reenvía sin token (contrato del lote; RN-21 c).
8. **Given** un 429 o 503 con `Retry-After: 120`, **When** llega, **Then** el siguiente intento ocurre entre 120 y 150 s después (RF-07).
9. **Given** un *timeout* o un 500, **When** se reintenta, **Then** las esperas siguen 30 s → 2 min → 10 min → 30 min → 2 h (tope), cada una con ±25 % (RF-07).
10. **Given** un evento con más de 168 h desde su detección, **When** toca enviarlo, **Then** no se envía, pasa a `EXPIRADO_LOCAL` y se cuenta (RF-06).
11. **Given** la respuesta 200 perdida por un corte, **When** se reenvía, **Then** el servidor los devuelve como `duplicados` y la app no genera nada nuevo (EQ-02).
12. **Given** la app cerrada con eventos `ENVIANDO`, **When** se abre, **Then** vuelven a `PENDING` antes de cualquier envío (RF-06).
13. **Given** una actualización del APK que cambia el esquema de Room, **When** migra, **Then** ningún evento `PENDING` se pierde (BD-A1).

---

### User Story 4 - Ver el mapa sin señal según mi rol (Priority: P1)

Como conductor, quiero un mapa del corredor que funcione sin internet y me diga cuándo se actualizó, para planificar mi viaje (HU-07, HU-08, RF-26).

**Why this priority**: *Must*; el mapa es lo que el anónimo ve antes de decidir registrarse.

**Independent Test**: MockWebServer con respuestas de `mapa.openapi.yaml` y del CDN; descarga cortada a la mitad; modo avión.

**Acceptance Scenarios**:

1. **Given** el primer uso con Wi-Fi, **When** descargo el mapa, **Then** se bajan el PMTiles, la polilínea con km y el polígono demo, se verifica cada SHA-256 contra el manifiesto y recién entonces se activan (RF-26, ADR-011).
2. **Given** una descarga cortada al 62 %, **When** vuelve la red, **Then** sigue desde donde quedó (Range) (RF-26).
3. **Given** datos móviles y la opción «Usar datos móviles» apagada, **When** falta el mapa, **Then** no se descarga y veo «Aún no tienes el mapa» con el botón para usar datos (RF-26).
4. **Given** un anónimo, **When** actualiza anomalías, **Then** pide `GET /api/v1/anomalies` **sin** `bbox` ni `updatedSince` (con `If-None-Match` si tiene `ETag`) y reemplaza toda su copia del demo (006 R-007).
5. **Given** un conductor registrado con copia, **When** actualiza, **Then** pide con `updatedSince` = el último `generadoUtc`, guarda las altas, borra las bajas (`eliminada: true`) y guarda el nuevo `generadoUtc` en la misma transacción (RF-14, RF-26).
6. **Given** un anónimo que sale del tramo demo, **When** cruza el límite, **Then** ve la zona sombreada y el aviso «Sin información de baches para usuarios sin cuenta. Crea una cuenta gratis para ver los 333 km.» una sola vez por viaje, sin tocar nada; la detección sigue (RF-08, RN-08).
7. **Given** una anomalía con `venceUtc` vencido según la hora del GPS y una copia que no se pudo actualizar, **When** miro el mapa, **Then** ya no aparece ni alerta (RF-26).
8. **Given** el mapa, **When** lo miro, **Then** veo «Actualizado hace 12 min», cada anomalía con su severidad, «permanente» si es persistente (sin número de días), «Último reporte hace 2 horas» y su km (RF-09, RNF-14, D13).

---

### User Story 5 - Alertas anticipadas a lo largo de la vía (Priority: P1)

Como conductor registrado, quiero que me avisen antes de cada bache, aunque no tenga señal ni mire la pantalla, para frenar a tiempo (HU-09, RF-10).

**Why this priority**: Es la protección del conductor; *Must*.

**Independent Test**: Trayectorias GPS simuladas sobre la polilínea (recta, curva cerrada, ida y vuelta) contra `ProximidadUseCase` con un reloj falso.

**Acceptance Scenarios**:

1. **Given** Liviano a 60 km/h hacia una anomalía `Activa` por delante, **When** faltan 8 s ± 1 s (≈ 133 m por la vía), **Then** suena una sola alerta sonora y visual (RN-16).
2. **Given** Liviano a 30 km/h, **When** se acerca, **Then** la alerta suena a 100 m (mínimo), no a 67 m (RN-16).
3. **Given** Pesado a 60 km/h, **When** se acerca, **Then** suena a 12 s (200 m); a 30 km/h, a 150 m (RN-16).
4. **Given** una curva donde el bache está a 80 m en línea recta y a 1 km por la pista, **When** voy a 60 km/h, **Then** no suena hasta faltar ≈ 133 m por la vía (RN-16).
5. **Given** una anomalía que ya dejé atrás, **When** sigo, **Then** no suena (solo por delante en el sentido de marcha); tampoco si estoy a más de 50 m de la vía (`diseño-interno` §7.3).
6. **Given** que ya me avisó de una anomalía, **When** sigo acercándome, **Then** no vuelve a sonar hasta alejarme más de 500 m y volver (RN-16).
7. **Given** tres anomalías dentro de la distancia de aviso, **When** se evalúa, **Then** suena una sola alerta que dice «3 baches seguidos» con el color de la peor (RN-16).
8. **Given** la pantalla bloqueada, música sonando y el celular en vibración, **When** suena la alerta, **Then** la música baja, la alerta suena con el volumen de alarma y vibra (RN-20).
9. **Given** un anónimo, **When** se acerca a un bache del demo, **Then** no recibe alerta (RN-08).
10. **Given** batería baja, Pesado o «Solo ver el mapa», **When** soy registrado, **Then** las alertas siguen (RN-22; `enfoque` §9 principio 5).

---

### User Story 6 - Mi cuenta desde la app (Priority: P1)

Como conductor, quiero crear mi cuenta, entrar, verificar mi correo, ver mis datos, cerrar sesión y eliminar mi cuenta desde la app, para controlar mis datos (HU-18, HU-18b, HU-22, HU-23, HU-24, HU-25, RN-21).

**Why this priority**: *Must* y obligación legal (Ley 29733); HU-24 es *Should* no recortable.

**Independent Test**: MockWebServer con las respuestas de `identidad.openapi.yaml`; Compose testing de cada pantalla.

**Acceptance Scenarios**:

1. **Given** una contraseña de 9 caracteres, **When** creo la cuenta, **Then** la app no la envía y explica «Mínimo 10 caracteres» (D13, RF-21).
2. **Given** el registro sin marcar la aceptación, **When** lo intento, **Then** no se crea la cuenta (HU-22).
3. **Given** un registro correcto (201), **When** termina, **Then** veo «Revisa tu correo» y puedo ir al mapa de 333 km enseguida (HU-18, HU-18b).
4. **Given** credenciales malas, una cuenta bloqueada o desactivada, **When** inicio sesión, **Then** veo siempre «Correo o contraseña incorrectos. Si fallas varias veces, espera unos minutos.», sin cuenta regresiva (RN-19, D13).
5. **Given** «Olvidé mi contraseña», **When** envío el correo, **Then** veo el mismo mensaje exista o no la cuenta (HU-25).
6. **Given** que cierro sesión con 3 detecciones pendientes, **When** confirmo tras el aviso, **Then** se borran el permiso local y el historial, la copia se recorta al tramo demo y los 3 pendientes se envían sin cuenta (RF-38, RN-21 d).
7. **Given** que elimino mi cuenta con la contraseña correcta, **When** responde 204, **Then** pasa lo mismo que al cerrar sesión; con contraseña incorrecta veo el error y no se borra nada (HU-23, RF-30, D13).
8. **Given** «Mi perfil», **When** lo abro, **Then** veo correo, tipo de cuenta, fecha de alta, versiones aceptadas con fecha y si el correo está verificado (HU-24, RF-31).
9. **Given** una sesión cuyo permiso local vence a mitad de un viaje, **When** sigo manejando, **Then** el corredor y las alertas siguen hasta detenerme más de 30 min (RN-21 b).
10. **Given** que el servidor responde `cuenta_desactivada` al renovar, **When** ocurre, **Then** el permiso local se borra al instante y la copia se recorta al tramo demo (RN-21 c, RNF-26).

---

### User Story 7 - Mi celular no me identifica (Priority: P1)

Como conductor, quiero que mis eventos viajen con un código aleatorio que cambia cada mes, para que nadie me reconstruya (HU-21, RF-27, ADR-007).

**Why this priority**: *Must* y legal.

**Independent Test**: Reloj falso en el dominio: rotación a los 30 días; inspección de los lotes en MockWebServer.

**Acceptance Scenarios**:

1. **Given** la instalación, **When** se crea el seudónimo, **Then** es un UUID aleatorio, no derivado del hardware ni de la cuenta (RF-27, ADR-007).
2. **Given** un seudónimo con 29 días, **When** se guarda un evento, **Then** lleva el mismo; con 30 días se genera otro (RN-09 `RotacionSeudonimoDias`).
3. **Given** una petición sin token, **When** sale, **Then** lleva la cabecera `X-KillaUru-Seudonimo`; con token, no (RF-27, 001 `Seudonimo`).

---

### User Story 8 - Publicar los datos del corredor en R2 (Priority: P1)

Como equipo, quiero generar desde el repositorio la polilínea con km, el polígono demo, el mapa base y su manifiesto, y subirlos a `cdn.killauru.pe` con un workflow, para que la app y el panel usen los mismos datos que el servidor (D16, RF-26, ADR-011).

**Why this priority**: Sin estos archivos no hay mapa offline ni alertas (PO-17, BD-19).

**Independent Test**: Correr los scripts sobre `backend/db/datos/` y comparar los km con los puntos de referencia del servidor; correr el workflow en modo de prueba.

**Acceptance Scenarios**:

1. **Given** `backend/db/datos/corredor_pe28a.geojson`, **When** corro el script, **Then** obtengo `corredor_pe28a.v<N>.json` con `[lon, lat, kmAcumulado]` por vértice, km 0 en Pisco, calculado con la misma fórmula que `CorredorVia` de 004 (R-016).
2. **Given** los puntos de referencia, **When** comparo el km de la app con el `km_via` del servidor, **Then** la diferencia es ≤ 1 m (D13; SC-012).
3. **Given** `tramo_demo.geojson`, **When** se verifica, **Then** todos los vértices de la polilínea entre el km 268 y el 333 caen dentro del polígono (BD-19).
4. **Given** el workflow manual, **When** se ejecuta, **Then** sube primero los archivos versionados (inmutables) y al final el manifiesto con versión, tamaño y SHA-256 de cada uno (D16, RF-26).

---

### User Story 9 - Recibir un modelo mejorado sin reinstalar (Priority: P3, *Should*, recortable)

Como conductor, quiero que la app use el modelo más nuevo sin instalar otro APK, para detectar mejor (RF-33, ADR-004).

**Why this priority**: *Should*; 3.º en el orden de recorte. La v1.0 puede salir con el modelo del APK.

**Independent Test**: MockWebServer con manifiestos válidos, con huella incorrecta y con versión mínima mayor.

**Acceptance Scenarios**:

1. **Given** Wi-Fi y un manifiesto con una versión nueva, **When** corre el actualizador, **Then** descarga el modelo, verifica su SHA-256 y lo deja listo; se activa cuando la detección está detenida, nunca a mitad de un viaje (RF-33).
2. **Given** una huella que no coincide, **When** se verifica, **Then** se descarta y sigue el modelo anterior (RF-33).
3. **Given** `versionMinimaApp` mayor que la del APK, o etiquetas distintas del catálogo, **When** se lee el manifiesto, **Then** no se descarga (RF-33, ADR-004 punto 4).

---

### User Story 10 - Mis viajes, solo en mi celular (Priority: P3, *Could*, recortable)

Como conductor registrado, quiero ver los tramos que recorrí y cuántos baches detecté en 6 meses, guardados solo en mi celular (HU-10, RF-25).

**Why this priority**: *Could*; 2.º en el orden de recorte.

**Independent Test**: Viajes simulados con reloj falso; purga a los 6 meses; borrado al cerrar sesión.

**Acceptance Scenarios**:

1. **Given** un viaje del km 333 al 268 con 7 detecciones, **When** termina (parada > 30 min), **Then** aparece «Ayacucho → Vinchos · km 333 → 268 · 1 h 20 min · 7» (HU-10).
2. **Given** un recorrido de hace 6 meses y 1 día, **When** corre la purga, **Then** se borra (RN-10).
3. **Given** un anónimo, **When** abre «Mis viajes», **Then** ve que el historial es para cuentas; no se guarda nada (`01-actores.md` §4).

### Edge Cases

- Primer uso **sin señal**: los textos legales vienen en el APK (versión incluida); la detección funciona; el mapa muestra «Aún no tienes el mapa» (R-024, RF-26).
- Reloj del celular adelantado 2 días: las reglas usan la hora del GPS; la purga y la expiración no se adelantan (R-003).
- Reinicio del celular sin GPS ni red: no hay hora confiable; la app **no** marca `EXPIRADO_LOCAL` ni purga hasta tenerla (R-003).
- Lote donde el evento de la posición 7 no tiene `eventoId` válido (no debería ocurrir): `{id: null, indice: 7}` → ese evento `REJECTED_LOCAL` (D5).
- Un 200 que menciona un id que no estaba en el lote: se ignora.
- 413 (cuerpo > 256 KB, no esperado con 50 eventos): se trata como 400 (R-026).
- 403 `sin_permiso` en el lote (token de un rol que no sincroniza): se descarta la sesión local y se reenvía sin token (R-008, R-033).
- Rotación del seudónimo con pendientes: los viejos salen en sus propios lotes (D2).
- Cerrar sesión **sin señal**: todo lo local se hace al instante; la revocación en el servidor queda pendiente y se envía con la siguiente señal (R-023).
- Descarga del mapa con huella incorrecta: se borra el parcial y se reintenta; nunca se activa (RF-26).
- Manifiesto del mapa nuevo mientras se maneja: el archivo nuevo se activa al terminar el viaje (R-014).
- La polilínea nueva cambia los km: la copia de anomalías se vuelve a bajar completa (R-016).
- Anomalía con `venceUtc` nulo en una respuesta del conductor (no debería): se trata como baja (006 contrato: nulo solo en estados finales).
- Velocidad GPS ausente (`hasSpeed() == false`): se calcula con dos fixes; si no se puede, no hay alerta ni activación de sensores.
- Vehículo a más de 50 m de la vía: no hay alerta (fuera del corredor).
- Android mata el servicio: `START_STICKY`; los eventos ya guardados no se pierden; el indicador lo muestra al volver.
- Usuario administrador que inicia sesión en la app: se rechaza con «Esta cuenta es de administración; usa el panel» (`01-actores.md` §1; R-033).
- Cambio de categoría a mitad de un viaje: aplica desde el siguiente fix; los pendientes de Liviano se siguen enviando.

## Requirements *(mandatory)*

### Functional Requirements

**A. Primer uso, consentimiento y ajustes (HU-27, HU-22, HU-26, RF-29, RF-34, RF-35, RF-37)**

- **FR-001**: La app MUST mostrar en el primer uso, con o sin cuenta, los términos de uso y el aviso de privacidad en una sola pantalla, con una casilla para cada uno; el botón solo se activa con las dos marcadas, y sin aceptar no se muestra el mapa ni se detecta. *(HU-27, RF-37, HU-22, RF-29)*
- **FR-002**: La app MUST guardar la versión y la fecha aceptadas de cada documento: en DataStore para el anónimo; para el registrado, se envían en `POST /api/v1/auth/registro` o en `POST /api/v1/me/aceptaciones`. *(RF-29, RF-37; 003 contrato)*
- **FR-003**: La app MUST traer en el APK los textos y la versión vigente al compilar, consultar `GET /api/v1/legal/versiones` al abrirse con red y, si la vigente difiere de la aceptada, pedir aceptarla antes de iniciar un nuevo viaje con detección o mostrar el mapa. *(RF-37, HU-27; R-024)*
- **FR-004**: La app MUST pedir en el primer uso la categoría (Liviano, Pesado o Moto) con la guía del soporte y del cargador, guardarla solo en el celular y permitir cambiarla en Ajustes. *(HU-26, RF-34, RN-18, S-07)*
- **FR-005**: La app MUST ofrecer en Ajustes «Solo ver el mapa, no enviar datos», que apaga la detección y el envío sin cambiar la categoría. *(RF-29, RN-18; R-025)*
- **FR-006**: La app MUST pedir los permisos explicando cada uno: ubicación precisa y «todo el tiempo» (Android 10+), notificaciones (Android 13+) y exclusión del ahorro de batería; si se niega uno, sigue con lo que pueda y explica qué se pierde. *(RF-35)*
- **FR-007**: La pantalla Ajustes MUST mostrar: vehículo, «Solo ver el mapa», versión del modelo, mapa (tamaño, «actualizado hace…», actualizar, usar datos móviles), Mi perfil, términos y privacidad (versión y fecha) y el enlace a la página de estado de UptimeRobot. *(RF-29, RF-34, RF-26, RF-33, HU-17; prototipo)*
- **FR-008**: La app MUST estar completamente en español, detectar con 0 toques y permitir cada acción principal en ≤ 3 toques. *(RNF-12, EQ-08, HU-01)*

**B. Detección (HU-01, HU-02, RF-01…RF-03, RF-36)**

- **FR-009**: La app MUST correr la detección y las alertas en un servicio en primer plano de tipo `location` con notificación permanente, leyendo el GPS a 1 Hz con `LocationManagerCompat` (sin Google Play Services). *(RF-01, RS-06, RF-36; `diseño-interno` §7.4)*
- **FR-010**: La app MUST encender acelerómetro y giroscopio a 50 Hz, en lote (`maxReportLatencyUs` ≈ 1 s), solo si `PoliticaDeteccion` lo permite: velocidad GPS > 5 km/h sostenida ≥ 10 s, categoría en `CategoriasQueDetectan`, sin «Solo ver el mapa», sin ahorro de batería y con permiso de ubicación; en otro caso los apaga. *(RF-01, RN-03, RN-18, RF-29, RN-22)*
- **FR-011**: La app MUST armar ventanas de 500 ms según el manifiesto del modelo, pasar al modelo solo las que superan el prefiltro de energía y clasificarlas con LiteRT reutilizando el intérprete y *buffers* preasignados, devolviendo tipo, severidad 1–5 y confidence. *(RF-02, ADR-004, RNF-04; `diseño-interno` §7.4)*
- **FR-012**: La app MUST guardar un evento solo si el tipo es Bache o Hundimiento y la confidence ≥ `ConfidenceMinimo` (0.75); `Normal` nunca se guarda. *(RN-04, RN-01, RF-03)*
- **FR-013**: Cada evento MUST guardarse con `eventoId` UUIDv7, `detectadoUtc` = hora del fix GPS más el tiempo monotónico transcurrido hasta el fin de la ventana, latitud, longitud y precisión del fix más cercano (≤ `AntiguedadMaximaFixMs`), seudónimo vigente, tipo, severidad, confidence, `modeloVersion` y estado `PENDING`; sin fix reciente no se guarda. *(RF-03; R-003, R-004, R-018)*
- **FR-014**: Ventanas positivas consecutivas separadas ≤ `FusionVentanasMs` MUST fundirse en un solo evento (el de mayor confidence). *(Decisión SDD, R-018)*
- **FR-015**: Cada evento MUST llevar la `modeloVersion` del modelo activo al detectar; el APK MUST incluir un modelo inicial con su manifiesto. *(ADR-004, RF-02)*
- **FR-016**: La app MUST mostrar el indicador de estado en la notificación del servicio y en la pantalla principal con los 7 estados de RF-36 (prioridad de R-032) y el número de eventos pendientes. *(RF-36)*
- **FR-017**: Con batería < `BateriaMinimaPorcentaje` (15 %) y sin cargar, la app MUST pausar la detección y el envío, mantener las alertas y reanudar sola al cargar o superar el umbral. *(RN-22)*
- **FR-018**: La app MUST definir un **viaje** que empieza al cumplirse RN-03 y termina tras `ParadaFinViajeMinutos` (30) detenido o al detener el servicio; lo usan el historial, el aviso fuera del demo y el permiso local. *(RN-21 b, RF-08, RF-25; R-019)*

**C. Base local en Room (RF-03, RF-06, ADR-003)**

- **FR-019**: La app MUST guardar sus datos en la base Room `killauru.db` (modo WAL) con las tablas, columnas, índices, códigos enteros estables y disparadores de validación de `data-model.md` §1–§2. *(ADR-003, `diseño-interno` §7.2; BD-A5, BD-A9)*
- **FR-020**: Un evento MUST cambiar de estado solo por las transiciones de `data-model.md` §3 (`PENDING`, `ENVIANDO`, `SYNCED`, `REJECTED_LOCAL`, `EXPIRADO_LOCAL`); cualquier otra la rechazan el dominio y el disparador `trg_evento_transicion`. *(RF-06, ADR-003)*
- **FR-021**: Al abrir la app, antes de programar cualquier envío, todo evento `ENVIANDO` MUST volver a `PENDING`. *(RF-06)*
- **FR-022**: Un evento `PENDING` con más de `AntiguedadMaximaEnvioHoras` (168) desde su detección, medidas con el reloj confiable, MUST pasar a `EXPIRADO_LOCAL` sin enviarse y sumarse al contador `expirado_local`. *(RF-06, RN-11)*
- **FR-023**: Una tarea diaria MUST borrar los eventos `SYNCED`, `REJECTED_LOCAL` y `EXPIRADO_LOCAL` con `estadoDesde` de más de 7 días, los recorridos de más de 6 meses y las anomalías con `venceUtc` de más de 7 días; nunca borra `PENDING` ni `ENVIANDO`. *(RF-06, RF-25, RN-10; BD-A4)*
- **FR-024**: Cada cambio del esquema MUST subir la versión de Room con una `Migration` explícita y probada que conserve `evento` y `recorrido`; `fallbackToDestructiveMigration` está prohibido y el esquema JSON de cada versión se guarda en git. *(BD-A1, RNF-06, RNF-33; R-006)*
- **FR-025**: La app MUST conservar sin pérdida ≥ 7 días de pendientes y ocupar < 100 MB tras 6 meses (sin el mapa base, hasta 80 MB aparte). *(RNF-33, RNF-15)*
- **FR-026**: Toda regla que dependa de la hora («ahora» para 168 h, `venceUtc`, rotación, permiso local, purgas) MUST usar el **reloj confiable** (hora del GPS o del servidor anclada al reloj monotónico); el reloj de pared del celular nunca se usa para reglas. *(RF-03; BD-A3; R-003)*
- **FR-027**: Los datos de la app MUST quedar fuera de la copia de seguridad de Android (`allowBackup = false` y reglas de extracción vacías). *(RNF-10, RN-21; R-027)*

**D. Sincronización (RF-04, RF-06, RF-07, contrato del lote)**

- **FR-028**: La app MUST sincronizar sola con un trabajo único de WorkManager (`sync`) con red conectada; al recuperar la red, un `NetworkCallback` reinicia el *backoff* y lanza el envío en ≤ 60 s. *(RF-04, RNF-33, HU-04; `diseño-interno` §7.5)*
- **FR-029**: Cada lote MUST tomar, en **una** transacción, hasta `MaxEventosPorLote` (50) eventos `PENDING` del seudónimo del pendiente más antiguo, los más antiguos primero, y marcarlos `ENVIANDO`. *(RF-06, D2; BD-A6)*
- **FR-030**: El cuerpo MUST ser `{ "eventos": [...] }` sin `loteId`; con sesión, va `Authorization: Bearer`; sin sesión, la cabecera `X-KillaUru-Seudonimo` con el seudónimo del lote. *(RF-11, RF-27, D2, D3; 005 contrato)*
- **FR-031**: Ante 200, la app MUST marcar `SYNCED` todo id de `aceptados`, `duplicados` y `rechazados`, marcar `REJECTED_LOCAL` el evento en la posición `indice` de cada rechazado con `id: null`, devolver a `PENDING` los no mencionados y sumar cada rechazo a `contador_local` por motivo. *(RF-06, contrato del lote; D5)*
- **FR-032**: Ante 400, la app MUST partir el lote en mitades y reintentar cada parte; un evento que falla **solo** pasa a `REJECTED_LOCAL`. *(RF-06, ADR-003)*
- **FR-033**: Ante 413, la app MUST actuar como ante 400. *(Decisión SDD, R-026; 005 R-002)*
- **FR-034**: Ante 401, la app MUST renovar el token y reenviar el **mismo** lote; si la renovación responde `sesion_invalida` o `cuenta_desactivada`, o el lote responde 403, MUST aplicar RN-21 c y reenviar el lote sin token. Nunca marca rechazados por esto. *(Contrato del lote, RN-21 c; R-008)*
- **FR-035**: Ante 429 o 503, la app MUST devolver el lote a `PENDING` y reintentar tras `Retry-After` más un *jitter* de 0 a +25 %. *(RF-07)*
- **FR-036**: Ante 5xx, *timeout* o error de red, la app MUST devolver el lote a `PENDING` y reintentar con 30 s → 2 min → 10 min → 30 min → 2 h (tope), cada espera con ±25 %, programada con `setInitialDelay`. *(RF-07, ADR-003)*
- **FR-037**: El envío MUST detenerse con «Solo ver el mapa» o con el ahorro de RN-22 y reanudarse solo al levantarse. *(RF-29, RN-22; R-025)*
- **FR-038**: Solo MUST correr un envío a la vez; si Android detiene el trabajo, sus eventos `ENVIANDO` vuelven a `PENDING`. *(RF-06)*

**E. Seudónimo (RF-27, RN-09, ADR-007)**

- **FR-039**: El seudónimo MUST ser un UUID aleatorio (no derivado del hardware ni de la cuenta) que rota cada `RotacionSeudonimoDias` (30) según el reloj confiable; cada evento guarda el vigente al detectarse. *(RF-27, RN-09, ADR-007; R-010)*
- **FR-040**: Las peticiones **sin** token (lote y mapa) MUST llevar `X-KillaUru-Seudonimo`; las peticiones con token no la llevan. *(RF-27, RNF-11; 001 contrato)*

**F. Mapa offline y anomalías locales (RF-08, RF-09, RF-14, RF-26, ADR-011)**

- **FR-041**: La app MUST descargar el mapa base PMTiles según `mapa/manifiesto.json`, de forma reanudable (Range), solo con Wi-Fi salvo que el usuario acepte datos móviles, verificar tamaño y SHA-256 antes de activarlo y conservar la versión anterior hasta entonces. *(RF-26, ADR-011)*
- **FR-042**: Con el mismo manifiesto, la app MUST descargar y verificar la polilínea con km y el polígono demo, y cargarlos en `Corredor` y `ZonaDemo`. *(RF-26, RN-16, RN-08, D16)*
- **FR-043**: El mapa MUST dibujarse con MapLibre Native leyendo el PMTiles local con el estilo del APK, mostrar la atribución «© OpenStreetMap» y dibujar las anomalías en una capa aparte por tipo y severidad. *(ADR-011)*
- **FR-044**: El conductor registrado MUST actualizar con `GET /api/v1/anomalies?updatedSince=<generadoUtc>` (sin `bbox` ni filtros; sin `updatedSince` la primera vez), guardar altas, borrar bajas y guardar el nuevo `generadoUtc` en la misma transacción. *(RF-14, RF-26; 006 R-006)*
- **FR-045**: El anónimo MUST pedir siempre `GET /api/v1/anomalies` sin `bbox` ni `updatedSince`, con `If-None-Match` si tiene `ETag`; ante 200 reemplaza toda la copia y ante 304 la conserva. *(006 R-007, RNF-01, EQ-11)*
- **FR-046**: La copia MUST actualizarse al abrir la app, al recuperar la red y cada `IntervaloActualizacionAnomaliasMin` (15) con red. *(RF-26; R-034)*
- **FR-047**: El mapa MUST mostrar «Actualizado hace …» con la hora del servidor de la última respuesta correcta y los tiempos relativos de RNF-14. *(RF-26, RNF-14; R-031)*
- **FR-048**: El mapa y las alertas MUST usar solo anomalías con `venceUtc` > ahora confiable. *(RF-26, RN-07)*
- **FR-049**: Para el anónimo, la app MUST sombrear en gris lo que queda fuera del polígono demo con el mensaje de RN-08 y mostrar el aviso una sola vez por viaje la primera vez que el vehículo está fuera, sin pedir toques; la detección y el envío siguen. *(RF-08, RN-08, HU-07; R-035)*
- **FR-050**: Para el registrado, el mapa MUST mostrar las anomalías del corredor con severidad, tiempo relativo, «permanente» si es persistente (sin número de días) y su `kmVia`; el detalle muestra además la distancia por la vía y el último reporte. *(RF-09, HU-08, D13)*
- **FR-051**: Al cerrar sesión, al eliminar la cuenta o al saberse la cuenta desactivada o eliminada, la app MUST borrar de la copia las anomalías con `enDemo = 0`. *(RN-21 c, d; BD-A2; R-012)*
- **FR-052**: Al iniciar sesión, la app MUST descartar la copia del demo y bajar la del corredor completo sin `updatedSince`. *(RN-08; R-011)*
- **FR-053**: Sin mapa descargado, la app MUST mostrar una pantalla que lo explica, con «Descargar ahora» y «Usar datos móviles»; la detección sigue. *(RF-26)*

**G. Alertas (RF-10, RN-16, RN-20)**

- **FR-054**: La app MUST alertar solo con permiso local vigente (conductor registrado, cualquier categoría), y nunca al anónimo. *(RN-08, RF-10, RN-21)*
- **FR-055**: Con cada fix, la app MUST proyectarlo sobre la polilínea empezando por el último segmento usado, obtener km y sentido de marcha y no alertar si el vehículo está a más de `DistanciaMaximaViaMetros` (50) de la vía. *(RN-16; `diseño-interno` §7.3)*
- **FR-056**: La distancia de aviso MUST ser `max(mínimo, velocidad × tiempo)` medida a lo largo de la vía, por delante en el sentido de marcha: Liviano y Moto 8 s / 100 m; Pesado 12 s / 150 m. *(RN-16, RN-18)*
- **FR-057**: Cada anomalía MUST alertar una vez por pasada (la pasada termina a más de 500 m); varias dentro de la misma distancia suenan como una sola alerta que dice cuántas son, con el color de la peor. *(RN-16)*
- **FR-058**: La alerta MUST ser sonora (frase grabada en español y tono), bajar la música mientras suena, sonar con la pantalla bloqueada y en vibración con el volumen de alarma, vibrar y mostrarse como complemento visual que se cierra sola. *(RN-20; R-021)*
- **FR-059**: Las alertas MUST funcionar sin internet y seguir con el ahorro de RN-22, con Pesado o Moto y con «Solo ver el mapa». *(RF-05, RF-10, RN-22; `enfoque` §9 principio 5)*

**H. Cuenta y sesión (RF-21, RF-22, RF-29…RF-32, RF-37, RF-38, RN-19, RN-21)**

- **FR-060**: El registro MUST pedir correo, contraseña de mínimo 10 caracteres y la aceptación de la política y los términos, y llamar a `POST /api/v1/auth/registro`; 201 guarda la sesión, 409 `correo_en_uso` sugiere iniciar sesión o recuperar la contraseña, 429 pide esperar. *(RF-21, HU-22, D13, D8)*
- **FR-061**: El inicio de sesión MUST usar `canal = app` y, ante 401 por cualquier causa, mostrar siempre el mismo mensaje genérico de RN-19, sin cuenta regresiva. *(RF-21, RN-19, D13)*
- **FR-062**: Los tokens MUST guardarse en DataStore cifrado con una clave AES-GCM de Android Keystore; el token de acceso se renueva solo (una renovación a la vez), y la app acepta solo sesiones de rol `ConductorRegistrado`. *(ADR-009, RF-21, `01-actores.md` §1; R-022, R-033)*
- **FR-063**: El permiso local MUST valer hasta `sesionVenceUtc`, renovarse con cada renovación correcta, no cortarse a mitad de un viaje (sigue hasta una parada > 30 min) y borrarse al cerrar sesión, al eliminar la cuenta o ante `sesion_invalida` o `cuenta_desactivada`. *(RN-21, RNF-26, RF-38)*
- **FR-064**: Tras el registro, la app MUST mostrar «Revisa tu correo» sin bloquear el mapa, con «Reenviar el correo» (`POST /api/v1/me/reenviar-verificacion`), y mostrar si el correo está verificado según `GET /api/v1/me`. *(HU-18b, RF-22)*
- **FR-065**: «Olvidé mi contraseña» MUST llamar a `POST /api/v1/auth/recuperar` y mostrar el mismo mensaje exista o no la cuenta. *(HU-25, RF-32)*
- **FR-066**: Cerrar sesión MUST mostrar antes lo que pasará y luego: llamar a `POST /api/v1/auth/cerrar-sesion` (sin red, la revocación queda pendiente), borrar tokens y permiso local, recortar la copia al demo, borrar el historial y dejar los pendientes para enviarse sin cuenta. *(RF-38, RN-21 d; R-023)*
- **FR-067**: Eliminar la cuenta MUST pedir la contraseña y una casilla de confirmación, llamar a `DELETE /api/v1/me` y, ante 204, hacer la misma limpieza local que al cerrar sesión; ante 403 `confirmacion_invalida`, no borrar nada; sin red, no se ofrece. *(HU-23, RF-30, D13)*
- **FR-068**: «Mi perfil» MUST mostrar todo lo que devuelve `GET /api/v1/me` y explicar que los viajes no están unidos a la cuenta. *(HU-24, RF-31)*

**I. Historial (HU-10, *Could*)**

- **FR-069**: Mientras haya sesión, la app MUST guardar al terminar cada viaje un resumen (inicio, fin, km desde y hasta, km recorridos, detecciones) solo en el celular, mostrar 6 meses con totales y borrarlo al cerrar sesión o eliminar la cuenta. *(HU-10, RF-25, RN-21)*

**J. Modelo (RF-02, RF-33, ADR-004)**

- **FR-070**: El actualizador MUST, solo con Wi-Fi, leer `modelo/manifiesto.json`, descargar una versión mayor si `versionMinimaApp` ≤ la del APK y las etiquetas coinciden con el catálogo, verificar su SHA-256, activarla solo con la detección detenida y conservar la anterior si algo falla. *(RF-33, ADR-004; *Should*, recortable)*
- **FR-071**: La app MUST leer del manifiesto del modelo (contrato compartido con 009) la forma de la entrada y de la salida, y rechazar un modelo cuyo catálogo no sea `Normal`, `Bache`, `Hundimiento`. *(ADR-004 punto 4, RN-01; R-017)*

**K. Datos del corredor en R2 (D16, RF-26, ADR-011)**

- **FR-072**: Scripts del repositorio MUST generar desde `backend/db/datos/corredor_pe28a.geojson` y `backend/db/datos/tramo_demo.geojson` la polilínea con km acumulado, el polígono demo verificado, el PMTiles del corredor (≤ 80 MB) y el manifiesto con versión, tamaño y SHA-256 de cada archivo y la atribución de OpenStreetMap. *(D16, RF-26, ADR-011, RNF-15; BD-19)*
- **FR-073**: Un workflow manual de GitHub Actions MUST subir a R2 los archivos versionados y, al final, el manifiesto, con las credenciales de R2 como secretos de GitHub. *(D16, RNF-34)*
- **FR-074**: El km de la app MUST coincidir con el `km_via` del servidor (≤ 1 m) en los puntos de referencia compartidos. *(D13, BD-19; R-016)*

**L. Arquitectura, calidad y CI**

- **FR-075**: El código MUST seguir las tres capas de `enfoque` §9 en un solo módulo Gradle por funcionalidad (`dominio`, `datos`, `ui`), verificado con Konsist en cada build. *(`enfoque-arquitectonico.md` §9, `diseño-interno` §7.1; R-001, R-002)*
- **FR-076**: Ningún archivo `.kt`, `.kts`, `.py` ni `.sh` de `app/` MUST pasar de 200 líneas (excepto generados y esquemas de Room), y las llamadas directas al reloj del sistema MUST estar prohibidas fuera de `RelojSistema`. *(Constitución VIII; propuesta de enmienda en `research.md`)*
- **FR-077**: Un workflow de CI MUST correr en cada PR que toque `app/`: build con advertencias como errores, lint, detekt, Konsist, unitarias, Robolectric, cobertura Kover ≥ 90 % de los paquetes `dominio` y el límite de 200 líneas. *(Constitución VII; propuesta de enmienda)*
- **FR-078**: Las pruebas de contrato MUST validar cada petición y cada respuesta simulada contra `ingesta.openapi.yaml`, `mapa.openapi.yaml`, `identidad.openapi.yaml` y `contracts/cdn.openapi.yaml`. *(Constitución IX; 001 convenciones)*

### Reglas y parámetros (`ParametrosApp`, archivo `assets/parametros.json`)

| Regla | Parámetro | Valor | Fuente |
|---|---|---|---|
| RN-03 | `VelocidadMinimaKmh` / `DuracionSostenidaSegundos` | 5 / 10 | `09` |
| RN-04 | `ConfidenceMinimo` | 0.75 | `09` |
| RN-18 | `CategoriasQueDetectan` | `[Liviano]` | `09` |
| RN-16 | `TiempoAvisoSegundos` / `DistanciaAvisoMinimaMetros` | 8 / 100 | `09` |
| RN-16 | `TiempoAvisoPesadoSegundos` / `DistanciaAvisoMinimaPesadoMetros` | 12 / 150 | `09` |
| RN-16 | `DistanciaFinPasadaMetros` | 500 | `09` |
| RN-16 | `DistanciaMaximaViaMetros` (sin alerta fuera de la vía) | 50 | `diseño-interno` §7.3 |
| RN-22 | `BateriaMinimaPorcentaje` | 15 | `09` |
| RN-21 | `DuracionPermisoLocalDias` / `ParadaFinViajeMinutos` | 30 / 30 | `09` (= 003 `SesionConductorDias`) |
| RN-09 | `RotacionSeudonimoDias` | 30 | `09` |
| RF-06 | `AntiguedadMaximaEnvioHoras` / `PurgaEstadosFinalesDias` | 168 / 7 | `03` |
| RF-06 | `MaxEventosPorLote` | 50 | `03` (= 005) |
| RF-07 | `BackoffSegundos` / `JitterPorcentaje` | [30, 120, 600, 1800, 7200] / 25 | `03` |
| RNF-33 | `ReanudarSincronizacionSegundosMax` | 60 | `04` |
| RF-25 | `RetencionHistorialMeses` | 6 | `09` RN-10 |
| BD-A4 | `PurgaAnomaliasVencidasDias` | 7 | SDD (R-013) |
| RF-01 | `FrecuenciaSensoresHz` / `LatenciaLoteSensoresMs` | 50 / 1000 | `03`, `diseño-interno` §7.4 |
| RF-03 | `AntiguedadMaximaFixMs` | 2000 | SDD (R-018) |
| — | `FusionVentanasMs` | 1000 | SDD (R-018) |
| RF-26 | `IntervaloActualizacionAnomaliasMin` | 15 | SDD (R-034) |
| RF-21 | `ContrasenaMinima` | 10 | `03` (D13) |

La ventana (500 ms), la frecuencia del modelo y la normalización vienen del **manifiesto del modelo** (R-017), no de `parametros.json`.

### Key Entities

- **Evento** (`evento`): detección con `eventoId`, hora GPS, posición, tipo, severidad, confidence, `modeloVersion`, seudónimo y estado local.
- **Anomalía local** (`anomalia`): copia de una anomalía visible para el rol, con `km` (`kmVia`), `venceUtc`, `ultimaDeteccionUtc` y `enDemo`.
- **Estado de la copia** (`estado_copia`): alcance (anónimo o registrado), `generadoUtc`, `ETag` y última verificación.
- **Recorrido** (`recorrido`): resumen de un viaje (solo registrado, 6 meses).
- **Contador local** (`contador_local`): rechazos por motivo y expirados.
- **Sesión** (DataStore cifrado): tokens, rol, `sesionVenceUtc` y permiso local.
- **Preferencias** (DataStore): categoría, «Solo ver el mapa», datos móviles, versiones aceptadas, seudónimo, ancla de tiempo, paso del *backoff*, versiones de mapa y modelo.
- **Manifiesto** (R2): versión y archivos con tamaño y SHA-256 (mapa y modelo).
- **Viaje** (dominio, en memoria): inicio por RN-03, fin por parada de 30 min.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: En la prueba de campo de 5 h en modo avión (EQ-01), el 100 % de las ventanas con confidence ≥ 0.75 queda guardado y la sincronización empieza en ≤ 60 s al volver la señal (RNF-33).
- **SC-002**: Con el reloj del emulador adelantado 7 días, 0 eventos `PENDING` perdidos; a las 168 h pasan a `EXPIRADO_LOCAL` (RNF-33, RF-06).
- **SC-003**: En la prueba de corte de red a mitad del envío (incluida la pérdida del 200), la cuenta de eventos enviados por la app coincide con `evento_recibido` y hay 0 duplicados (EQ-02, RNF-06).
- **SC-004**: La inferencia por ventana es < 50 ms p95 en 3 celulares de gama media (RNF-04, EQ-03).
- **SC-005**: Con la pantalla apagada, ≤ 5 % de batería por hora en 3 celulares; < 1 MB de datos por hora; < 100 MB de almacenamiento a 6 meses sin el mapa base (RNF-15).
- **SC-006**: En las trayectorias simuladas, la alerta suena a 8 s ± 1 s (Liviano) y 12 s ± 1 s (Pesado), respetando los mínimos, una vez por pasada (HU-09, RN-16).
- **SC-007**: 0 toques para detectar; cada acción principal en ≤ 3 toques; 100 % de los textos en español (RNF-12, EQ-08).
- **SC-008**: El 100 % de las migraciones probadas conserva los eventos `PENDING` y los recorridos (BD-A1).
- **SC-009**: El 100 % de las peticiones del mapa del anónimo van sin `bbox` ni `updatedSince` (006 R-007; condición del 80 % de aciertos de RNF-01).
- **SC-010**: Cobertura ≥ 90 % de los paquetes `dominio`; 0 archivos de `app/` de más de 199 líneas; 0 dependencias prohibidas entre capas.
- **SC-011**: Una descarga cortada se reanuda, y un archivo con huella incorrecta nunca se activa (RF-26).
- **SC-012**: El km de la app difiere ≤ 1 m del `km_via` del servidor en el 100 % de los puntos de referencia (D13, BD-19).

## Fuera de alcance

- Reglas de calidad, confirmación, ciclo de vida y recorte por rol: servidor (004, 005, 006).
- Entrenamiento, dataset, matriz de confusión y publicación del modelo y su manifiesto en R2: 009 (HU-28, RNF-35). 008 solo los consume.
- Páginas web de verificación de correo y restablecimiento de contraseña (`panel.killauru.pe/cuenta/…`): 003 y 010.
- Panel web, su mapa y su estilo en línea: 010 (lee el mismo PMTiles y estilo que publica 008).
- Publicación en Play Store e *Integrity API* (S-02).

## Assumptions

- El APK se instala fuera de Play Store (S-02); se actualiza instalando un APK nuevo con `versionCode` mayor.
- 004 T002 y 006 T002 producen `backend/db/datos/corredor_pe28a.geojson` y `backend/db/datos/tramo_demo.geojson` antes de US8 (D16).
- 009 publica `modelo/manifiesto.json` con el contrato de `contracts/contratos-internos.md` §3; mientras no exista, la app usa el modelo del APK.
- Celulares de gama media (≤ 4 GB de RAM) con GPS, acelerómetro y giroscopio; Android 8.0 (`minSdk` 26) o superior.
