using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Entidades.Dtos;

// DTO de entrada para registrar una mascota (POST)
public class MascotaCrearDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La especie es obligatoria.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "La especie debe tener entre 3 y 30 caracteres.")]
    public string Especie { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "La raza no puede superar los 50 caracteres.")]
    public string? Raza { get; set; }

    // Nullable para que [Required] detecte cuando no se envía (un DateOnly vacío sería 01/01/0001)
    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateOnly? FechaNacimiento { get; set; }

    [Range(0.01, 150.0, ErrorMessage = "El peso debe estar entre 0.01 y 150 kg.")]
    public decimal Peso { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El id del propietario debe ser mayor que 0.")]
    public int PropietarioId { get; set; }
}
