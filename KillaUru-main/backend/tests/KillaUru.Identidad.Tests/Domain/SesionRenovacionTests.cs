using KillaUru.Identidad.Domain;

namespace KillaUru.Identidad.Tests.Domain;

/// <summary>
/// Una generación de la familia rotativa del token de actualización: guarda solo el hash,
/// tolera reintentos durante la gracia y se invalida al revocarse, vencer o desactivarse la cuenta.
/// Spec 003 · FR-015, FR-016, FR-017, FR-018, FR-039.
/// </summary>
public sealed class SesionRenovacionTests
{
    private const int GraciaSegundos = 60;

    private static SesionRenovacion SesionDeConductor() =>
        SesionRenovacion.Crear(Guid.NewGuid(), Guid.NewGuid(), [1], Instantes.Ahora, Instantes.Ahora.AddDays(30), null);

    /// <summary>
    /// Se copia el hash: si quien lo creó reutiliza su arreglo, la sesión no cambia (FR-015).
    /// Las horas se guardan truncadas para no correlacionar sesiones con eventos (FR-039).
    /// </summary>
    [Fact]
    public void Crear_copia_el_hash_y_trunca_las_horas()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        byte[] hash = [1, 2, 3];
        var limiteAbsoluto = Instantes.Ahora.AddHours(1);

        // Act
        var sesion = SesionRenovacion.Crear(usuarioId, usuarioId, hash, Instantes.Ahora, Instantes.Ahora.AddDays(1), limiteAbsoluto);
        hash[0] = 9;

        // Assert
        Assert.Equal(1, sesion.HashToken[0]);
        Assert.NotEqual(Guid.Empty, sesion.Id);
        Assert.Equal(usuarioId, sesion.UsuarioId);
        Assert.Equal(usuarioId, sesion.FamiliaId);
        Assert.Equal(HorasGuardadas.ALaHora(Instantes.Ahora), sesion.CreadoUtc);
        Assert.Equal(limiteAbsoluto, sesion.LimiteAbsolutoUtc);
    }

    /// <summary>
    /// Un token ya usado vale hasta 60 s inclusive (reintentos por red); después se trata como robo (FR-016).
    /// </summary>
    [Theory]
    [InlineData(59, true)]
    [InlineData(60, true)]
    [InlineData(61, false)]
    public void Gracia_de_reuso_es_inclusiva(int segundosDespues, bool sigueValida)
    {
        // Arrange
        var sesion = SesionDeConductor();
        sesion.Usar(Instantes.Ahora);

        // Act
        var error = sesion.Validar(Instantes.Ahora.AddSeconds(segundosDespues), cuentaActiva: true, GraciaSegundos);

        // Assert
        Assert.Equal(Instantes.Ahora, sesion.UsadoUtc);
        Assert.Equal(sigueValida ? null : ErroresSesion.SesionInvalida, error);
    }

    /// <summary>Cada renovación revisa que la cuenta siga activa (FR-017).</summary>
    [Fact]
    public void Cuenta_desactivada_se_informa_aparte()
    {
        // Arrange
        var sesion = SesionDeConductor();

        // Act
        var error = sesion.Validar(Instantes.Ahora, cuentaActiva: false, GraciaSegundos);

        // Assert
        Assert.Equal(ErroresSesion.CuentaDesactivada, error);
    }

    /// <summary>El vencimiento es exclusivo: en el instante exacto ya no sirve (FR-015).</summary>
    [Fact]
    public void Sesion_vencida_es_invalida()
    {
        // Arrange
        var sesion = SesionDeConductor();

        // Act
        var error = sesion.Validar(sesion.ExpiraUtc, cuentaActiva: true, GraciaSegundos);

        // Assert
        Assert.Equal(ErroresSesion.SesionInvalida, error);
    }

    /// <summary>El administrador no pasa de 8 h aunque renueve (límite absoluto, FR-015).</summary>
    [Fact]
    public void Limite_absoluto_alcanzado_es_invalido()
    {
        // Arrange
        var limite = Instantes.Ahora.AddHours(1);
        var sesion = SesionRenovacion.Crear(Guid.NewGuid(), Guid.NewGuid(), [1], Instantes.Ahora, Instantes.Ahora.AddDays(1), limite);

        // Act
        var error = sesion.Validar(limite, cuentaActiva: true, GraciaSegundos);

        // Assert
        Assert.Equal(ErroresSesion.SesionInvalida, error);
    }

    /// <summary>Cerrar sesión revoca la familia; la hora de revocación se trunca (FR-018, FR-039).</summary>
    [Fact]
    public void Revocar_invalida_la_sesion()
    {
        // Arrange
        var sesion = SesionDeConductor();

        // Act
        sesion.Revocar(Instantes.Ahora);

        // Assert
        Assert.Equal(HorasGuardadas.ALaHora(Instantes.Ahora), sesion.RevocadoUtc);
        Assert.Equal(ErroresSesion.SesionInvalida, sesion.Validar(Instantes.Ahora, cuentaActiva: true, GraciaSegundos));
    }
}
