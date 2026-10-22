using KillaUru.Shared.Tareas;

namespace KillaUru.Shared.Tests.Tareas;

public sealed class ProgramaRelojTests
{
    private static DateTimeOffset Utc(string texto) => DateTimeOffset.Parse(texto, System.Globalization.CultureInfo.InvariantCulture);

    [Theory]
    [InlineData("2026-10-22T10:04:59Z", 5, "2026-10-22T10:05:00Z")]
    [InlineData("2026-10-22T10:05:00Z", 5, "2026-10-22T11:05:00Z")]   // estrictamente después de ahora
    [InlineData("2026-10-22T23:30:00Z", 10, "2026-10-23T00:10:00Z")]  // cruza el día
    public void ProximaHoraria(string ahora, int minuto, string esperada) =>
        Assert.Equal(Utc(esperada), ProgramaReloj.ProximaHoraria(Utc(ahora), minuto));

    [Theory]
    [InlineData("2026-10-22T08:29:59Z", "03:30", "2026-10-22T08:30:00Z")]   // 03:30 de Lima = 08:30 UTC
    [InlineData("2026-10-22T08:30:01Z", "03:30", "2026-10-23T08:30:00Z")]
    [InlineData("2026-10-22T05:29:59Z", "00:30", "2026-10-22T05:30:00Z")]   // indicadores (007)
    [InlineData("2026-10-22T03:00:00Z", "23:00", "2026-10-22T04:00:00Z")]   // 22:00 de Lima del día anterior
    public void ProximaDiaria_en_hora_de_Lima(string ahora, string horaLocal, string esperada)
    {
        var hora = TimeOnly.Parse(horaLocal, System.Globalization.CultureInfo.InvariantCulture);

        Assert.Equal(Utc(esperada), ProgramaReloj.ProximaDiaria(Utc(ahora), hora));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(60)]
    public void ProximaHoraria_con_minuto_invalido_es_error(int minuto) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => ProgramaReloj.ProximaHoraria(Utc("2026-10-22T10:00:00Z"), minuto));
}
