# Research: Módulo Consulta de Mapa

## R-001 · De `bbox` a celdas: hace falta saber qué celdas toca la vía

- **Problema**: `diseño-interno` §4 dice «bbox → celdas» y «el corredor tiene ~1 000 celdas». Pero el rectángulo que envuelve los 333 km (≈ 2,6° × 1,5°) contiene ~150 000 celdas de 0,005°, casi todas cerro vacío. Sin saber cuáles tocan la vía, cada petición del corredor haría 150 000 búsquedas en la caché.
- **Decision**: `IAnomaliaLectura.CeldasDelCorredor()` (agregado en 004, R-012): celdas que toca la franja de 50 m alrededor de la polilínea. `CorredorMapa` las pide una vez (perezoso, cuando dejan de estar vacías) y guarda también la clasificación de la zona demo. Celdas de una petición = celdas del corredor cuyo rectángulo intersecta el `bbox`.
- **Rationale**: toda anomalía nace a ≤ 50 m de la vía (RN-13), así que su celda siempre está en ese conjunto. Mapa no copia la polilínea (un dato, un dueño).
- **Confirmado (D1)**: la operación pasa a docs (`componentes-arquitectonicos.md` §3.1, Nivel3 §2.2, `diseño-interno` §4).

## R-002 · Tamaño de celda fijo y `Celda.Limites()`

- **Decision**: la celda es la de `Shared` (0,005°). Se agrega `Celda.Limites()` en Shared (001) para obtener su rectángulo sin repetir la fórmula. `MapaOptions` no tiene «tamaño de celda» aunque `enfoque` §5 lo lista.
- **Rationale**: `AnomaliaCambiada` trae la celda que calculó Anomalías y que está guardada en `anomalia.celda`; si Mapa usara otra rejilla, invalidaría la celda equivocada.
- **Confirmado (D1)**: `enfoque-arquitectonico.md` §5 quita «tamaño de celda» de `MapaOptions`; `Celda.Limites()` pasa a `diseño-interno` §6.1.

## R-003 · `bbox` opcional y validación

- **Decision**: formato `minLon,minLat,maxLon,maxLat` (orden de GeoJSON). 400 `datos_invalidos` si no son 4 números, si `min > max` o si sale de [−180, 180] / [−90, 90]. Sin `bbox` = todo el alcance del rol (la app lo usa para la primera descarga offline).
- **Rationale**: RF-14 usa `?bbox=`; hacerlo opcional evita que la app tenga que conocer el rectángulo del corredor.

## R-004 · Estados por defecto del administrador

- **Decision** (D6): sin `estado`, el administrador recibe **los 5 estados** (RN-08: «todos los estados, con filtros»). Con `estado=…` recibe solo los pedidos. `MapaOptions` ya no tiene `EstadosAdministradorPorDefecto`.
- **Rationale**: gana docs. Para no llenar el mapa con los estados finales que se acumulan 2 años (RN-10), **el panel abre con «Activa + PendienteConfirmacion» marcados** y los manda en `estado`: es una regla del cliente (nota para 010-panel-admin y el prototipo), no de la API.
- **Alternativa rechazada**: el valor por defecto en el servidor (versión anterior de esta decisión), porque contradecía RN-08.

## R-005 · Filtros de conductores y anónimos

- **Decision**: `tipo`, `severidad`, `desde` y `hasta` se permiten a todos, siempre dentro del alcance del rol: `FiltrarAsync` con `Estados = [Activa]` y el `bbox` recortado; para el anónimo, además `ZonaDemo.Contiene` punto por punto. La respuesta anónima filtrada también es `public, max-age=60` (los filtros forman parte de la URL). `estado` de un no administrador → 403 `sin_permiso`, con o sin token (regla única T-SEG20: sin token en un endpoint que exige sesión → 401; con token y rol insuficiente → 403; parámetro reservado usado por otro rol → 403). Un token inválido o vencido sigue dando 401 (001).
- **Fuente**: `diseño-interno` §4, fila «Filtros».

## R-006 · Forma de `updatedSince`

- **Decision**:
  - Respuesta: la misma `FeatureCollection`; una baja es una *feature* con `geometry: null` y `properties: {id, eliminada: true}` (GeoJSON admite geometría nula). Nunca lleva estado ni posición (RF-14: «sin revelar su estado interno»).
  - Visible = `Activa` y, para el anónimo, dentro del polígono. Un cambio no visible → baja; para el anónimo, todo cambio fuera del demo se omite, sea alta o baja (R-012).
  - Administrador con `updatedSince`: todas las cambiadas como altas con su `estado`, sin bajas.
  - `generadoUtc` en la raíz de toda respuesta: la app lo guarda y lo manda como próximo `updatedSince`; así no depende del reloj del celular (RF-03 desconfía de él).
  - `updatedSince` con filtros → 400 `parametros_incompatibles`; en el futuro (> 5 min) → 400 `datos_invalidos`.
- **Rationale**: un solo formato para la app; las bajas no filtran información a un rol que no debe verla.

## R-007 · Aciertos en el borde (RNF-01: ≥ 80 % anónimo en el demo)

- **Decision**: Cloudflare cachea por URL exacta. Para que las URL se repitan, la app anónima pide **sin `bbox`** (todo el demo, ~65 km, pocos KB) y refresca la copia completa en lugar de usar `updatedSince` (cuyo valor cambia en cada petición). El conductor registrado sí usa `updatedSince` (no pasa por el borde).
- **Confirmado (D17)**: es una regla de la app; se traslada a la futura spec **008-app-android**. Esta spec solo la supone (Assumptions).

## R-008 · `ETag` y 304

- **Decision**: `ETag` fuerte = `"` + base64url(SHA-256(instante de arranque ‖ alcance ‖ (celda, versión)… en orden ascendente))[0..22] + `"`. `If-None-Match` igual → 304 con el mismo `ETag` y las mismas cabeceras de caché. Solo en el camino por celdas.
- **Rationale**: se calcula con datos en memoria; Cloudflare también revalida con él las respuestas anónimas.

## R-009 · Armar la respuesta sin volver a serializar

- **Decision**: `CeldaGeoJson` guarda las *features* de la celda ya en UTF-8, separadas por comas. El controlador escribe en el `PipeWriter`: encabezado (`{"type":"FeatureCollection","generadoUtc":"…","features":[`), los bloques de cada celda no vacía separados por comas, y el cierre `]}`. La compresión la hace Cloudflare (Brotli) en el borde, no el origen.
- **Rationale**: `diseño-interno` §4, fila «Valor»; ahorra CPU en la VM ARM gratuita.

## R-010 · Polígono del tramo demo

- **Decision**: el polígono se arma una vez a partir de la polilínea de la PE-28A entre el km 268 (Vinchos) y el km 333 (Ayacucho), con un margen de 200 m a cada lado, simplificado a ≤ 60 vértices, y se guarda en `appsettings.json` (`Mapa:PoligonoDemo`) y como GeoJSON en `db/datos/tramo_demo.geojson`, con la atribución de OpenStreetMap. Ese archivo es la fuente única que reutilizará la app para sombrear (RF-08; D16, se publica según la spec 008).
- **Confirmado (D1)**: km 268 → 333, margen 200 m, ≤ 60 vértices; pasa a RN-08 y al glosario (`PoligonoDemo`).

## R-011 · `kmVia` en cada *feature* (D13)

- **Decision**: toda *feature* lleva `kmVia` (número, km sobre la vía), para los tres roles. Mapa lo copia de `AnomaliaVisible.KmVia` / `AnomaliaCambio.KmVia` (004), que sale de la columna `anomalia.km_via`, calculada **al nacer** con `CorredorVia.KmSobreVia` (la posición es fija, RN-05). Las bajas no lo llevan.
- **Rationale**: el prototipo lo muestra en la lista del mapa del panel, en la franja del corredor y en el hito del detalle de la app (PO-09); calcularlo en el cliente obligaría a cada cliente a tener la polilínea con el mismo algoritmo.
- **Fuente**: D13.

## R-012 · Bajas del anónimo solo dentro del demo (T-SEG16)

- **Decision**: con `updatedSince`, al anónimo solo se le devuelven bajas de anomalías cuya posición está dentro del polígono demo (`ZonaDemo.Contiene`); las de fuera se omiten, en cualquier estado.
- **Rationale**: R-07 limita la exposición pública al tramo demo; devolver bajas de fuera filtraba ids y el ritmo de cambios de todo el corredor (SEG-16). Una baja de dentro que el anónimo nunca vio es inofensiva: la app la ignora.

## R-013 · Métrica de aciertos de la caché (T-OPS15)

- **Decision**: contador `killauru.mapa.cache{resultado}` con `resultado` ∈ {`acierto`, `fallo`, `no_modificado`}, en un medidor propio del módulo (`KillaUru.Mapa`, `Infrastructure/MetricasMapa.cs`). `acierto`/`fallo` los cuenta `FuenteCeldasCacheada` (una vez por celda); `no_modificado`, `ConsultarPorCeldasUseCase` al responder 304. La tasa de aciertos en el borde (SC-002) se lee en las analíticas de Cloudflare y se anota en `deploy/runbooks/monitoreo.md`.
- **Rationale**: `diseño-interno` §8 mide RNF-01 por acierto, 304 y fallo; 3 valores fijos mantienen baja la cardinalidad en el plan gratuito de Grafana (RNF-32).

## Confirmados por el autor el 2026-10-21

1. R-001 `CeldasDelCorredor` en `IAnomaliaLectura` (y R-012 de 004) — D1.
2. R-002 sin «tamaño de celda» en `MapaOptions`; `Celda.Limites()` en Shared — D1.
3. R-004 sin `estado`, el administrador recibe los 5 estados; el panel abre con «Activa + PendienteConfirmacion» — D6.
4. R-007 la app anónima pide sin `bbox` y sin `updatedSince`: regla de la spec 008-app-android — D17.
5. R-010 km 268 → 333, margen 200 m, ≤ 60 vértices — D1 (fuente única para la app: D16).
6. R-011 `kmVia` en cada *feature* — D13.
