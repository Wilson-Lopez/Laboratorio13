using AutoMapper;
using Veterinaria.Datos.Repositorios;
using Veterinaria.Entidades;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Negocio.Excepciones;

namespace Veterinaria.Negocio.Servicios;

public class PropietarioServicio : IPropietarioServicio
{
    private readonly IPropietarioRepositorio _propietarioRepositorio;
    private readonly IMapper _mapper;

    public PropietarioServicio(IPropietarioRepositorio propietarioRepositorio, IMapper mapper)
    {
        _propietarioRepositorio = propietarioRepositorio;
        _mapper = mapper;
    }

    public async Task<List<PropietarioDto>> ListarAsync(string? nombres, string? dni)
    {
        var propietarios = await _propietarioRepositorio.ListarAsync(nombres, dni);
        return _mapper.Map<List<PropietarioDto>>(propietarios);
    }

    public async Task<PropietarioDto?> ObtenerPorIdAsync(int id)
    {
        var propietario = await _propietarioRepositorio.ObtenerPorIdAsync(id);
        return propietario == null ? null : _mapper.Map<PropietarioDto>(propietario);
    }

    public async Task<PropietarioDto> CrearAsync(PropietarioCrearDto dto)
    {
        // Regla de negocio 1: no puede haber dos propietarios con el mismo DNI
        if (await _propietarioRepositorio.ExisteDniAsync(dto.Dni))
        {
            throw new ReglaNegocioException($"Ya existe un propietario registrado con el DNI {dto.Dni}.");
        }

        // AutoMapper crea una entidad nueva a partir del DTO de entrada
        var propietario = _mapper.Map<Propietario>(dto);

        await _propietarioRepositorio.AgregarAsync(propietario);

        return _mapper.Map<PropietarioDto>(propietario);
    }

    public async Task<bool> ActualizarAsync(int id, PropietarioActualizarDto dto)
    {
        var propietario = await _propietarioRepositorio.BuscarParaEditarAsync(id);

        if (propietario == null)
        {
            return false;
        }

        // AutoMapper copia el DTO sobre la entidad existente: el Id y el DNI se conservan
        _mapper.Map(dto, propietario);

        await _propietarioRepositorio.ActualizarAsync(propietario);

        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var propietario = await _propietarioRepositorio.BuscarParaEditarAsync(id);

        if (propietario == null)
        {
            return false;
        }

        // Regla de negocio 2: no se elimina un propietario que todavía tiene mascotas registradas
        var cantidadMascotas = await _propietarioRepositorio.ContarMascotasAsync(id);

        if (cantidadMascotas > 0)
        {
            throw new ReglaNegocioException(
                $"No se puede eliminar al propietario porque tiene {cantidadMascotas} mascota(s) registrada(s).");
        }

        await _propietarioRepositorio.EliminarAsync(propietario);

        return true;
    }
}
