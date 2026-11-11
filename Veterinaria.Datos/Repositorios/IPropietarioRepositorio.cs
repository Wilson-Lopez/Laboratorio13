using Veterinaria.Entidades;

namespace Veterinaria.Datos.Repositorios;

public interface IPropietarioRepositorio
{
    Task<List<Propietario>> ListarAsync(string? nombres, string? dni);

    Task<Propietario?> ObtenerPorIdAsync(int id);

    Task<Propietario?> BuscarParaEditarAsync(int id);

    Task<bool> ExisteAsync(int id);

    Task<bool> ExisteDniAsync(string dni);

    Task<int> ContarMascotasAsync(int id);

    Task AgregarAsync(Propietario propietario);

    Task ActualizarAsync(Propietario propietario);

    Task EliminarAsync(Propietario propietario);
}
