using KillaUru.Anomalias.Domain;
namespace KillaUru.Anomalias.Application.Procesamiento;
/// <summary>Publica una instantánea inmutable; la recepción nunca observa una carga parcial.</summary>
internal sealed class EstadoCorredor
{
    private CorredorVia? actual;
    internal CorredorVia? Actual => Volatile.Read(ref actual);
    internal void Cargar(CorredorVia corredor) => Volatile.Write(ref actual, corredor);
}
