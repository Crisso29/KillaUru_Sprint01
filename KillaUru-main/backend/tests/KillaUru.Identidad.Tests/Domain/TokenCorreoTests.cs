using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Tests.Domain;

/// <summary>
/// Enlaces enviados por correo: un solo uso, vigencia exacta según su finalidad y solo el hash guardado.
/// Spec 003 · FR-021 (verificación 24 h), FR-022 (recuperación 30 min), FR-024, FR-031 (invitación 24 h).
/// </summary>
public sealed class TokenCorreoTests
{
    /// <summary>El enlace vale hasta un segundo antes de su vencimiento y nunca en el instante exacto.</summary>
    [Theory]
    [InlineData((short)PropositoToken.Verificacion, 1440)]
    [InlineData((short)PropositoToken.Recuperacion, 30)]
    [InlineData((short)PropositoToken.Invitacion, 1440)]
    public void Vence_exactamente_al_terminar_su_vigencia(short codigoProposito, int minutosVigencia)
    {
        // Arrange
        var proposito = (PropositoToken)codigoProposito;
        var token = TokenCorreo.Crear([1], Guid.NewGuid(), proposito, Instantes.Ahora, TimeSpan.FromMinutes(minutosVigencia));

        // Act
        var valeUnSegundoAntes = token.PuedeUsarse(token.ExpiraUtc.AddSeconds(-1));
        var valeAlVencer = token.PuedeUsarse(token.ExpiraUtc);

        // Assert
        Assert.Equal(proposito, token.Proposito);
        Assert.True(valeUnSegundoAntes);
        Assert.False(valeAlVencer);
    }

    /// <summary>Un enlace usado no se puede volver a usar y conserva la hora del primer uso.</summary>
    [Fact]
    public void Es_de_un_solo_uso()
    {
        // Arrange
        var token = TokenCorreo.Crear([1], Guid.NewGuid(), PropositoToken.Recuperacion, Instantes.Ahora, TimeSpan.FromMinutes(30));

        // Act
        token.Usar(Instantes.Ahora);
        token.Usar(Instantes.Ahora.AddMinutes(1));

        // Assert
        Assert.Equal(Instantes.Ahora, token.UsadoUtc);
        Assert.False(token.PuedeUsarse(Instantes.Ahora));
    }

    /// <summary>Se copia el hash recibido para que nadie pueda alterarlo desde fuera (FR-024).</summary>
    [Fact]
    public void Copia_el_hash_y_el_usuario()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        byte[] hash = [1, 2, 3];

        // Act
        var token = TokenCorreo.Crear(hash, usuarioId, PropositoToken.Invitacion, Instantes.Ahora, TimeSpan.FromHours(24));
        hash[0] = 9;

        // Assert
        Assert.Equal(usuarioId, token.UsuarioId);
        Assert.Equal([1, 2, 3], token.HashToken);
    }
}
