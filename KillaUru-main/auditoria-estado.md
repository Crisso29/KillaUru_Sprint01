# Auditoría de estado — KillaUru

**Generado:** 2026-11-09  
**Propósito:** Leer y borrar. Resumen de qué está hecho, qué falta y qué descrepancias hay.

---

## Resumen ejecutivo

| Feature | Título | tasks.md | Trazabilidad | Código |
|---------|--------|----------|--------------|--------|
| 001 | Plataforma backend | 72/81 (89%) | 41/54 (76%) | ✅ 423 cs prod + 155 test |
| 002 | Módulo Auditoría | 35/38 (92%) | 14/19 (74%) | ✅ 30 cs |
| 003 | Módulo Identidad | **65/65 (100%)** | **45/45 (100%)** | ✅ 128 cs + 21 test |
| 004 | Módulo Anomalías | 56/58 (97%) | 49/54 (91%) | ✅ 110 cs + 13 test |
| 005 | Módulo Ingesta | ⚠️ 0/40 (0%) | 31/33 (94%) | ✅ 46 cs + 11 test |
| 006 | Módulo Mapa/Consulta | ⚠️ 0/33 (0%) | 29/31 (94%) | ✅ 37 cs + 6 test |
| 007 | Módulo Dashboard | ⚠️ 0/26 (0%) | **23/23 (100%)** | ✅ 25 cs + 6 test |
| 008 | App Android | 92/128 (72%) | 67/72 (93%) | ✅ 211 kt prod + 58 test |
| 009 | Dataset y modelo ML | ⚠️ 0/74 (0%) | 12/28 (43%) | ✅ 42 py prod + 24 test |
| 010 | Panel de administración | ⚠️ 0/94 (0%) | **50/50 (100%)** | ✅ 194 ts/tsx + 30 test |

**Total tasks:** 320/637 tareas en tasks.md — pero ver "⚠️ Discrepancia" abajo.

---

## ⚠️ DISCREPANCIA IMPORTANTE: tasks.md no está actualizado

Los features 005, 006, 007, 009 y 010 muestran **0 % en tasks.md** pero:

- **El código está escrito** (hay archivos .cs, .py, .ts/.tsx en disco).
- **La trazabilidad dice que casi todo está hecho** (005: 94%, 006: 94%, 007: 100%, 010: 100%).

**Causa:** tasks.md fue creado con todas las casillas vacías `[ ]` y nunca se actualizó conforme se fue implementando.

**Qué hay que hacer:** revisar cada feature e ir marcando `[x]` las tareas que ya están hechas. Las listas de pendientes reales que importan son las de **trazabilidad**, no las de tasks.md.

---

## Feature 001 — Plataforma backend (89%)

### Hecho
- Toda la infraestructura: Docker Compose, PostgreSQL+PostGIS, Cloudflare, backup, Alloy.
- 6 módulos compilados (Identidad, Auditoría, Dashboard, Ingesta, Mapa, Anomalías).
- CI/CD parcialmente listo (`deploy/ci/`, `backend/deploy/`).
- `quickstart.md`, `TimeProvider`, `IContextoTransaccion`, rate limiting, OpenAPI/Scalar.
- 72 de 81 tareas tachadas.

### Pendiente (9 tareas — todas operacionales/externas)
| Tarea | Descripción | Tipo |
|-------|-------------|------|
| T050 | Configurar Grafana Cloud (paneles, alertas, UptimeRobot) | Cuenta externa |
| T054 | Simulacro en «killauru-pruebas» | Entorno real |
| T055 | Completar 6 READMEs con «Cómo se prueba» | Documentación |
| T056 | Ejecutar `quickstart.md` completo y marcar trazabilidad | Validación |
| T057 | Alertas de costo en Oracle/Cloudflare (US$ 8) | Cuenta externa |
| T070 | Proteger rama `main` en GitHub (check ci obligatorio) | Config GitHub |
| T072 | Cloudflare Access para SSH (token de servicio) | Cuenta externa |
| T079 | Pruebas de carga (5 escenarios, anotar resultados) | Espera 005+006 |
| T080 | Prueba punta a punta lote→anomalía→mapa→indicador | Espera 004–007 |

**Veredicto:** 100% del código está. Solo faltan cuentas externas y validaciones en entorno real.

---

## Feature 002 — Módulo Auditoría (92%)

### Pendiente (3 tareas)
| Tarea | Descripción | Tipo |
|-------|-------------|------|
| T027 | Reglas de alerta en Grafana para acciones de riesgo | Cuenta externa |
| T031 | Simulacro EQ-13 en «killauru-pruebas» | Entorno real |
| T038 | Revisión de código: 003/004 no llaman `RegistrarAccionRiesgo` | Revisión manual |

---

## Feature 003 — Módulo Identidad ✅ COMPLETO

128 archivos .cs de producción + 21 de test. 65/65 tareas. 45/45 trazabilidad. Nada pendiente.

---

## Feature 004 — Módulo Anomalías (97%)

### Pendiente (2 tareas)
| Tarea | Descripción | Tipo |
|-------|-------------|------|
| T053 | Medir p95 con 300 dispositivos (junto con 005) | Requiere 005 listo |
| T054 | Escenario EQ-12 completo (ataque + subida legítima + lluvia) | Prueba integración |

---

## Feature 005 — Módulo Ingesta (⚠️ tasks.md sin marcar)

**Código:** 46 archivos .cs de producción + 11 de test.  
**Trazabilidad:** 31/33 completado (94%).  
**tasks.md:** 0/40 (sin marcar — hay que corregir esto).

### Pendientes reales (de trazabilidad, 2 ítems)
- Aún no determinados sin revisar trazabilidad en detalle — probablemente son operacionales.

### Acción
1. Ir a `specs/005-modulo-ingesta/tasks.md` y marcar las 40 tareas según el estado real del código.
2. Verificar los 2 ítems pendientes en `specs/005-modulo-ingesta/checklists/trazabilidad.md`.

---

## Feature 006 — Módulo Mapa/Consulta (⚠️ tasks.md sin marcar)

**Código:** 37 archivos .cs de producción + 6 de test.  
**Trazabilidad:** 29/31 completado (94%).  
**tasks.md:** 0/33 (sin marcar).

### Acción
Mismo proceso que 005: marcar tasks.md y revisar los 2 ítems pendientes en trazabilidad.

---

## Feature 007 — Módulo Dashboard (⚠️ tasks.md sin marcar)

**Código:** 25 archivos .cs de producción + 6 de test.  
**Trazabilidad:** 23/23 completado (100%).  
**tasks.md:** 0/26 (sin marcar).

**Probablemente completo.** Solo falta actualizar tasks.md.

---

## Feature 008 — App Android (72%)

**Código:** 211 archivos .kt producción + 58 test.  
**tasks.md:** 92/128 (72%). **36 pendientes.**

### Pendientes por categoría

**Tests UI (Compose/Robolectric) — faltantes:**
- T027 `PrimerUsoUiTest`, T028 `AjustesUiTest`
- T044 `ClasificadorLiteRtTest`, T045 `EventoDaoTest`
- T053 `ServicioDeteccionTest`

**Tests de sincronización:**
- T060–T066: lote, contrato, Worker, corte de red, purga, tamaño BD, migraciones

**Tests de mapa y actualización:**
- T082–T089: `ActualizarAnomaliasUseCase`, `AvisoFueraDemo`, `RepositorioAnomaliasRoom`, contratos, descarga reanudable, política de descarga, CDN, UI del mapa

**Código de producción faltante:**
- T090 `Corredor`, `PosicionEnVia`, `Vertice` — módulo corredor
- T091 Puertos y casos de uso de mapa (`ActualizarAnomaliasUseCase`, `AvisoFueraDemo`)
- T092 `AnomaliaDao`, `RepositorioAnomaliasRoom`, `MapaApi`, `MapeadorGeoJson`
- T095 `DescargaMapaPantalla` + ViewModel

**Tests de alerta de proximidad:**
- T100 `ProximidadTrayectoriasTest`, T101 `AlertaSonoraAndroidTest`

**Tests de sesión (feature 006 de Android):**
- T108 `RenovadorTokenTest`, T109 `ContratoIdentidadTest`, T110 `CuentaUiTest`

**Tests de modelo ML en Android:**
- T119 `ContratoManifiestoModeloTest`

**Pruebas de campo (requieren hardware físico):**
- T121 Informe de campo (Ayacucho–Vinchos)
- T122 `InferenciaBenchmarkTest` (3 celulares, p95 < 50 ms)
- T123 Prueba campo EQ-01 (5 h en modo avión)
- T124 Batería, datos, almacenamiento (3 celulares)
- T125 Conciliación de punta a punta

**Operacional:**
- T127 Correr `app-datos-r2.yml` y publicar

---

## Feature 009 — Dataset y modelo ML (⚠️ tasks.md sin marcar)

**Código Python:** 42 archivos producción + 24 test = **66 archivos** (en `ml/killauru_ml/` y `ml/tests/`).  
**Trazabilidad:** 12/28 completado (43%).  
**tasks.md:** 0/74 (sin marcar).

### El código EXISTE y cubre:
- `killauru_ml/comun/`: config, entorno, huella, semillas
- `killauru_ml/etiquetado/`: acuerdo, candidatos, validar_etiquetas
- `killauru_ml/evaluacion/`: métricas, registro
- `killauru_ml/features/`: features, prefiltro
- `killauru_ml/lectura/`: catálogo, grabación, ingerir, integridad
- `killauru_ml/modelo/`: arquitectura, cuantizar, entrenar, manifiesto, puerta
- `killauru_ml/particion/`: no_fuga, particion, tamaño
- `killauru_ml/pipeline/`: sintético
- `killauru_ml/senal/`: componentes, condicion_velocidad, gravedad, remuestreo
- `killauru_ml/ventanas/`: ventaneo
- 24 archivos de test en `ml/tests/`

### Pendiente real (trazabilidad: 16 ítems pendientes)
Revisar `specs/009-dataset-y-modelo/checklists/trazabilidad.md` para ver cuáles son.  
Probablemente son: grabaciones de campo reales, entrenamiento final, publicación del modelo `.tflite` y manifiesto.

---

## Feature 010 — Panel de administración (⚠️ tasks.md sin marcar)

**Código:** 194 archivos .ts/.tsx producción + 30 test.  
**Trazabilidad:** **50/50 completado (100%)**.  
**tasks.md:** 0/94 (sin marcar).

**Muy probablemente completo.** La trazabilidad dice 100%. El código existe. Solo falta:
1. Actualizar las 94 casillas en tasks.md.
2. Verificar si las tareas E2E (Playwright) están corriendo.

---

## Resumen de lo que REALMENTE falta

### Código aún no escrito
| Área | Qué falta |
|------|-----------|
| Android | Módulo corredor (T090), dominio de mapa (T091), datos de mapa (T092), UI descarga (T095) |
| Android | Tests de sincronización T060–T066 |
| Android | Tests de mapa T082–T089 |
| Android | Tests de alerta T100–T101, sesión T108–T110, ML T119 |

### Validaciones que requieren entorno real
- T054/001, T031/002: simulacros en «killauru-pruebas»
- T053/004: prueba de carga con 300 dispositivos
- T054/004: escenario EQ-12
- T121–T125/008: pruebas de campo con hardware físico

### Configuración de cuentas externas
- Grafana Cloud: T050/001, T027/002
- GitHub branch protection: T070/001
- Cloudflare Access SSH: T072/001
- Oracle/Cloudflare cost alerts: T057/001

### Tareas administrativas (no bloquean el código)
- Actualizar tasks.md para 005, 006, 007, 009, 010
- Completar READMEs de módulos (T055/001)
- Grabaciones de campo reales para ML
- Publicar modelo `.tflite` final

---

## Prioridad recomendada

1. **Inmediato (hoy):** Marcar tasks.md de 005, 006, 007, 010 según código existente — te quitarás la falsa sensación de tener el 50% cuando probablemente estás al 80%+.
2. **Corto plazo:** Terminar Android (módulo corredor + dominio/datos mapa + tests faltantes de sincronización y mapa).
3. **Medio plazo:** Conseguir grabaciones de campo reales para ML; entrenar modelo final; publicar `.tflite`.
4. **Cuando tengas entornos:** Simulacros, pruebas de carga, Grafana, GitHub protection, Cloudflare Access.

---

*Puedes borrar este archivo cuando hayas leído lo que necesitas.*
