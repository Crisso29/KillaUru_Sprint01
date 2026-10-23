using NetArchTest.Rules;

namespace KillaUru.Arquitectura.Tests;

/// <summary>Convierte el resultado de NetArchTest en una aserción con los tipos que violan la regla.</summary>
internal static class Regla
{
    internal static void Cumple(ConditionList condicion, string descripcion)
    {
        var resultado = condicion.GetResult();
        var infractores = string.Join(", ", resultado.FailingTypeNames ?? []);
        Assert.True(resultado.IsSuccessful, $"{descripcion}. Tipos que la violan: {infractores}");
    }
}
