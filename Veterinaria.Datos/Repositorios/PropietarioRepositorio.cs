using Microsoft.EntityFrameworkCore;
using Veterinaria.Datos.Models;
using Veterinaria.Entidades;

namespace Veterinaria.Datos.Repositorios;

public class PropietarioRepositorio : IPropietarioRepositorio
{
    private readonly VeterinariaContext _context;

    public PropietarioRepositorio(VeterinariaContext context)
    {
        _context = context;
    }

    public async Task<List<Propietario>> ListarAsync(string? nombres, string? dni)
    {
        // Include de Mascotas para que el MappingProfile calcule CantidadMascotas
        var consulta = _context.Propietarios
            .AsNoTracking()
            .Include(p => p.Mascotas)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(nombres))
        {
            consulta = consulta.Where(p => p.Nombres.Contains(nombres));
        }

        if (!string.IsNullOrWhiteSpace(dni))
        {
            consulta = consulta.Where(p => p.Dni.StartsWith(dni));
        }

        return await consulta.OrderBy(p => p.Nombres).ToListAsync();
    }

    public async Task<Propietario?> ObtenerPorIdAsync(int id)
    {
        return await _context.Propietarios
            .AsNoTracking()
            .Include(p => p.Mascotas)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // Con seguimiento (tracking): el servicio copia el DTO sobre esta entidad y luego se guarda
    public async Task<Propietario?> BuscarParaEditarAsync(int id)
    {
        return await _context.Propietarios.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<bool> ExisteAsync(int id)
    {
        return await _context.Propietarios.AnyAsync(p => p.Id == id);
    }

    public async Task<bool> ExisteDniAsync(string dni)
    {
        return await _context.Propietarios.AnyAsync(p => p.Dni == dni);
    }

    public async Task<int> ContarMascotasAsync(int id)
    {
        return await _context.Mascotas.CountAsync(m => m.PropietarioId == id);
    }

    public async Task AgregarAsync(Propietario propietario)
    {
        await _context.Propietarios.AddAsync(propietario);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarAsync(Propietario propietario)
    {
        _context.Propietarios.Update(propietario);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(Propietario propietario)
    {
        _context.Propietarios.Remove(propietario);
        await _context.SaveChangesAsync();
    }
}
