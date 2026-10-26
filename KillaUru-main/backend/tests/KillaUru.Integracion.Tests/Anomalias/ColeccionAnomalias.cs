namespace KillaUru.Integracion.Tests.Anomalias;
/// <summary>Aísla las purgas globales; cada escenario de concurrencia usa trabajadores explícitos en paralelo.</summary>
[CollectionDefinition("Anomalias", DisableParallelization = true)]
public sealed class ColeccionAnomalias;
