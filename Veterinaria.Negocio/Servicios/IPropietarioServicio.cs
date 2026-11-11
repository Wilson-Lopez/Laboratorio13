using Veterinaria.Entidades.Dtos;

namespace Veterinaria.Negocio.Servicios;

public interface IPropietarioServicio
{
    Task<List<PropietarioDto>> ListarAsync(string? nombres, string? dni);

    Task<PropietarioDto?> ObtenerPorIdAsync(int id);

    Task<PropietarioDto> CrearAsync(PropietarioCrearDto dto);

    Task<bool> ActualizarAsync(int id, PropietarioActualizarDto dto);

    Task<bool> EliminarAsync(int id);
}
