using System.Security.Cryptography;
using System.Text;
using KillaUru.Identidad.Application;
using KillaUru.Identidad.Application.Puertos;
using Microsoft.Extensions.Options;

namespace KillaUru.Identidad.Infrastructure.Seguridad;

/// <summary>Ventana deslizante en memoria; elimina entradas vencidas y no conserva correos ni IP en claro.</summary>
internal sealed class LimitadorRecuperacionEnMemoria(IOptions<IdentidadOptions> opciones, TimeProvider reloj) : ILimitadorRecuperacion
{
    private readonly object _candado = new();
    private readonly Dictionary<string, Queue<DateTimeOffset>> _intentos = [];
    private readonly byte[] _sal = RandomNumberGenerator.GetBytes(32);

    public bool Admitir(string correo, string ip)
    {
        lock (_candado)
        {
            var ahora = reloj.GetUtcNow();
            Limpiar(ahora.AddHours(-1));
            var pedidosAlCorreo = Obtener("correo:" + correo);
            var pedidosDesdeIp = Obtener("ip:" + ip);
            var correoAgotado = pedidosAlCorreo.Count >= opciones.Value.RecuperacionesPorCorreoPorHora;
            var ipAgotada = pedidosDesdeIp.Count >= opciones.Value.RecuperacionesPorIpPorHora;
            if (correoAgotado || ipAgotada) return false;
            pedidosAlCorreo.Enqueue(ahora);
            pedidosDesdeIp.Enqueue(ahora);
            return true;
        }
    }

    /// <summary>La clave se guarda como HMAC con sal aleatoria: la memoria no contiene correos ni IP legibles.</summary>
    private Queue<DateTimeOffset> Obtener(string clave)
    {
        var hash = Convert.ToHexString(HMACSHA256.HashData(_sal, Encoding.UTF8.GetBytes(clave)));
        if (!_intentos.TryGetValue(hash, out var cola)) _intentos[hash] = cola = new();
        return cola;
    }

    private void Limpiar(DateTimeOffset limite)
    {
        foreach (var par in _intentos)
            while (par.Value.TryPeek(out var instante) && instante <= limite) par.Value.Dequeue();
        foreach (var clave in _intentos.Where(x => x.Value.Count == 0).Select(x => x.Key).ToArray()) _intentos.Remove(clave);
    }
}
