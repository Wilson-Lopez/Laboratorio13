using AutoMapper;
using Veterinaria.Entidades;
using Veterinaria.Entidades.Dtos;

namespace Veterinaria.Negocio.Mapeos;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ---------- Propietarios ----------

        // Entidad -> DTO de lectura (lo que devuelve la API)
        CreateMap<Propietario, PropietarioDto>()
            .ForMember(destino => destino.CantidadMascotas,
                       opciones => opciones.MapFrom(origen => origen.Mascotas.Count));

        // DTO de entrada -> Entidad (POST)
        CreateMap<PropietarioCrearDto, Propietario>();

        // DTO de entrada -> Entidad existente (PUT)
        CreateMap<PropietarioActualizarDto, Propietario>();

        // ---------- Mascotas ----------

        // Entidad -> DTO de lectura (lo que devuelve la API)
        CreateMap<Mascota, MascotaDto>()
            .ForMember(destino => destino.Propietario,
                       opciones => opciones.MapFrom(origen => origen.Propietario.Nombres));

        // DTO de entrada -> Entidad (POST)
        CreateMap<MascotaCrearDto, Mascota>();

        // DTO de entrada -> Entidad existente (PUT)
        CreateMap<MascotaActualizarDto, Mascota>();
    }
}
