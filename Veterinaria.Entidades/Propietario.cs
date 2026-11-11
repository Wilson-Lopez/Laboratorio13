using System;
using System.Collections.Generic;

namespace Veterinaria.Entidades;

public partial class Propietario
{
    public int Id { get; set; }

    public string Nombres { get; set; } = null!;

    public string Dni { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string? Correo { get; set; }

    public virtual ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
}
