# Quickstart: comprobar el módulo Auditoría

## Requisitos previos

Plataforma (001) funcionando; migraciones `V0001` y `V0002` aplicadas; un token de administrador de desarrollo (cuando exista 003, uno real; antes, uno firmado con la clave de desarrollo).

## 1. Pruebas automáticas

```powershell
cd backend
dotnet test tests/KillaUru.Auditoria.Tests
dotnet test tests/KillaUru.Integracion.Tests --filter "FullyQualifiedName~Auditoria"
```

**Esperado:**

| Prueba | Qué demuestra |
|---|---|
| `TransaccionCompartidaTests.Accion_y_registro_se_confirman_juntos` | US1-1 |
| `TransaccionCompartidaTests.Si_la_auditoria_falla_la_accion_se_revierte` | US1-2, RNF-31 |
| `InmutabilidadTests.La_app_no_puede_actualizar_ni_borrar` | FR-008 (`permission denied` al hacer `UPDATE`, `DELETE` y `TRUNCATE` con `killauru_app`) |
| `RegistroAuditoriaTests.Rechaza_clave_correo` | FR-006, RF-30 |
| `AuditoriaApiTests.Solo_el_administrador_consulta` | US2-3 (401 sin token, 403 con conductor) |
| `AuditoriaApiTests.La_consulta_se_audita` | FR-012 (con `RegistrarLecturaAsync`) |
| `TransaccionCompartidaTests.Sin_transaccion_unida_lanza` | FR-002, D15 (`InvalidOperationException`) |
| `TransaccionCompartidaTests.Lectura_usa_transaccion_propia` | FR-017, D15 |
| `TransaccionCompartidaTests.Metrica_de_riesgo_solo_tras_commit` | FR-013, SC-006 |

## 2. Consulta manual

```powershell
curl -s "http://localhost:8080/api/v1/auditoria?categoria=Sesion&tamano=20" -H "Authorization: Bearer $env:KU_TOKEN_ADMIN"
```

**Esperado:** 200 con `elementos` ordenados del más reciente al más antiguo, `total` y una nueva fila `AccesoAdministrativoDatos` en la siguiente consulta.

## 3. Alertas de riesgo (entorno de pruebas)

Registrar una exportación y 5 bloqueos con el script de semillas `db/semillas/auditoria-riesgo.sql`; comprobar en Grafana las dos alertas.

## 4. Simulacro EQ-13 (semestral)

Seguir `deploy/runbooks/incidentes-datos-personales.md` en «killauru-pruebas» con cronómetro; meta: borrador de notificación en ≤ 48 h.
