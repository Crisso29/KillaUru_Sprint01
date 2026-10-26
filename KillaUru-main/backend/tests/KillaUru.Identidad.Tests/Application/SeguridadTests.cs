using KillaUru.Identidad.Application;
using KillaUru.Identidad.Infrastructure.Seguridad;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
namespace KillaUru.Identidad.Tests.Application;

public sealed class SeguridadTests
{
    [Fact]
    public void Secretos_son_aleatorios_hash_y_derivacion_depende_de_la_clave()
    {
        var a = new GeneradorSecretos(Options.Create(new IdentidadOptions { ClaveRotacion = new string('a', 32) }));
        var b = new GeneradorSecretos(Options.Create(new IdentidadOptions { ClaveRotacion = new string('b', 32) }));
        var token = a.Aleatorio32();
        Assert.Equal(43, token.Length);
        Assert.NotEqual(token, a.Aleatorio32());
        Assert.Equal(32, a.Hash(token).Length);
        Assert.Equal(a.Derivar(token), a.Derivar(token));
        Assert.NotEqual(a.Derivar(token), b.Derivar(token));
    }

    [Fact]
    public void PasswordHasher_sala_y_verifica_sin_aceptar_hash_ausente()
    {
        var hash = new HashContrasenas();
        var uno = hash.Hash("una-clave-larga");
        var dos = hash.Hash("una-clave-larga");
        Assert.NotEqual(uno, dos);
        Assert.True(hash.Verificar(uno, "una-clave-larga", out var rehash));
        Assert.False(rehash);
        Assert.False(hash.Verificar(uno, "incorrecta", out _));
        Assert.False(hash.Verificar(null, "una-clave-larga", out _));
    }

    [Fact]
    public void Recuperacion_limita_correo_e_ip_y_libera_al_limite()
    {
        var reloj = new FakeTimeProvider();
        var limite = new LimitadorRecuperacionEnMemoria(Options.Create(new IdentidadOptions()), reloj);
        for (var i = 0; i < 3; i++) Assert.True(limite.Admitir("a@example.com", "ip"));
        Assert.False(limite.Admitir("a@example.com", "otra-ip"));
        for (var i = 0; i < 7; i++) Assert.True(limite.Admitir(i + "@example.com", "ip"));
        Assert.False(limite.Admitir("nuevo@example.com", "ip"));
        reloj.Advance(TimeSpan.FromHours(1));
        Assert.True(limite.Admitir("a@example.com", "ip"));
    }
}
