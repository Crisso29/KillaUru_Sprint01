using KillaUru.Identidad.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KillaUru.Identidad.Infrastructure.Configuraciones;

/// <summary>Cuenta y política de bloqueo en una sola fila, sin relación con eventos.</summary>
internal sealed class UsuarioConfig : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuario"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.Correo).HasColumnName("correo").HasColumnType("citext").HasConversion(x => x.Valor, x => new Domain.Correo(x));
        builder.Property(x => x.HashContrasena).HasColumnName("hash_contrasena");
        builder.Property(x => x.Rol).HasColumnName("rol").HasConversion<short>();
        builder.Property(x => x.Activo).HasColumnName("activo");
        builder.Property(x => x.CorreoVerificado).HasColumnName("correo_verificado");
        builder.Property(x => x.CreadoUtc).HasColumnName("creado_utc");
        builder.Ignore(x => x.Estado);
        builder.OwnsOne(x => x.Bloqueo, bloqueo =>
        {
            bloqueo.Property(x => x.IntentosFallidos).HasColumnName("intentos_fallidos");
            bloqueo.Property(x => x.NivelBloqueo).HasColumnName("nivel_bloqueo");
            bloqueo.Property(x => x.VentanaFallosDesdeUtc).HasColumnName("ventana_fallos_desde_utc");
            bloqueo.Property(x => x.BloqueadoHasta).HasColumnName("bloqueado_hasta");
        });
        builder.Navigation(x => x.Bloqueo).IsRequired();
    }
}
