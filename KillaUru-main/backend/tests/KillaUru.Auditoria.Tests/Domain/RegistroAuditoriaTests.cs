using KillaUru.Auditoria.Domain;
using Microsoft.Extensions.Time.Testing;

namespace KillaUru.Auditoria.Tests.Domain;

/// <summary>Protege el catálogo, la privacidad recursiva y la copia inmutable del contexto.</summary>
public sealed class RegistroAuditoriaTests
{
    [Fact]
    public void Crea_con_hora_inyectada_y_copia_del_contexto()
    {
        var reloj = new FakeTimeProvider();
        var contexto = new Dictionary<string, object?> { ["filas"] = 3 };
        var resultado = RegistroAuditoria.Crear(13, null, null, "exportacion", contexto, reloj.GetUtcNow());
        contexto["filas"] = 9;
        Assert.NotNull(resultado.Registro);
        Assert.Equal(reloj.GetUtcNow(), resultado.Registro.OcurridoUtc);
        Assert.Contains("3", resultado.Registro.Contexto);
    }

    [Theory]
    [InlineData("correo")] [InlineData("EMAIL")] [InlineData("Contrasena")]
    [InlineData("password")] [InlineData("SEUDONIMO")]
    public void Rechaza_claves_incluso_en_objetos_anidados(string clave)
    {
        var contexto = new Dictionary<string, object?> { ["detalle"] = new[] { new Dictionary<string, object?> { [clave] = "secreto" } } };
        var resultado = RegistroAuditoria.Crear(1, null, null, null, contexto, new FakeTimeProvider().GetUtcNow());
        Assert.Null(resultado.Registro);
        Assert.Equal("contexto_no_permitido", resultado.Error);
    }

    [Theory]
    [InlineData(0)] [InlineData(15)]
    public void Rechaza_accion_desconocida(short accion)
    {
        Assert.Equal("accion_desconocida", RegistroAuditoria.Crear(accion, null, null, null,
            new Dictionary<string, object?>(), new FakeTimeProvider().GetUtcNow()).Error);
    }

    [Fact]
    public void Rechaza_objetivo_largo_y_actor_incoherente()
    {
        var ahora = new FakeTimeProvider().GetUtcNow();
        Assert.Null(RegistroAuditoria.Crear(1, null, null, new string('a', 101), new Dictionary<string, object?>(), ahora).Registro);
        Assert.Null(RegistroAuditoria.Crear(1, Guid.NewGuid(), null, null, new Dictionary<string, object?>(), ahora).Registro);
    }
}
