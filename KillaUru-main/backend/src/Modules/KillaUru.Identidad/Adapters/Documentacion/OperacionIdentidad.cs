using System.Text.Json.Nodes;
using KillaUru.Identidad.Adapters.Dtos;
using KillaUru.Identidad.Application.Resultados;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
namespace KillaUru.Identidad.Adapters.Documentacion;

/// <summary>Documenta variantes app/panel y filtros CSV sin alterar los contratos de otros módulos.</summary>
internal sealed class OperacionIdentidad : IOpenApiOperationTransformer
{
    public async Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken ct)
    {
        if (context.Description.ActionDescriptor is not Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor accion
            || accion.ControllerTypeInfo.Assembly != typeof(OperacionIdentidad).Assembly) { return; }
        foreach (var parametro in operation.Parameters?.OfType<OpenApiParameter>() ?? []) CompletarParametro(parametro);
        if (operation.OperationId is "iniciarSesion" or "renovarSesion") await VariantesSesionAsync(operation, context, ct);
        if (operation.OperationId == "iniciarSesion") Bloqueo(operation);
        if (operation.OperationId is "eliminarMiCuenta" or "aceptarVersion")
            operation.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("tokenAcceso", context.Document)] = [] }];
        EjemplosIdentidad.Completar(operation);
    }

    private static async Task VariantesSesionAsync(OpenApiOperation operacion, OpenApiOperationTransformerContext contexto, CancellationToken ct)
    {
        var app = await contexto.GetOrCreateSchemaAsync(typeof(SesionEmitida), cancellationToken: ct);
        var panel = await contexto.GetOrCreateSchemaAsync(typeof(SesionPanel), cancellationToken: ct);
        contexto.Document!.Components!.Schemas!["SesionPanel"] = panel;
        foreach (var contenido in operacion.Responses!["200"].Content!.Values)
            contenido.Schema = new OpenApiSchema { OneOf = [app, new OpenApiSchemaReference("SesionPanel", contexto.Document)] };
    }

    private static void CompletarParametro(OpenApiParameter parametro)
    {
        parametro.Name = char.ToLowerInvariant(parametro.Name![0]) + parametro.Name[1..];
        if (parametro.Schema is OpenApiSchema opcional) opcional.Type &= ~JsonSchemaType.Null;
        if (parametro.Name == "rol") parametro.Schema = EsquemasIdentidad.Enum("ConductorRegistrado", "Administrador");
        if (parametro.Name == "estado") parametro.Schema = EsquemasIdentidad.Enum("Activa", "Desactivada", "InvitacionPendiente");
        if (parametro.Name == "ids")
        {
            parametro.Style = ParameterStyle.Form; parametro.Explode = false;
            parametro.Schema = new OpenApiSchema { Type = JsonSchemaType.Array, MaxItems = 200,
                Items = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" } };
        }
        if (parametro.Name is "pagina" or "tamano" && parametro.Schema is OpenApiSchema esquema)
        { esquema.Type = JsonSchemaType.Integer; esquema.Default = JsonValue.Create(parametro.Name == "pagina" ? 1 : 50); }
    }

    private static void Bloqueo(OpenApiOperation operacion)
    {
        var respuesta = (OpenApiResponse)operacion.Responses!["423"];
        respuesta.Headers = new Dictionary<string, IOpenApiHeader>
        { ["Retry-After"] = new OpenApiHeader { Description = "Segundos hasta el desbloqueo.", Schema = new OpenApiSchema { Type = JsonSchemaType.Integer } } };
        respuesta.Content!["application/problem+json"] = new OpenApiMediaType { Schema = new OpenApiSchema
        {
            AllOf = [new OpenApiSchemaReference("ProblemDetails"), new OpenApiSchema { Type = JsonSchemaType.Object,
                Required = new HashSet<string> { "bloqueadoHastaUtc" }, Properties = new Dictionary<string, IOpenApiSchema>
                { ["bloqueadoHastaUtc"] = new OpenApiSchema { Type = JsonSchemaType.String, Format = "date-time" } } }],
        } };
    }
}
