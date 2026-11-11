using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Entidades.Dtos;

// DTO de entrada para actualizar un propietario (PUT).
// No incluye el DNI: es el documento que identifica al propietario y no se modifica.
public class PropietarioActualizarDto
{
    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Los nombres deben tener entre 3 y 100 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(15, MinimumLength = 7, ErrorMessage = "El teléfono debe tener entre 7 y 15 caracteres.")]
    public string Telefono { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres.")]
    public string? Correo { get; set; }
}
