using KillaUru.Anomalias.Domain;
using KillaUru.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace KillaUru.Anomalias.Infrastructure.Configuraciones;
/// <summary>El tipo viene del agregado; el seudónimo es una columna nullable, nunca JSON.</summary>
internal sealed class ReporteConfig : IEntityTypeConfiguration<ReporteAnomalia>
{
    public void Configure(EntityTypeBuilder<ReporteAnomalia> b)
    {
        b.ToTable("reporte_anomalia"); b.HasKey(x => x.EventoId); b.Ignore(x => x.Tipo);
        b.Property(x => x.EventoId).HasColumnName("evento_id");
        b.Property(x => x.AnomaliaId).HasColumnName("anomalia_id");
        b.Property(x => x.DetectadoUtc).HasColumnName("detectado_utc");
        b.Property(x => x.RecibidoUtc).HasColumnName("recibido_utc");
        b.Property(x => x.PrecisionMetros).HasColumnName("precision_m").HasConversion<float>();
        b.Property(x => x.Severidad).HasColumnName("severidad").HasConversion(x => (short)x.Valor, x => Severidad.Crear(x)!.Value);
        b.Property(x => x.Confidence).HasColumnName("confidence").HasConversion(x => (float)x.Valor, x => Confidence.Crear(x)!.Value);
        b.Property(x => x.Posicion).HasColumnName("posicion").HasColumnType("geography(Point,4326)").HasConversion(x => ConversionEspacial.Punto(x), x => ConversionEspacial.Coordenada(x));
        b.Property(x => x.KmVia).HasColumnName("km_via");
        b.Property(x => x.Seudonimo).HasColumnName("seudonimo");
        b.Property(x => x.ConSesion).HasColumnName("con_sesion");
        b.Property(x => x.ModeloVersion).HasColumnName("modelo_version");
        b.HasOne<Anomalia>().WithMany().HasForeignKey(x => x.AnomaliaId).OnDelete(DeleteBehavior.Cascade);
    }
}
