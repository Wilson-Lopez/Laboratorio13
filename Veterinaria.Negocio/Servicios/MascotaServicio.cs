using AutoMapper;
using Veterinaria.Datos.Repositorios;
using Veterinaria.Entidades;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Negocio.Excepciones;

namespace Veterinaria.Negocio.Servicios;

public class MascotaServicio : IMascotaServicio
{
    private readonly IMascotaRepositorio _mascotaRepositorio;
    private readonly IPropietarioRepositorio _propietarioRepositorio;
    private readonly IMapper _mapper;

    public MascotaServicio(
        IMascotaRepositorio mascotaRepositorio,
        IPropietarioRepositorio propietarioRepositorio,
        IMapper mapper)
    {
        _mascotaRepositorio = mascotaRepositorio;
        _propietarioRepositorio = propietarioRepositorio;
        _mapper = mapper;
    }

    public async Task<List<MascotaDto>> ListarAsync(string? especie, string? raza, string? nombre)
    {
        var mascotas = await _mascotaRepositorio.ListarAsync(especie, raza, nombre);
        return _mapper.Map<List<MascotaDto>>(mascotas);
    }

    public async Task<MascotaDto?> ObtenerPorIdAsync(int id)
    {
        var mascota = await _mascotaRepositorio.ObtenerPorIdAsync(id);
        return mascota == null ? null : _mapper.Map<MascotaDto>(mascota);
    }

    public async Task<List<MascotaDto>?> ListarPorPropietarioAsync(int propietarioId)
    {
        if (!await _propietarioRepositorio.ExisteAsync(propietarioId))
        {
            return null;
        }

        var mascotas = await _mascotaRepositorio.ListarPorPropietarioAsync(propietarioId);
        return _mapper.Map<List<MascotaDto>>(mascotas);
    }

    public async Task<MascotaDto> CrearAsync(MascotaCrearDto dto)
    {
        await ValidarDatosAsync(dto.PropietarioId, dto.FechaNacimiento);

        // Regla de negocio 3: un propietario no puede tener dos mascotas con el mismo nombre
        if (await _mascotaRepositorio.ExisteNombreAsync(dto.PropietarioId, dto.Nombre))
        {
            throw new ReglaNegocioException(
                $"El propietario con id {dto.PropietarioId} ya tiene una mascota llamada {dto.Nombre}.");
        }

        // AutoMapper crea una entidad nueva a partir del DTO de entrada
        var mascota = _mapper.Map<Mascota>(dto);

        await _mascotaRepositorio.AgregarAsync(mascota);

        // Se vuelve a leer con su propietario para devolver el nombre del propietario en el DTO
        var creada = await _mascotaRepositorio.ObtenerPorIdAsync(mascota.Id);

        return _mapper.Map<MascotaDto>(creada);
    }

    public async Task<bool> ActualizarAsync(int id, MascotaActualizarDto dto)
    {
        var mascota = await _mascotaRepositorio.BuscarParaEditarAsync(id);

        if (mascota == null)
        {
            return false;
        }

        await ValidarDatosAsync(dto.PropietarioId, dto.FechaNacimiento);

        // Regla de negocio 3: sin contar a la propia mascota, el nombre no se puede repetir con el mismo propietario
        if (await _mascotaRepositorio.ExisteNombreAsync(dto.PropietarioId, dto.Nombre, id))
        {
            throw new ReglaNegocioException(
                $"El propietario con id {dto.PropietarioId} ya tiene otra mascota llamada {dto.Nombre}.");
        }

        // AutoMapper copia el DTO sobre la entidad existente: el Id se conserva
        _mapper.Map(dto, mascota);

        await _mascotaRepositorio.ActualizarAsync(mascota);

        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var mascota = await _mascotaRepositorio.BuscarParaEditarAsync(id);

        if (mascota == null)
        {
            return false;
        }

        await _mascotaRepositorio.EliminarAsync(mascota);

        return true;
    }

    // Datos que solo se pueden comprobar en el negocio: si fallan, el controlador responde 400
    private async Task ValidarDatosAsync(int propietarioId, DateOnly? fechaNacimiento)
    {
        // La mascota debe pertenecer a un propietario registrado
        if (!await _propietarioRepositorio.ExisteAsync(propietarioId))
        {
            throw new DatoInvalidoException(
                nameof(MascotaCrearDto.PropietarioId), $"No existe el propietario con id {propietarioId}.");
        }

        // La fecha de nacimiento no puede ser posterior a la fecha actual
        if (fechaNacimiento > DateOnly.FromDateTime(DateTime.Today))
        {
            throw new DatoInvalidoException(
                nameof(MascotaCrearDto.FechaNacimiento), "La fecha de nacimiento no puede ser una fecha futura.");
        }
    }
}
