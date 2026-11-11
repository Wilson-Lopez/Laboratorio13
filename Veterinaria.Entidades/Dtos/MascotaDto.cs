namespace Veterinaria.Entidades.Dtos;

// DTO de lectura: lo que la API devuelve de una mascota
public class MascotaDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Especie { get; set; } = string.Empty;

    public string? Raza { get; set; }

    public DateOnly FechaNacimiento { get; set; }

    public decimal Peso { get; set; }

    public int PropietarioId { get; set; }

    // Nombre del propietario en lugar del objeto Propietario (ForMember en el MappingProfile)
    public string Propietario { get; set; } = string.Empty;
}
