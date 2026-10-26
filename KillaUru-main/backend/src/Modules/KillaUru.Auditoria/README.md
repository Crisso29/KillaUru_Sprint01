# Auditoría

Registra hechos sensibles de forma permanente y permite consultarlos únicamente al administrador.
Depende de Shared; no conoce Identidad ni Anomalías. Sus únicos tipos públicos son Contracts y AuditoriaModule.

## Organización y decisiones

- Domain valida catálogo, actor, objetivo y contexto. Copia el contexto como JSON inmutable y rechaza claves personales en objetos y listas.
- Application coordina escrituras, lecturas y consulta. Resultados inválidos de consulta producen 400; un registro interno inválido es un error de programación.
- Infrastructure implementa puertos separados de lectura y escritura (ISP), con EF Core y el esquema auditoria.
- Adapters traduce HTTP y documenta el contrato; no expone entidades ni consulta la base directamente.

Patrones usados: fábrica con resultado de validación, repositorio específico append-only, servicio de consulta y raíz de composición.
No hay repositorio genérico ni intermediario de mensajería. Cada archivo tiene un propósito y comentarios sobre decisiones o restricciones.

## Catálogo estable

| Código | Acción | Categoría |
|---|---|---|
| 1 | UsuarioCreado | Usuarios |
| 2 | RolCambiado | Usuarios |
| 3 | CuentaDesactivada | Usuarios |
| 4 | CuentaActivada | Usuarios |
| 5 | CuentaEliminada | CuentaDelTitular |
| 6 | InicioSesionFallido | Sesion |
| 7 | CuentaBloqueada | Sesion |
| 8 | InicioSesionAdministrador | Sesion |
| 9 | RecuperacionSolicitada | CuentaDelTitular |
| 10 | ContrasenaCambiada | CuentaDelTitular |
| 11 | RevisionPrivilegios | Usuarios |
| 12 | AnomaliaModerada | Moderacion |
| 13 | DatasetExportado | Exportacion |
| 14 | AccesoAdministrativoDatos | AccesoADatos |

Solo 2, 7 y 13 generan métricas de riesgo, después de confirmar. Los códigos nunca se renumeran.

## Auditar desde otro módulo

1. Abrir la transacción propia y unir conexión y transacción a IContextoTransaccion.
2. Ejecutar la escritura de negocio.
3. Llamar IAuditoriaService.RegistrarAsync con id interno del actor y contexto sin datos personales.
4. Confirmar con IContextoTransaccion.ConfirmarAsync; ante un error anterior al commit, revertir con RevertirAsync.
5. Publicar eventos después del commit. El propietario desecha conexión y transacción.

RegistrarAsync sin transacción falla. La auditoría no confirma por el llamador.
RegistrarLecturaAsync solo admite AccesoAdministrativoDatos o DatasetExportado: abre y confirma su propia transacción.
Para exportar, contar filas, auditar y recién emitir el archivo. Mapa y Dashboard no auditan consultas.

El contrato recibe un diccionario; su contenido se copia al crear el registro. No incluir datos personales
bajo otras claves o dentro de textos libres: el filtro de claves no puede deducir el significado de cada valor.

## Endpoint

`GET /api/v1/auditoria`, política SoloAdministrador. Filtros: desde/hasta inclusivos, categoria, accion, actorId, pagina y tamano (1–200).
Orden: ocurridoUtc e id descendentes. COUNT y página comparten una instantánea; el propio acceso se registra después y aparece en la siguiente consulta.
No existen PUT, PATCH ni DELETE. PostgreSQL impide UPDATE, DELETE y TRUNCATE con killauru_app.

```powershell
curl.exe -s "http://localhost:8080/api/v1/auditoria?categoria=Sesion&tamano=20" -H "Authorization: Bearer $env:KU_TOKEN_ADMIN"
```

Respuestas: 200, 400 datos_invalidos, 401 token_invalido, 403 sin_permiso y 429 limite_excedido con Retry-After.
Toda respuesta autenticada usa private, no-store. El contrato está en specs/002-modulo-auditoria/contracts/auditoria.openapi.yaml.

## Verificación desde backend

```powershell
dotnet build KillaUru.sln
dotnet test --project tests/KillaUru.Auditoria.Tests --no-build -- --coverage --coverage-output-format cobertura
dotnet test --project tests/KillaUru.Integracion.Tests --no-build -- --filter-class '*Auditoria*'
dotnet test --project tests/KillaUru.Arquitectura.Tests --no-build
```

Docker debe estar disponible. Las pruebas aplican V0000/V0001/V0002 con los roles reales en una base temporal.
RendimientoConsultaTests mide 30 consultas HTTP tras calentamiento y escribe TestResults/auditoria-rendimiento.json.

## Operación pendiente

Responsable de incidentes: CRISOLOGO; reemplazo: CRISSO. Procedimiento en deploy/runbooks/incidentes-datos-personales.md.
Faltan activar y verificar las alertas en Grafana y ejecutar el simulacro EQ-13 en killauru-pruebas.
La revisión de emisores en 003/004 se repite cuando esos módulos tengan implementación.
