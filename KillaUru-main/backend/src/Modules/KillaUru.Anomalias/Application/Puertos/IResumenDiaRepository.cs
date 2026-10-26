using KillaUru.Anomalias.Contracts.Lecturas;
namespace KillaUru.Anomalias.Application.Puertos;
/// <summary>Agregados diarios sin exponer los identificadores contados.</summary>
internal interface IResumenDiaRepository
{
    Task<ResumenDia> ObtenerAsync(DateOnly dia, int tramosTotales, CancellationToken ct);
}
