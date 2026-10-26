using System.Text.Json.Serialization;
namespace KillaUru.Anomalias.Application.Administracion;
/// <summary>Acepta únicamente los nombres del contrato; los números no son acciones HTTP válidas.</summary>
internal sealed class ConversorAccionModeracion : JsonStringEnumConverter<AccionModeracion>
{
    public ConversorAccionModeracion() : base(namingPolicy: null, allowIntegerValues: false) { }
}
