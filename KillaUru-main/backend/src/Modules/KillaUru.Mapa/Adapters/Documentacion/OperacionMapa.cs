using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace KillaUru.Mapa.Adapters.Documentacion;

/// <summary>
/// Completa `consultarMapa` en el OpenAPI publicado: esquemas GeoJSON, cabeceras de caché, 401 y el catálogo de
/// `tipo`. Solo toca esta operación (contrato `specs/006/contracts/mapa.openapi.yaml`, FR-001, ADR-006).
/// </summary>
internal sealed class OperacionMapa : IOpenApiOperationTransformer
{
    internal const string OperationId = "consultarMapa";

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken ct)
    {
        if (operation.OperationId != OperationId) return Task.CompletedTask;
        var documento = context.Document!;
        RegistrarEsquemas(documento);
        DescribirRespuestas(operation, documento);
        foreach (var parametro in operation.Parameters?.OfType<OpenApiParameter>() ?? [])
            CompletarParametro(parametro);
        return Task.CompletedTask;
    }

    private static void RegistrarEsquemas(OpenApiDocument documento)
    {
        var esquemas = documento.Components!.Schemas!;
        esquemas["ColeccionAnomalias"] = EsquemasMapa.Coleccion(documento);
        esquemas["AnomaliaFeature"] = EsquemasMapa.Anomalia();
        esquemas["BajaFeature"] = EsquemasMapa.Baja();
    }

    private static void DescribirRespuestas(OpenApiOperation operacion, OpenApiDocument documento)
    {
        var respuestas = operacion.Responses!;
        respuestas["200"] = ConContenido(documento);
        respuestas["304"] = new OpenApiResponse
        {
            Description = "Ninguna celda pedida cambió desde el `ETag` enviado.",
            Headers = new Dictionary<string, IOpenApiHeader> { ["ETag"] = Cabecera(null) },
        };
        // Es pública, pero un token inválido o vencido se rechaza en vez de tratarse como anónimo (T-SEG20).
        respuestas.TryAdd("401", new OpenApiResponse { Description = "Token inválido o vencido (`token_invalido`)." });
    }

    private static OpenApiResponse ConContenido(OpenApiDocument documento) => new()
    {
        Description = "Anomalías visibles para el rol.",
        Headers = new Dictionary<string, IOpenApiHeader>
        {
            ["ETag"] = Cabecera("Solo sin filtros ni `updatedSince`."),
            ["Cache-Control"] = Cabecera("`public, max-age=60` sin token; `private, no-store` con token."),
        },
        Content = new Dictionary<string, OpenApiMediaType>
        {
            ["application/geo+json"] = new() { Schema = new OpenApiSchemaReference("ColeccionAnomalias", documento) },
        },
    };

    private static void CompletarParametro(OpenApiParameter parametro)
    {
        if (parametro.Name == "tipo") parametro.Schema = EsquemasMapa.Tipos();
        if (parametro.Name == "severidad") parametro.Description = "Lista separada por comas (severidad mostrada), p. ej. `4,5`.";
        if (parametro.Name == "estado")
            parametro.Description = "Solo administrador (otro rol → 403). Lista separada por comas; sin `estado`, los 5 estados.";
    }

    private static OpenApiHeader Cabecera(string? descripcion) =>
        new() { Description = descripcion, Schema = new OpenApiSchema { Type = JsonSchemaType.String } };
}
