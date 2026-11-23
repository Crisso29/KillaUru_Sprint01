# Colección de Postman de la API (parte 8 del sprint 2)

> 2026-11-23, contra `killauru-local` (Docker, `ASPNETCORE_ENVIRONMENT=Pruebas`). [Volver a evidencias](../../README.md) ·
> [Colección y cómo usarla](../../../../../backend/tests/postman/README.md)

## 1. Resultado

| Medida | Resultado | Archivo |
|---|---|---|
| Newman (colección completa) | **62 peticiones, 281 comprobaciones, 0 fallas**; media 42 ms, máximo 459 ms | [informe HTML](informe-newman.html) · [salida](salida-newman.txt) |
| Cobertura del contrato | las **32 operaciones** de los contratos 001–007; los 16 endpoints de administrador con 401 y 403 | `test_coleccion.py` (6 pruebas, en CI) |
| Backend completo después de los arreglos | **1 167 pruebas: 1 164 aprobadas, 0 fallidas, 3 omitidas**; dominio 97–100 % | [RESUMEN-backend.md](RESUMEN-backend.md) |

El informe HTML se generó con `--reporter-htmlextra-skipSensitiveData`: no muestra cabeceras ni cuerpos (tokens y la
contraseña de la cuenta de prueba). La cuenta se crea y se **borra** en la misma corrida.

## 2. Cómo está hecha (para que no se desincronice)

- La colección **se genera** con `generar_coleccion.py` desde escenarios en Python; no se edita a mano.
- Los 32 endpoints salen de leer los **contratos OpenAPI**; los 32 casos 401/403 del panel se generan de ellos.
- `test_coleccion.py` (prueba de contrato, en CI) falla si falta una operación, si se inventa una ruta, si una
  petición no comprueba su estado, si el archivo no es el que produce el generador o si guarda un secreto.
  Se comprobó con **mutaciones**: quitar una petición y cambiar una ruta hicieron fallar 3 pruebas cada una.
- Reglas transversales en **un solo lugar** (script de colección): HSTS y `nosniff`, errores `problem+json` con
  `codigo` y `correlacionId`, y menos de 2 s por petición.
- Alcance acordado: sin entrar como administrador (el captcha Turnstile no se simula en local; decisión del autor).

## 3. Defectos de la API que encontró (todos corregidos con TDD)

| Defecto | Efecto | Prueba que lo reprodujo (rojo) → arreglo |
|---|---|---|
| El mapa devolvía **JSON inválido** (`},,{`) cuando una celda tenía 2 o más anomalías | La app y el panel no podían leer el mapa | `SerializadorGeoJsonTests` (2 de 3 en rojo). `Utf8JsonWriter` ponía su propia coma además de la manual; se reinicia el escritor tras cada anomalía |
| Los errores del **mapa** y de la **exportación** salían como `application/geo+json` | Violaba el contrato y la constitución IX: el cliente no reconocía el error | `ErroresMapaApiTests` y `ExportacionApiTests` (3 en rojo). `[Produces]` imponía el tipo a toda respuesta; se cambió por `[ProducesResponseType(…, 200, tipo)]`, que solo documenta el 200. Las pruebas de contrato OpenAPI siguen en verde |
| El correo **reintentaba errores 4xx** definitivos (clave inválida) | El registro tardaba 4,7 s y el reenvío 2,3 s | `ServiciosExternosTests` (3 en rojo). Solo se reintentan red, 408, 429 y 5xx (R-007: «falla del proveedor»). Registro: de 4,7 s a < 0,5 s |
| La colección esperaba `datos_invalidos` para un lote vacío | Error de la colección, no de la API: el contrato dice `lote_invalido` | Se corrigió la colección |

Además, el CI tenía 3 bytes en Latin-1 en `ci.yml` (dos comentarios); se pasaron a UTF-8 al agregar el paso nuevo.

## 4. Para decidir (no se cambió)

R-007 dice que el correo por consola es para «desarrollo **y pruebas**», pero la API solo lo usa en `Development`;
en `killauru-local` y en la VM de pruebas intenta enviar por Resend. Cambiarlo tiene una consecuencia de seguridad:
la consola imprime los enlaces, que son credenciales. Queda para decisión del autor.

## 5. Repetir

```powershell
# desde backend/deploy: levantar la API local (ver deploy/local/README.md)
docker compose --env-file .env.local -f docker-compose.local.yml up -d --build
# desde backend/tests/postman
npx --yes newman@6.2.1 run KillaUru.postman_collection.json -e local.postman_environment.json
python -m unittest -v
```
