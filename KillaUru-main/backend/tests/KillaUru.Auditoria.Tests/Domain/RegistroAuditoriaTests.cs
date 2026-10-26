using KillaUru.Auditoria.Domain;
using Microsoft.Extensions.Time.Testing;

namespace KillaUru.Auditoria.Tests.Domain;

/// <summary>
/// Protege la fábrica inmutable de <see cref="RegistroAuditoria"/>: la validación ocurre ANTES
/// de persistir, garantizando que ningún hecho inválido llega a la base de datos.
/// Spec 002 · FR-001 · CHK014 · CHK009 · T008, T009, T016.
/// </summary>
public sealed class RegistroAuditoriaTests
{
    /// <summary>
    /// El reloj se inyecta (jamás DateTime.Now) y el contexto se copia en el momento de creación;
    /// cambios posteriores al diccionario no alteran el hecho ya registrado (inmutabilidad, CHK008).
    /// </summary>
    [Fact]
    public void Crea_con_hora_inyectada_y_copia_del_contexto()
    {
        // Arrange
        var reloj = new FakeTimeProvider();
        var contexto = new Dictionary<string, object?> { ["filas"] = 3 };

        // Act
        var resultado = RegistroAuditoria.Crear(13, null, null, "exportacion", contexto, reloj.GetUtcNow());
        contexto["filas"] = 9; // mutación posterior al snapshot

        // Assert — el registro conserva el valor original del contexto
        Assert.NotNull(resultado.Registro);
        Assert.Equal(reloj.GetUtcNow(), resultado.Registro.OcurridoUtc);
        Assert.Contains("3", resultado.Registro.Contexto);
    }

    /// <summary>
    /// Anidar un dato sensible en una lista o en un objeto no evita la regla de privacidad:
    /// <see cref="ClavesProhibidas"/> recorre el JSON completo de forma recursiva (FR-001, CHK009).
    /// </summary>
    [Theory]
    [InlineData("correo")]
    [InlineData("EMAIL")]
    [InlineData("Contrasena")]
    [InlineData("password")]
    [InlineData("SEUDONIMO")]
    public void Rechaza_claves_incluso_en_objetos_anidados(string clave)
    {
        // Arrange — el dato sensible está dentro de un array de objetos
        var contexto = new Dictionary<string, object?>
        {
            ["detalle"] = new[] { new Dictionary<string, object?> { [clave] = "secreto" } }
        };

        // Act
        var resultado = RegistroAuditoria.Crear(1, null, null, null, contexto, new FakeTimeProvider().GetUtcNow());

        // Assert
        Assert.Null(resultado.Registro);
        Assert.Equal("contexto_no_permitido", resultado.Error);
    }

    /// <summary>
    /// Los códigos de acción fuera del catálogo (0 y 15) se rechazan sin intentar persistir (CHK002).
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    public void Rechaza_accion_desconocida(short accion)
    {
        // Act + Assert — la fábrica devuelve error sin crear el registro
        var resultado = RegistroAuditoria.Crear(accion, null, null, null,
            new Dictionary<string, object?>(), new FakeTimeProvider().GetUtcNow());

        Assert.Equal("accion_desconocida", resultado.Error);
    }

    /// <summary>
    /// Un objetivo más largo que 100 caracteres y un actor con solo uno de los dos campos
    /// (id sin rol, o rol sin id) se rechazan: el actor es siempre completo o nulo (Sistema).
    /// </summary>
    [Fact]
    public void Rechaza_objetivo_largo_y_actor_incoherente()
    {
        // Arrange
        var ahora = new FakeTimeProvider().GetUtcNow();
        var objetivoLargo = new string('a', 101);
        var actorSinRol = Guid.NewGuid();

        // Act + Assert
        Assert.Null(RegistroAuditoria.Crear(1, null, null, objetivoLargo,
            new Dictionary<string, object?>(), ahora).Registro);
        Assert.Null(RegistroAuditoria.Crear(1, actorSinRol, null, null,
            new Dictionary<string, object?>(), ahora).Registro);
    }
}
