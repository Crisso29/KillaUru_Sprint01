using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Tests.Domain;

public sealed class PoliticaSesionTests
{
    [Fact]
    public void ConductorDeslizaYAdministradorRespetaLimite()
    {
        var ahora = new DateTimeOffset(2026, 10, 23, 10, 17, 43, TimeSpan.Zero);
        Assert.Equal(HorasGuardadas.ALaHora(ahora.AddDays(30)), PoliticaSesion.Vencimiento(ahora, null, 30, 8));
        var limite = ahora.AddHours(3);
        Assert.Equal(HorasGuardadas.ALaHora(limite), PoliticaSesion.Vencimiento(ahora, limite, 30, 8));
    }
}
