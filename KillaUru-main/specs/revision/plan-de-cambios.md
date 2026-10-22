# Plan de cambios — aplicación de D1…D17 y correcciones técnicas (2026-10-21)

Fuente: `decisiones.md` (lo que decidió el autor) y los 6 informes `revision-*.md` (hallazgos con archivo:línea).
**Regla de oro (condición del autor en D1):** todo coherente, consistente y lógico; **no alterar otras cosas mayores**. Cada cambio es puntual, en el archivo que ya corresponde, sin mover archivos ni carpetas de docs (estructura fijada por la docente). Si un hallazgo de un informe choca con una decisión de abajo, **gana la decisión**.

Los nombres, valores y SQL de este archivo son **canónicos**: todos los archivos deben usarlos exactamente igual.

---

## A. Decisiones del autor → qué cambia

### D1 · Docs también recibe los agregados de las specs
Se agregan a docs (y se marcan como confirmados en los `research.md`, sección «Pendientes de confirmar por el autor» → «Confirmados por el autor el 2026-10-21»):
- `ContextoReporte(bool ConSesion, int EventosEnLaHora, DateTimeOffset RecibidoUtc)` e `IAnomaliasService.EvaluarIntrinseca(evento, recibidoUtc)` (componentes §3.1, Nivel3 §2.2, Nivel4, enfoque §6, diseño-interno §6).
- Acción del administrador `MantenerDescartada` (fila nueva en la tabla «Acciones del administrador» de RN-07; patrones §2.1 dice «3 acciones» → 4).
- `IAnomaliaLectura.CeldasDelCorredor()` (componentes §3.1, Nivel3, diseño-interno §4).
- `MapaOptions` sin «tamaño de celda» (enfoque §5): la celda es la de `Shared` (0,005°).
- `Shared/Persistencia/IContextoTransaccion` y `Shared/Domain/Roles` (enfoque §4, componentes §3 fila Shared, diseño-interno §2).
- `Celda.Limites()` (diseño-interno §6.1).
- Columnas `transicion.actor_id`, `reporte_anomalia.km_via` y **`anomalia.km_via`** (D13) en diseño-interno §3.3; vencimiento al reabrir = momento de la reapertura + caducidad (RN-07).
- Polígono demo: km 268 (Vinchos) → 333 (Ayacucho), margen 200 m, ≤ 60 vértices (RN-08 / glosario `PoligonoDemo`).
- `DiasRecuperables = 2` y fórmulas del Dashboard (007 R-002, R-003) en diseño-interno §3.8.
- Los demás «Pendientes» de los `research.md` de 001…007 se resuelven igual: si no chocan con D2…D17, se pasan a docs; si chocan, se ajustan a la decisión.

### D2 · Un lote = un seudónimo; techo por IP
- **Regla nueva de recepción (RN-11):** motivo `SeudonimoInconsistente` (código **8**, se evalúa en **Recepción**, en Ingesta). Seudónimo del lote = el de la cabecera `X-KillaUru-Seudonimo` si viene; si no, el del **primer** evento con seudónimo válido. Todo evento con otro seudónimo sale en `rechazados` con `SeudonimoInconsistente`. La app parte el lote cuando rota su seudónimo (nota para la spec 008).
- `MotivoDescarte` (004 Contracts) suma `SeudonimoInconsistente = 8`; el enum de `rechazados` del contrato del lote lo incluye.
- **Techo por IP** (solo peticiones anónimas, clave = IP real de `CF-Connecting-IP`): `LimiteTasa:TechoIpLotesPorHora = 600`, `LimiteTasa:TechoIpConsultasPorHora = 300` en `ApiOptions` (001). Se suma a la cuota por seudónimo + IP; 429 con `Retry-After`. RNF-11 y ADR-015 se actualizan.
- 005 `research.md` R-005 ya no dice «un lote viene de un solo celular» como supuesto: ahora es regla.

### D3 · Que la hora no una cuenta con eventos
- `loteId` lo genera **el servidor** (UUIDv7): se quita del cuerpo del lote; la respuesta lo sigue devolviendo. `evento_recibido.lote_id` pasa a `NULL`able.
- **Purga de 72 h (RN-09), en la misma sentencia que pone el seudónimo en `NULL`:**
  - `ingesta.evento_recibido`: `seudonimo = NULL, lote_id = NULL, recibido_utc = date_trunc('hour', recibido_utc)`.
  - `anomalias.reporte_anomalia`: `seudonimo = NULL, recibido_utc = date_trunc('hour', recibido_utc)`.
- **Identidad:** `sesion_refresh.expira_utc` se guarda truncado hacia abajo a la hora; `usado_utc` se guarda exacto (lo necesita la gracia de 60 s) y la tarea de purga de Identidad lo trunca a la hora cuando tiene más de 2 min; `usuario.creado_utc` se guarda truncado al día. Purga diaria: borrar `sesion_refresh` con `expira_utc` o `revocado_utc` de hace más de **30 días** y `token_correo` vencidos o usados hace más de 30 días (también resuelve BD-05). RN-09 y RNF-10 en docs lo dicen.

### D4 · Adapters solo usa Application (`enfoque` §3)
- Ningún controlador ni DTO referencia tipos de `Domain/`. Lo que hoy cruzaba pasa a **Application**: `LoteEntrante`, `LoteAceptado`, `ResultadoLote` (Ingesta); `ResultadoModeracion` y un comando de moderación con la acción como texto o enum de Application (Anomalías; el dominio conserva `AccionAdmin` porque es su tabla de RN-07, y Application lo mapea); los resultados de Identidad (partidos en varios archivos ≤ 100 líneas); `IndicadorResumen` y `ResumenDashboard` (Dashboard).
- El **dominio no usa contratos de otro módulo**: Ingesta mapea `EventoReportado` en Application; `DiaIndicadores` se construye con valores simples y el mapeo desde `ResumenDia` vive en Application.
- `enfoque` §4.1 fila Ingesta: «Domain mínimo (estado de recepción, hora de detección)»; el lote y su resultado son de Application. `diseño-interno` §6.6 sigue igual (el controlador usa `ResultadoLote`, ahora de Application).
- 001: la prueba de arquitectura agrega las reglas que faltan (ARQ-03): Adapters ↛ Domain; Domain ↛ `Contracts` de otro módulo; Domain ↛ Application/Adapters/Infrastructure; Application ↛ Adapters/Infrastructure.

### D5 · Evento sin `eventoId`
- El lote no falla. Cada elemento de `rechazados` es `{ id: uuid | null, indice: int, motivo }` con `indice` = posición 0-based en el arreglo `eventos` (siempre presente). Sin `eventoId` UUID → `id: null`, `EsquemaInvalido`.
- 400 `lote_invalido` solo: cuerpo ilegible, sin arreglo `eventos`, vacío o > 50 (RF-11 tal cual). El contrato del lote de `03-requisitos-funcionales.md` agrega `indice`; la app marca `REJECTED_LOCAL` el evento de esa posición.

### D6 · Estados del administrador
- Sin `estado`, el administrador recibe **los 5 estados** (RN-08). Se quita `EstadosAdministradorPorDefecto` de `MapaOptions` y de 006. El panel abre con «Activa + PendienteConfirmacion» marcados (nota para 010 y el prototipo).

### D7 · `Descartada` y la purga de 2 años
- RN-10: una `Descartada` se borra solo si `ultima_deteccion_utc` y `estado_desde_utc` tienen más de 2 años (2 años sin ningún reporte nuevo). `Expirada` y `Reparada`: sin cambio. Reportes: se borran a los 2 años de su detección, como siempre. La purga histórica de 004 cambia su SQL.

### D8 · Cuentas desechables
- El token de acceso lleva el claim `verificado` (bool). `con_sesion = true` solo si rol = `ConductorRegistrado` **y** `verificado = true` (Ingesta, RN-14). RN-14 y HU-18b en docs lo dicen; tras verificar, el claim cambia en la siguiente renovación (≤ 5 min).
- Límite: `LimiteTasa:RegistrosPorHoraPorIp = 5` (`ApiOptions`), 429.
- «Correo en uso» (409) queda como **riesgo aceptado** (005-restricciones o 07, sección de riesgos), mitigado por el límite.

### D9 · Restaurar un backup
- Runbook `restaurar.md` (001): antes de reabrir, revocar todas las `sesion_refresh` (`UPDATE … SET revocado_utc = …`), rotar `KU_JWT_CLAVE_FIRMA`, arrancar la API (las tareas corren al arrancar, ver T-OPS05) y comprobar 0 seudónimos > 72 h. Las eliminaciones, desactivaciones y cambios de rol posteriores al backup se pierden: **riesgo aceptado**, acotado por el RPO de 24 h; el runbook pide revisar a mano las solicitudes de eliminación del último día. RN-10 y RNF-07 en docs lo dicen; el simulacro mensual lo verifica.

### D10 · CI/CD (GitHub Actions)
- `.github/workflows/ci.yml` (cada PR): `dotnet build` (advertencias = errores), pruebas unitarias, de arquitectura (incluye límite de líneas), de integración (Testcontainers), de contrato OpenAPI, cobertura con **coverlet** ≥ 90 % en los proyectos `Domain` (puerta). `main` protegida: no se une sin CI en verde.
- `.github/workflows/cd.yml` (merge a `main`): imagen arm64 (runner `ubuntu-24.04-arm`) → `ghcr.io/<cuenta>/killauru-api:<sha>` → despliegue automático: SSH por **Cloudflare Access** (sin puertos abiertos) a la VM y `desplegar.sh <sha>` (respaldo → migraciones → `docker compose up -d` → espera `/health/ready`). Volver atrás = `desplegar.sh <sha-anterior>`. Entorno de GitHub `produccion`.
- Secretos nuevos en RNF-34: `KU_CF_ACCESS_CLIENT_ID`, `KU_CF_ACCESS_CLIENT_SECRET`, `KU_DEPLOY_SSH_KEY` (secretos de GitHub) y `KU_PG_SUPERUSUARIO` (T-BD01).
- Docs: RNF-17/RNF-18 (despliegue), RNF-28 («Pipeline de CI»), ADR-010, constitución (fila Despliegue).

### D11 · Carga, estrés, resistencia y caos
- Scripts en `backend/tests/carga/`: `carga-combinada.k6.js` (300 sincronizando + 300 mapa, 10 min), `picos.k6.js` (ráfaga de 600), `estres.k6.js` (escalera de 100 en 100 hasta p95 > 2× la meta o errores > 1 %; se registra el punto de quiebre), `resistencia.k6.js` (4 h al 50 %), `caos.md` + `caos.sh` (`kill -9` del contenedor de la API bajo carga; conciliación exacta). Los scripts de 005 y 006 se reemplazan por estos (una sola feature dueña: **001**, que tiene el entorno; 005 y 006 los referencian).
- Corren **fuera de la VM** (PC del autor o workflow manual de GitHub `carga.yml`) contra el **entorno de pruebas** R-11, que pasa a admitir pruebas de carga: compose de pruebas con límites (`cpus: 2`, `mem_limit: 6g` para API+BD de pruebas), volumen aparte, `ApiOptions` de pruebas con límites de tasa altos; en horario de poco tráfico. VM: **A1 de 4 OCPU / 24 GB** (Always Free) con límites por contenedor también en producción.
- Error = todo 5xx; 503 con `Retry-After` solo aceptado en `estres`; 429 no debe aparecer. Frecuencia: antes de cada versión y mensual (calendario en runbook `simulacros.md`).
- Métrica nueva: `IMetricas.RegistrarRetrasoCola(TimeSpan)` → histograma `killauru.ingesta.retraso_cola_s` (procesado − recibido), emitida por Ingesta (RNF-23).
- Docs: R-11 (05-restricciones), RNF-03/RNF-23 y EQ-06/EQ-14 (cómo se mide), enfoque §8 fila Sistema.

### D12 · Grafana Alloy
- Contenedor `alloy` en el compose de producción (imagen oficial `grafana/alloy`, fija por versión): métricas de la VM (disco, CPU, memoria), logs de Docker hacia Grafana Cloud y métrica del backup: `respaldar.sh` escribe al terminar bien `killauru_tarea_ultima_ok{tarea="backup"} <unix>` en un archivo `.prom` de un volumen compartido; Alloy lo lee. Alertas: disco > 80 %, tarea > 26 h. Rotación de logs de Docker (`json-file`, `max-size: 10m`, `max-file: 3`). Solo envía (cumple ADR-010); dentro de RNF-32. Docs: ADR-010, RNF-22, Nivel2.

### D13 · Prototipo
- Contraseña mínima **10** (003: `ContrasenaMinima = 10`, OpenAPI `minLength: 10`; docs RF-21).
- Prototipo `referencia/prototipo/killauru-diseno.html`: (a) la app de login muestra el mensaje genérico de RN-19 («Correo o contraseña incorrectos. Si fallas varias veces, espera unos minutos.»), sin cuenta regresiva; (b) eliminar cuenta pide la contraseña; (c) «Eventos hoy» → «Eventos ayer» y su ayuda; (d) detalle del bache en la app: «permanente» sin el número de días; (e) filtro Estado del mapa del panel abre con «Activa» y «PendienteConfirmacion» marcados.
- `kmVia`: columna `anomalia.km_via real NOT NULL` calculada **al nacer** con `CorredorVia.KmSobreVia` (posición fija); `AnomaliaVisible` y `AnomaliaCambio` suman `KmVia`; la *feature* del mapa suma `kmVia` (todos los roles); detalle y listas del administrador lo toman de la anomalía.

### D14 · Reglas finas
- RN-07 / 004 FR-019: una `Reparada` vuelve a nacer con un reporte **detectado después de `estado_desde_utc`** (la reparación); la condición «después del vencimiento» queda solo para `Expirada`.
- Glosario «Reaparición» = «anomalía que **vuelve a nacer** después de expirar (hacia `Activa` o `PendienteConfirmacion`)»; RN-07 línea del Dashboard igual.
- RF-18: «eventos válidos por día» y cobertura con «eventos válidos» (= reportes procesados); 007 muestra «Eventos válidos».

### D15 · Auditoría
- `IAuditoriaService.RegistrarAsync(accion, actor, objetivo, contexto, ct)` **exige** una transacción unida en `IContextoTransaccion`; si no hay, lanza `InvalidOperationException` (programación, no flujo de negocio).
- Nuevo `IAuditoriaService.RegistrarLecturaAsync(accion, actor, objetivo, contexto, ct)`: transacción propia, solo para lecturas sensibles.
- Lecturas que se registran (acción `AccesoAdministrativoDatos` salvo que ya tenga una propia): lista y detalle de usuarios y `GET /usuarios?ids=` (Identidad), consulta de la auditoría (Auditoría), exportación (`DatasetExportado`, Anomalías: **cuenta las filas primero**, audita con `filas` y luego escribe, PO-12). Mapa y Dashboard no se registran. Lo hacen los módulos dueños; `componentes` §3 (fila API REST) y §3.1 (consumidores) se ajustan.
- `IContextoTransaccion` suma `AlConfirmar(Func<Task>)` (se ejecuta después del commit) y libera la transacción tras commit/rollback (ARQ-10). La métrica de acciones de riesgo la emite **solo Auditoría**, con `AlConfirmar` (SEG-18, OPS-16).

### D16 · Datos del corredor
- Se definen en la spec de la app (008). Fuente única desde ya: `db/datos/corredor_pe28a.geojson` (004 T002) y `db/datos/tramo_demo.geojson` (006 T002), con atribución de OpenStreetMap.

### D17 · Specs nuevas
- `specs/README.md`: filas planificadas 008-app-android, 009-dataset-y-modelo (HU-28), 010-panel-admin (estado «por escribir»), y la tabla RF→feature corregida (RF-08 parte servidor → 006).

---

## B. Correcciones técnicas sin decisión (canónicas)

| ID | Corrección | Archivos |
|---|---|---|
| T-BD01 | `V0000__arranque.sql` lo corre `migrar.sh --arranque` **como superusuario** (`KU_PG_SUPERUSUARIO`): roles `killauru_migrador`/`killauru_app`, extensiones `postgis` y `citext`, esquemas, tabla `migraciones.aplicada`, permisos. V0001…V0007 con `killauru_migrador`, `psql -v ON_ERROR_STOP=1 --single-transaction`. Testcontainers aplica los mismos scripts con los mismos roles. Backup = `pg_dumpall --roles-only` + `pg_dump`. Orden del primer despliegue: arranque → migraciones → `up` | 001 (+ docs diseño-interno §3.7, RNF-34) |
| T-BD02 | Carrera caducidad/trabajador: el trabajador y la moderación leen la anomalía con `SELECT … FOR UPDATE` dentro de su bloqueo de celda y revalidan el estado; la caducidad conserva `FOR UPDATE SKIP LOCKED` | 004 |
| T-BD06 | `CREATE INDEX ix_recibido_purga ON ingesta.evento_recibido (recibido_utc) WHERE seudonimo IS NOT NULL;` | 005 |
| T-BD-CHK | CHECK faltantes: `persistencia IN (0,1)`, `transicion.desde/hacia BETWEEN 1 AND 5`, `evento_recibido.motivo BETWEEN 1 AND 8`, `confidence BETWEEN 0 AND 1`; quitar `ix_anomalia_pendiente` si sobra (BD); prueba «sin FK entre esquemas» | 004, 005, 001 |
| T-OPS05 | **Tareas por reloj, no por `PeriodicTimer` de 24 h.** Todas corren **al arrancar** y luego: cada hora en el minuto fijo (caducidad `:05`, purgas de seudónimo `:10`); diarias en hora local fija: 00:30 indicadores (007), 02:00 backup (host), 03:00 purga de recepción 30 d (005), 03:30 purga histórica (004), 03:45 purga de Identidad (003). Helper común `Shared/Tareas/ProgramaReloj` (próximo instante con `TimeProvider`). Nombres de métrica: `caducidad`, `purga-seudonimos`, `purga-seudonimos-recepcion`, `purga-recepcion`, `purga-historica`, `purga-identidad`, `indicadores`, `backup` | 001, 003, 004, 005, 007 (+ componentes §4, diseño-interno §3.8) |
| T-OPS08/09 | `restart: unless-stopped` + `healthcheck` en el compose; rotación de logs (D12) | 001 |
| T-OPS14/15 | Emisores: Anomalías llama `RegistrarRetenidoPorTope` y `RegistrarDuracionProceso`; Ingesta `RegistrarRetrasoCola` (D11); Mapa: aciertos de caché vía métricas propias de HybridCache o contador `killauru.mapa.cache{resultado}` (baja cardinalidad) | 001, 004, 005, 006 |
| T-OPS17 | Alertas de p95 por ruta: mapa 250 ms, lote 300 ms (docs); el resto, metas SDD marcadas como tales | 001 |
| T-OPS19 | `/health/ready`: BD + cola de Ingesta + corredor cargado (Anomalías) + migraciones al día; documentado en 001 FR-018 y en docs (RF-28 / ADR-010) | 001, 004 |
| T-ARQ06 | Nombres de docs: `IAnomaliaRepository`, `AnomaliaRepositoryPostgis` (y cualquier otro renombrado sin razón) | 004 |
| T-ARQ07/08 | Puertos que faltan en 004 (listas, detalle, exportación, resumen); partir archivos que pasarían 100 líneas (`UsuariosController`, `TareasRepositorio`, `AnomaliaRepositoryPostgis`, `PendientesRepositorio`, `Resultados.cs`) | 003, 004, 005 |
| T-QA16 | `Microsoft.CodeAnalysis.BannedApiAnalyzers` con `BannedSymbols.txt` (`DateTime.Now/UtcNow`, `DateTimeOffset.Now/UtcNow`) en `Directory.Build.props` | 001 |
| T-QA17 | La prueba de 100 líneas cubre `.cs`, `.sql` de consultas, `.sh` y `.js`; excluye migraciones `V*.sql`, generados y OpenAPI (constitución VIII) | 001 |
| T-QA18 | Valores límite faltantes: 150/151 km/h, tolerancia futuro 5 min ± 1 s, cuotas 120/600/1 000/3 000, registro 5/6 | 001, 003, 004 |
| T-QA20 | Prueba de punta a punta: lote → anomalía → mapa (invalidación) → indicador del día — `KillaUru.Integracion.Tests/PuntaAPunta/` | 001 (o 007 como última) |
| T-QA14 | Verificaciones periódicas (restauración mensual, simulacro de incidente semestral, revisión de costo mensual, revisión de privilegios mensual) en un calendario del runbook, no como tareas únicas | 001, 002, 003 |
| T-SEG03 | Política de logs: nunca seudónimo, IP, tokens, correos ni el par `sub`+`loteId`; filtro de Serilog + prueba | 001 |
| T-SEG09 | `ClockSkew = TimeSpan.Zero` (token ≤ 5 min, RNF-26) | 001/003 |
| T-SEG14 | El enlace del primer administrador no sale por logs (se imprime solo en la consola del comando de semilla) | 003 |
| T-SEG16 | `updatedSince` anónimo: las bajas solo de anomalías dentro del polígono demo | 006 |
| T-SEG19 | 429 declarado en todas las operaciones de todos los contratos | 002, 003, 004 |
| T-SEG20 | Regla única (convenciones-api): sin token en endpoint que exige sesión → 401; con token y rol insuficiente → 403; parámetro reservado (`estado`) usado por otro rol, con o sin token → 403. 002 y 007 dejan de decir «403 o 401» | 001, 002, 006, 007 |
| T-PO08 | `venceUtc` admite `null` (estados finales) en mapa y detalle | 004, 006 |
| T-PO11 | Columnas de exportación iguales al prototipo: `id, tipo, severidad, estado, persistencia, km, latitud, longitud, reportes, primer_reporte, ultimo_reporte` | 004 |
| T-PO19 | `ElementoPorRevisar` suma `severidadMostrada` | 004 |
| T-PO18/22/27/28 | Tabla RF→feature del README; fases recortables marcadas; nombres del glosario (`EstadoPersistencia`, etc.) | todos |

Cualquier otro hallazgo **sin** decisión del autor de los informes se aplica si es coherente con lo de arriba; si no lo es, se anota en `revision/no-aplicado.md` con el motivo.
