using KillaUru.Identidad.Domain;
namespace KillaUru.Identidad.Tests.Domain;

public sealed class EntidadesTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 23, 10, 17, 43, TimeSpan.Zero);

    [Fact]
    public void Cuenta_invitada_activa_verificada_y_desactivada()
    {
        var correo = Correo.Crear("conductor@example.com")!;
        var u = Usuario.Invitar(correo, RolUsuario.Administrador, Ahora);
        Assert.NotEqual(Guid.Empty, u.Id);
        Assert.Equal(correo, u.Correo);
        Assert.Equal("InvitacionPendiente", u.Estado);
        Assert.False(u.PuedeIniciarSesion(Ahora));
        Assert.Equal(HorasGuardadas.AlDia(Ahora), u.CreadoUtc);
        u.FijarContrasena("hash");
        u.MarcarCorreoVerificado();
        Assert.Equal("hash", u.HashContrasena);
        Assert.True(u.CorreoVerificado);
        Assert.True(u.PuedeIniciarSesion(Ahora));
        Assert.Equal("Activa", u.Estado);
        u.Desactivar();
        Assert.False(u.Activo);
        Assert.Equal("Desactivada", u.Estado);
        Assert.False(u.PuedeIniciarSesion(Ahora));
        u.Activar();
        u.CambiarRol(RolUsuario.ConductorRegistrado);
        Assert.Equal(RolUsuario.ConductorRegistrado, u.Rol);
        Assert.True(u.Activo);
    }

    [Fact]
    public void Registro_crea_conductor_y_sus_constancias()
    {
        var u = Usuario.RegistrarConductor(Correo.Crear("a@example.com")!, "hash", Ahora);
        var c = Consentimiento.Crear(u.Id, DocumentoLegal.PoliticaPrivacidad, "1.2", Ahora);
        Assert.Equal(u.Id, c.UsuarioId);
        Assert.Equal(DocumentoLegal.PoliticaPrivacidad, c.Documento);
        Assert.Equal("1.2", c.Version);
        Assert.Equal(HorasGuardadas.AlDia(Ahora), c.AceptadoUtc);
        var r = RevisionPrivilegios.Crear(u.Id, 2, 1, Ahora);
        Assert.Equal(0, r.Id);
        Assert.Equal(u.Id, r.ActorId);
        Assert.Equal(2, r.Confirmados);
        Assert.Equal(1, r.Retirados);
        Assert.Equal(Ahora, r.RealizadaUtc);
        Assert.Equal(Ahora, RevisionPrivilegios.Vence(null, Ahora));
        Assert.Equal(new DateTimeOffset(2026, 11, 1, 5, 0, 0, TimeSpan.Zero), RevisionPrivilegios.Vence(Ahora, Ahora));
    }

    [Fact]
    public void Sesiones_y_enlaces_copian_hash_y_respetan_limite_absoluto()
    {
        var id = Guid.NewGuid();
        byte[] hash = [1, 2, 3];
        var s = SesionRenovacion.Crear(id, id, hash, Ahora, Ahora.AddDays(1), Ahora.AddHours(1));
        hash[0] = 9;
        Assert.Equal(1, s.HashToken[0]);
        Assert.NotEqual(Guid.Empty, s.Id);
        Assert.Equal(id, s.UsuarioId);
        Assert.Equal(id, s.FamiliaId);
        Assert.Equal(HorasGuardadas.ALaHora(Ahora), s.CreadoUtc);
        Assert.Equal(Ahora.AddHours(1), s.LimiteAbsolutoUtc);
        Assert.Equal("sesion_invalida", s.Validar(Ahora.AddHours(1), true, 60));
        var t = TokenCorreo.Crear(hash, id, PropositoToken.Invitacion, Ahora, TimeSpan.FromHours(24));
        Assert.Equal(id, t.UsuarioId);
        Assert.Equal(PropositoToken.Invitacion, t.Proposito);
        Assert.Equal(hash, t.HashToken);
        t.Usar(Ahora);
        t.Usar(Ahora.AddMinutes(1));
        Assert.Equal(Ahora, t.UsadoUtc);
    }

    [Fact]
    public void Casos_limite_restantes()
    {
        Assert.Null(Correo.Crear(null));
        Assert.Null(Correo.Crear("@example.com"));
        Assert.Null(Correo.Crear("a@.example.com"));
        var p = new PoliticaBloqueo();
        var parametros = new ParametrosBloqueo(1, 15, [5]);
        Assert.True(p.RegistrarFallo(Ahora, parametros));
        Assert.False(p.RegistrarFallo(Ahora, parametros));
        Assert.Equal(0, p.IntentosFallidos);
        Assert.Null(p.VentanaFallosDesdeUtc);
        Assert.Equal(HorasGuardadas.ALaHora(Ahora.AddHours(8)), PoliticaSesion.Vencimiento(Ahora, Ahora.AddHours(10), 30, 8));
    }
}
