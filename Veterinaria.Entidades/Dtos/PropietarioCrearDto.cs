using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Entidades.Dtos;

// DTO de entrada para registrar un propietario (POST)
public class PropietarioCrearDto
{
    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Los nombres deben tener entre 3 y 100 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener 8 dígitos.")]
    [RegularExpression("^[0-9]{8}$", ErrorMessage = "El DNI solo debe contener números.")]
    public string Dni { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(15, MinimumLength = 7, ErrorMessage = "El teléfono debe tener entre 7 y 15 caracteres.")]
    public string Telefono { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres.")]
    public string? Correo { get; set; }
}
