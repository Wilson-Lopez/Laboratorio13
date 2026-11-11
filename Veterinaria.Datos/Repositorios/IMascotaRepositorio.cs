using Veterinaria.Entidades;

namespace Veterinaria.Datos.Repositorios;

public interface IMascotaRepositorio
{
    Task<List<Mascota>> ListarAsync(string? especie, string? raza, string? nombre);

    Task<List<Mascota>> ListarPorPropietarioAsync(int propietarioId);

    Task<Mascota?> ObtenerPorIdAsync(int id);

    Task<Mascota?> BuscarParaEditarAsync(int id);

    Task<bool> ExisteNombreAsync(int propietarioId, string nombre, int? excluirId = null);

    Task AgregarAsync(Mascota mascota);

    Task ActualizarAsync(Mascota mascota);

    Task EliminarAsync(Mascota mascota);
}
