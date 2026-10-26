using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Tests.Domain;

public sealed class TokenCorreoTests
{
    [Theory]
    [InlineData(1, 1440)]
    [InlineData(2, 30)]
    [InlineData(3, 1440)]
    public void LimiteExactoYUsoUnico(int proposito, int minutos)
    {
        var ahora = new DateTimeOffset(2026, 10, 23, 10, 0, 0, TimeSpan.Zero);
        var token = TokenCorreo.Crear([1], Guid.NewGuid(), (PropositoToken)proposito, ahora, TimeSpan.FromMinutes(minutos));
        Assert.True(token.PuedeUsarse(token.ExpiraUtc.AddSeconds(-1)));
        Assert.False(token.PuedeUsarse(token.ExpiraUtc));
        token.Usar(ahora);
        Assert.False(token.PuedeUsarse(ahora));
    }
}
