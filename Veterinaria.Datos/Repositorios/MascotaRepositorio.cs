using Microsoft.EntityFrameworkCore;
using Veterinaria.Datos.Models;
using Veterinaria.Entidades;

namespace Veterinaria.Datos.Repositorios;

public class MascotaRepositorio : IMascotaRepositorio
{
    private readonly VeterinariaContext _context;

    public MascotaRepositorio(VeterinariaContext context)
    {
        _context = context;
    }

    public async Task<List<Mascota>> ListarAsync(string? especie, string? raza, string? nombre)
    {
        // Include del Propietario para que el MappingProfile devuelva su nombre
        var consulta = _context.Mascotas
            .AsNoTracking()
            .Include(m => m.Propietario)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(especie))
        {
            consulta = consulta.Where(m => m.Especie == especie);
        }

        if (!string.IsNullOrWhiteSpace(raza))
        {
            consulta = consulta.Where(m => m.Raza != null && m.Raza.Contains(raza));
        }

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            consulta = consulta.Where(m => m.Nombre.Contains(nombre));
        }

        return await consulta.OrderBy(m => m.Nombre).ToListAsync();
    }

    public async Task<List<Mascota>> ListarPorPropietarioAsync(int propietarioId)
    {
        return await _context.Mascotas
            .AsNoTracking()
            .Include(m => m.Propietario)
            .Where(m => m.PropietarioId == propietarioId)
            .OrderBy(m => m.Nombre)
            .ToListAsync();
    }

    public async Task<Mascota?> ObtenerPorIdAsync(int id)
    {
        return await _context.Mascotas
            .AsNoTracking()
            .Include(m => m.Propietario)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    // Con seguimiento (tracking) y sin Include: el servicio copia el DTO sobre esta entidad y luego se guarda
    public async Task<Mascota?> BuscarParaEditarAsync(int id)
    {
        return await _context.Mascotas.FirstOrDefaultAsync(m => m.Id == id);
    }

    // excluirId permite ignorar la propia mascota al actualizarla
    public async Task<bool> ExisteNombreAsync(int propietarioId, string nombre, int? excluirId = null)
    {
        return await _context.Mascotas.AnyAsync(m =>
            m.PropietarioId == propietarioId &&
            m.Nombre == nombre &&
            m.Id != excluirId);
    }

    public async Task AgregarAsync(Mascota mascota)
    {
        await _context.Mascotas.AddAsync(mascota);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Mascota mascota)
    {
        _context.Mascotas.Update(mascota);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(Mascota mascota)
    {
        _context.Mascotas.Remove(mascota);
        await _context.SaveChangesAsync();
    }
}
