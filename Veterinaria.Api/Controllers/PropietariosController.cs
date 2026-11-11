using Microsoft.AspNetCore.Mvc;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Negocio.Excepciones;
using Veterinaria.Negocio.Servicios;

namespace Veterinaria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropietariosController : ControllerBase
{
    private readonly IPropietarioServicio _propietarioServicio;
    private readonly IMascotaServicio _mascotaServicio;

    public PropietariosController(IPropietarioServicio propietarioServicio, IMascotaServicio mascotaServicio)
    {
        _propietarioServicio = propietarioServicio;
        _mascotaServicio = mascotaServicio;
    }

    // GET: api/propietarios?nombres=Rojas&dni=458
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PropietarioDto>>> GetPropietarios(
        [FromQuery] string? nombres,
        [FromQuery] string? dni)
    {
        var propietarios = await _propietarioServicio.ListarAsync(nombres, dni);
        return Ok(propietarios);
    }

    // GET: api/propietarios/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropietarioDto>> GetPropietario(int id)
    {
        var propietario = await _propietarioServicio.ObtenerPorIdAsync(id);

        if (propietario == null)
        {
            return NotFound();
        }

        return Ok(propietario);
    }

    // GET: api/propietarios/5/mascotas  (ruta anidada)
    [HttpGet("{id}/mascotas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<MascotaDto>>> GetMascotasDelPropietario(int id)
    {
        var mascotas = await _mascotaServicio.ListarPorPropietarioAsync(id);

        if (mascotas == null)
        {
            return NotFound();
        }

        return Ok(mascotas);
    }

    // POST: api/propietarios
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PropietarioDto>> PostPropietario(PropietarioCrearDto dto)
    {
        try
        {
            var creado = await _propietarioServicio.CrearAsync(dto);
            return CreatedAtAction(nameof(GetPropietario), new { id = creado.Id }, creado);
        }
        catch (ReglaNegocioException ex)
        {
            // La regla de negocio incumplida se convierte en 409 Conflict
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Se incumplió una regla de negocio.",
                Detail = ex.Message
            });
        }
    }

    // PUT: api/propietarios/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutPropietario(int id, PropietarioActualizarDto dto)
    {
        var actualizado = await _propietarioServicio.ActualizarAsync(id, dto);

        if (!actualizado)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: api/propietarios/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeletePropietario(int id)
    {
        try
        {
            var eliminado = await _propietarioServicio.EliminarAsync(id);

            if (!eliminado)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ReglaNegocioException ex)
        {
            // La regla de negocio incumplida se convierte en 409 Conflict
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Se incumplió una regla de negocio.",
                Detail = ex.Message
            });
        }
    }
}
