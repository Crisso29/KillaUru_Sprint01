using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace KillaUru.Anomalias.Infrastructure.Configuraciones;
/// <summary>Mapeo explícito del agregado con posición fija geography y fechas UTC.</summary>
internal sealed class AnomaliaConfig : IEntityTypeConfiguration<Anomalia>
{
    public void Configure(EntityTypeBuilder<Anomalia> b)
    {
        b.ToTable("anomalia"); b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.Tipo).HasColumnName("tipo").HasConversion<short>();
        b.Property(x => x.Estado).HasColumnName("estado").HasConversion<short>();
        b.Property(x => x.Persistencia).HasColumnName("persistencia").HasConversion<short>();
        b.Property(x => x.SeveridadMostrada).HasColumnName("severidad_mostrada").HasConversion(x => (short)x.Valor, x => Severidad.Crear(x)!.Value);
        b.Property(x => x.Posicion).HasColumnName("posicion").HasColumnType("geography(Point,4326)").HasConversion(x => ConversionEspacial.Punto(x), x => ConversionEspacial.Coordenada(x));
        b.Property(x => x.Celda).HasColumnName("celda").HasConversion(x => x.Id, x => new Celda(x));
        b.Property(x => x.KmVia).HasColumnName("km_via");
        b.Ignore(x => x.Transiciones);
        ConfigurarEstado(b);
    }
    private static void ConfigurarEstado(EntityTypeBuilder<Anomalia> b)
    {
        b.Property(x => x.UltimaDeteccionUtc).HasColumnName("ultima_deteccion_utc");
        b.Property(x => x.VenceUtc).HasColumnName("vence_utc");
        b.Property(x => x.EstadoDesdeUtc).HasColumnName("estado_desde_utc");
        b.Property(x => x.ActualizadaUtc).HasColumnName("actualizada_utc");
        b.Property(x => x.CreadaPorTope).HasColumnName("creada_por_tope");
        b.Property(x => x.PorRevisar).HasColumnName("por_revisar");
        b.Property(x => x.MarcasRevision).HasColumnName("marcas_revision").HasColumnType("timestamp with time zone[]");
    }
}
