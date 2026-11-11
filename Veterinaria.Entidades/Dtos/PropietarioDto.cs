namespace Veterinaria.Entidades.Dtos;

// DTO de lectura: lo que la API devuelve de un propietario
public class PropietarioDto
{
    public int Id { get; set; }

    public string Nombres { get; set; } = string.Empty;

    public string Dni { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public string? Correo { get; set; }

    // Se calcula en el MappingProfile a partir de la colección Mascotas
    public int CantidadMascotas { get; set; }
}
