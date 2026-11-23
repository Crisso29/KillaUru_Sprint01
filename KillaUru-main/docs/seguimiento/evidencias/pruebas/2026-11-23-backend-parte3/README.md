# Backend, parte 3: DT-03, DT-04 y la prueba intermitente de 50 eventos

> 2026-11-23, local con Docker 29.8.2 y Testcontainers (PostgreSQL/PostGIS real). [Volver a evidencias](../../README.md) ·
> [Deuda técnica](../../../deuda-tecnica.md) · [Revisión de calidad anterior](../2026-11-23-calidad/README.md)

## 1. Resultado

| Medida | Antes | Ahora |
|---|---|---|
| Pruebas del backend ([RESUMEN.md](RESUMEN.md)) | 1 153: 1 149 aprobadas, **1 fallida**, 3 omitidas | **1 155: 1 152 aprobadas, 0 fallidas**, 3 omitidas |
| Cobertura del dominio (puerta de CI 90 %) | 97–100 % | 97–100 % ([resumen](cobertura-resumen.md)) |
| Pruebas de arquitectura | 497 | 498 (+ `ChequeoDeSaludPostgresTests`) |

Repetir desde `backend/`: `pwsh tests/generar-resultados.ps1` (requiere Docker).

## 2. Los tres problemas

| Problema | Causa | Arreglo | Cómo se probó |
|---|---|---|---|
| **DT-03** — el migrador salía con `exit 2` al crear la VM (001/T083) | En el primer arranque la imagen de PostgreSQL levanta un servidor temporal solo por socket y se reinicia; `pg_isready` sin `-h` lo daba por sano | `pg_isready -h 127.0.0.1` en `docker-compose.yml` y `docker-compose.pruebas.yml` (ya estaba en `local`) | **Prueba de arquitectura** (revisa archivos de configuración): [ChequeoDeSaludPostgresTests](../../../../../backend/tests/KillaUru.Arquitectura.Tests/ChequeoDeSaludPostgresTests.cs) recorre **todo** `deploy/docker-compose*.yml`, también los futuros. Rojo con los 2 archivos señalados → verde |
| **DT-04** — `ConsolaPrivacidadTests` fallaba a veces con `chunkLength` (001/T084) | La escritura **ya** tenía candado; lo que fallaba era **leer** el `StringWriter` sin candado mientras el host seguía registrando | `SinkDePrueba` guarda su texto en privado y solo se lee con `Capturado`, bajo el mismo candado: no queda forma insegura de usarlo | **Prueba unitaria** del soporte, sin base de datos: [SinkDePruebaTests](../../../../../backend/tests/KillaUru.Integracion.Tests/specs/003-modulo-identidad-accesos/soporte/SinkDePruebaTests.cs) — 8 hilos escriben 16 000 eventos mientras otro lee. **3 de 3 en rojo** con el mismo error de CI; **5 de 5 en verde** después. La prueba real, 3 de 3 |
| **Lote de 50** — `005/RecepcionApiTests` fallaba a veces en la suite completa | No esperaba un tiempo fijo (eso se dijo por error en la revisión anterior): ya sondeaba cada 200 ms. El tope de 15 s era corto: un lote lo procesa un trabajador, evento tras evento, a ~186 ms (DT-01) | Tope de 60 s, justificado en el código; el sondeo sale en cuanto termina, así que no alarga la prueba. Mensaje de falla con cuántos eventos siguen sin procesar | **Integración** (HTTP + PostgreSQL reales). Medido: **11,3 s** en la suite completa y 17 s con el equipo cargado, frente a 15 s. 3 de 3 en verde; **mutación** a 1 s → falla con «50 de 50 siguen en Recibido tras 1 s (DT-01)» |

## 3. Lo que se descartó (y por qué)

- **El barredor de Ingesta en las pruebas:** cada API de prueba lo arranca y encola todos los pendientes de la base
  compartida. Se sospechó como causa del lote de 50, pero los datos no lo sostienen: en toda la suite quedan pocos
  eventos pendientes (unos 200 enviados en total, casi todos procesados) y la prueba que parecía dejar 600
  («Lluvia de 600») no usa la cola de Ingesta. Se deja como está: cambiarlo sin evidencia no sería un arreglo.
- **Subir el tope sin medir:** el valor de 60 s sale de las mediciones de arriba y de DT-01, no de probar números.

## 4. Lo que sigue abierto

**DT-01** (el procesamiento es más lento que la llegada) es la causa de fondo de que 50 eventos tarden 11 s. Su
arreglo es la parte 4 del plan. Cuando se resuelva, este tope puede volver a bajar.
