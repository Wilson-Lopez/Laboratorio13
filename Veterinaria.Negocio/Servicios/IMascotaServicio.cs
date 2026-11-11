using Veterinaria.Entidades.Dtos;

namespace Veterinaria.Negocio.Servicios;

public interface IMascotaServicio
{
    Task<List<MascotaDto>> ListarAsync(string? especie, string? raza, string? nombre);

    Task<MascotaDto?> ObtenerPorIdAsync(int id);

    // Devuelve null si el propietario no existe
    Task<List<MascotaDto>?> ListarPorPropietarioAsync(int propietarioId);

    Task<MascotaDto> CrearAsync(MascotaCrearDto dto);

    Task<bool> ActualizarAsync(int id, MascotaActualizarDto dto);

    Task<bool> EliminarAsync(int id);
}
