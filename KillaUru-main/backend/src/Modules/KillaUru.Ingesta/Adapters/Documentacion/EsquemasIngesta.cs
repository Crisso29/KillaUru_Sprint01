using System.Text.Json.Nodes;
using KillaUru.Ingesta.Adapters.Dtos;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace KillaUru.Ingesta.Adapters.Documentacion;

/// <summary>
/// Completa en el OpenAPI publicado lo que C# no expresa solo: requeridos, límites y catálogos del contrato
/// `specs/005/contracts/ingesta.openapi.yaml`. Solo toca los DTO de Ingesta (FR-001, FR-006).
/// </summary>
internal sealed class EsquemasIngesta : IOpenApiSchemaTransformer
{
    // Tope del contrato (RF-11); IngestaOptions.MaxEventosPorLote puede bajarlo, nunca subirlo.
    private const int MaximoEventos = 50;

    // Motivos que la recepción puede devolver; VelocidadImposible se decide después, en Anomalías.
    private static readonly string[] MotivosDeRecepcion =
    [
        "EsquemaInvalido", "ConfidenceBajo", "PrecisionGpsInsuficiente", "TimestampFueraDeRango",
        "TipoDesconocido", "FueraDelCorredor", "SeudonimoInconsistente",
    ];

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken ct)
    {
        var tipo = context.JsonTypeInfo.Type;
        if (tipo == typeof(LoteDto)) CompletarLote(schema);
        if (tipo == typeof(RespuestaLoteDto)) schema.Required = schema.Properties!.Keys.ToHashSet();
        if (tipo == typeof(RechazoDto)) CompletarRechazo(schema);
        return Task.CompletedTask;
    }

    private static void CompletarLote(OpenApiSchema lote)
    {
        lote.Required = new HashSet<string> { "eventos" };
        lote.Properties!["eventos"] = new OpenApiSchema
        {
            Type = JsonSchemaType.Array, MinItems = 1, MaxItems = MaximoEventos, Items = Evento(),
        };
    }

    private static void CompletarRechazo(OpenApiSchema rechazo)
    {
        rechazo.Required = new HashSet<string> { "id", "indice", "motivo" };
        rechazo.Properties!["indice"] = new OpenApiSchema
        {
            Type = JsonSchemaType.Integer, Minimum = "0", Maximum = (MaximoEventos - 1).ToString(),
        };
        rechazo.Properties["motivo"] = Catalogo(MotivosDeRecepcion);
    }

    /// <summary>
    /// El DTO recibe cada evento como JsonElement para rechazarlo sin romper el lote (D5),
    /// así que su forma se documenta aquí, sin campos requeridos.
    /// </summary>
    private static OpenApiSchema Evento() => new()
    {
        Type = JsonSchemaType.Object,
        Description = "Un campo faltante o de otro tipo rechaza solo este evento con EsquemaInvalido (D5).",
        Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["eventoId"] = Texto("uuid"),
            ["detectadoUtc"] = Texto("date-time"),
            ["latitud"] = Numero(JsonSchemaType.Number, "-90", "90"),
            ["longitud"] = Numero(JsonSchemaType.Number, "-180", "180"),
            ["precisionMetros"] = Numero(JsonSchemaType.Number, "0", null),
            ["tipo"] = Catalogo("Bache", "Hundimiento"),
            ["severidad"] = Numero(JsonSchemaType.Integer, "1", "5"),
            ["confidence"] = Numero(JsonSchemaType.Number, "0", "1"),
            ["modeloVersion"] = Texto(null),
            ["seudonimo"] = Texto("uuid"),
        },
    };

    private static OpenApiSchema Texto(string? formato) => new() { Type = JsonSchemaType.String, Format = formato };

    private static OpenApiSchema Numero(JsonSchemaType tipo, string? minimo, string? maximo) =>
        new() { Type = tipo, Minimum = minimo, Maximum = maximo };

    private static OpenApiSchema Catalogo(params string[] valores) => new()
    {
        Type = JsonSchemaType.String,
        Enum = valores.Select(v => (JsonNode)JsonValue.Create(v)!).ToList(),
    };
}
