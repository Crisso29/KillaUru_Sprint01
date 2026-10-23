using System.Text.Json.Nodes;
using KillaUru.Auditoria.Application;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
namespace KillaUru.Auditoria.Adapters.Documentacion;

/// <summary>Documenta los catálogos de filtros y un ejemplo del endpoint sin acoplar el host al negocio.</summary>
internal sealed class OperacionAuditoria : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Description.RelativePath != "api/v1/auditoria") return Task.CompletedTask;
        operation.Description = "Rol: Administrador. Consulta inmutable por filtros; cada acceso queda auditado.";
        operation.Summary = "Consultar el registro de auditoría";
        foreach (var parametro in operation.Parameters?.OfType<OpenApiParameter>() ?? [])
            Completar(parametro);
        operation.Responses!["200"].Content!["application/json"].Example = JsonNode.Parse(
            """{"elementos":[{"id":1042,"ocurridoUtc":"2026-10-20T14:32:05Z","actorId":null,"actorRol":"Sistema","categoria":"Exportacion","accion":"DatasetExportado","objetivo":"exportacion","contexto":{"filas":312}}],"pagina":1,"tamano":50,"total":1}""");
        return Task.CompletedTask;
    }

    private static void Completar(OpenApiParameter parametro)
    {
        parametro.Name = char.ToLowerInvariant(parametro.Name![0]) + parametro.Name[1..];
        if (parametro.Schema is OpenApiSchema opcional) opcional.Type &= ~JsonSchemaType.Null;
        if (parametro.Name == "categoria") parametro.Schema = EsquemasAuditoria.Enumeracion(FiltrosAuditoria.CategoriasPermitidas);
        if (parametro.Name == "accion") parametro.Schema = EsquemasAuditoria.Enumeracion(FiltrosAuditoria.AccionesPermitidas);
        if (parametro.Name is "pagina" or "tamano" && parametro.Schema is OpenApiSchema esquema)
        {
            esquema.Type = JsonSchemaType.Integer;
            esquema.Default = JsonValue.Create(parametro.Name == "pagina" ? 1 : 50);
        }
    }
}
