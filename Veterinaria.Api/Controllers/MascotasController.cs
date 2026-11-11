using Microsoft.AspNetCore.Mvc;
using Veterinaria.Entidades.Dtos;
using Veterinaria.Negocio.Excepciones;
using Veterinaria.Negocio.Servicios;

namespace Veterinaria.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MascotasController : ControllerBase
{
    private readonly IMascotaServicio _mascotaServicio;

    public MascotasController(IMascotaServicio mascotaServicio)
    {
        _mascotaServicio = mascotaServicio;
    }

    // GET: api/mascotas?especie=Perro&raza=Labrador&nombre=Fir
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MascotaDto>>> GetMascotas(
        [FromQuery] string? especie,
        [FromQuery] string? raza,
        [FromQuery] string? nombre)
    {
        var mascotas = await _mascotaServicio.ListarAsync(especie, raza, nombre);
        return Ok(mascotas);
    }

    // GET: api/mascotas/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MascotaDto>> GetMascota(int id)
    {
        var mascota = await _mascotaServicio.ObtenerPorIdAsync(id);

        if (mascota == null)
        {
            return NotFound();
        }

        return Ok(mascota);
    }

    // POST: api/mascotas
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MascotaDto>> PostMascota(MascotaCrearDto dto)
    {
        try
        {
            var creada = await _mascotaServicio.CrearAsync(dto);
            return CreatedAtAction(nameof(GetMascota), new { id = creada.Id }, creada);
        }
        catch (DatoInvalidoException ex)
        {
            ModelState.AddModelError(ex.Campo, ex.Message);
            return ValidationProblem(ModelState);
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

    // PUT: api/mascotas/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PutMascota(int id, MascotaActualizarDto dto)
    {
        try
        {
            var actualizada = await _mascotaServicio.ActualizarAsync(id, dto);

            if (!actualizada)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (DatoInvalidoException ex)
        {
            ModelState.AddModelError(ex.Campo, ex.Message);
            return ValidationProblem(ModelState);
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

    // DELETE: api/mascotas/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMascota(int id)
    {
        var eliminada = await _mascotaServicio.EliminarAsync(id);

        if (!eliminada)
        {
            return NotFound();
        }

        return NoContent();
    }
}
