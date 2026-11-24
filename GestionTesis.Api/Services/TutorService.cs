using GestionTesis.Api.Data;
using GestionTesis.Api.Interfaces;
using GestionTesis.Api.Models;
using GestionTesis.Contratos.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GestionTesis.Api.Services;

/// <summary>Aquí viven las reglas de negocio de los tutores.</summary>
public class TutorService(AppDbContext db) : ITutorService
{
    public async Task<List<TutorDto>> ListarAsync() =>
        await db.Tutores
            .AsNoTracking()
            .OrderBy(t => t.Nombre)
            .Select(t => new TutorDto
            {
                Id = t.Id,
                Nombre = t.Nombre,
                Especialidad = t.Especialidad,
                Correo = t.Correo,
                CantidadTesis = t.Tesis.Count
            })
            .ToListAsync();

    public async Task<Resultado<TutorDto>> ObtenerAsync(Guid id)
    {
        var dto = await db.Tutores
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TutorDto
            {
                Id = t.Id,
                Nombre = t.Nombre,
                Especialidad = t.Especialidad,
                Correo = t.Correo,
                CantidadTesis = t.Tesis.Count
            })
            .FirstOrDefaultAsync();

        return dto is null
            ? Resultado<TutorDto>.Falla(TipoError.NoEncontrado, "El tutor no existe.")
            : Resultado<TutorDto>.Ok(dto);
    }

    public async Task<Resultado<TutorDto>> CrearAsync(TutorGuardarDto dto)
    {
        var nombre = dto.Nombre.Trim();
        var correo = dto.Correo.Trim().ToLower();

        // Regla: no puede duplicarse un tutor (ni por nombre ni por correo)
        var duplicado = await ValidarDuplicadosAsync(nombre, correo, idActual: null);
        if (duplicado is not null) return duplicado;

        var tutor = new Tutor
        {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            Especialidad = dto.Especialidad.Trim(),
            Correo = correo
        };

        db.Tutores.Add(tutor);
        await db.SaveChangesAsync();

        return await ObtenerAsync(tutor.Id);
    }

    public async Task<Resultado<TutorDto>> EditarAsync(Guid id, TutorGuardarDto dto)
    {
        var tutor = await db.Tutores.FindAsync(id);
        if (tutor is null)
            return Resultado<TutorDto>.Falla(TipoError.NoEncontrado, "El tutor no existe.");

        var nombre = dto.Nombre.Trim();
        var correo = dto.Correo.Trim().ToLower();

        var duplicado = await ValidarDuplicadosAsync(nombre, correo, idActual: id);
        if (duplicado is not null) return duplicado;

        tutor.Nombre = nombre;
        tutor.Especialidad = dto.Especialidad.Trim();
        tutor.Correo = correo;
        await db.SaveChangesAsync();

        return await ObtenerAsync(id);
    }

    public async Task<Resultado<bool>> EliminarAsync(Guid id)
    {
        var tutor = await db.Tutores.FindAsync(id);
        if (tutor is null)
            return Resultado<bool>.Falla(TipoError.NoEncontrado, "El tutor no existe.");

        // Regla: no borrar un tutor que tiene tesis (además la BD tiene Restrict)
        var cantidad = await db.Tesis.CountAsync(t => t.TutorId == id);
        if (cantidad > 0)
            return Resultado<bool>.Falla(TipoError.Conflicto,
                $"No se puede eliminar a {tutor.Nombre}: tiene {cantidad} tesis asignada(s). Reasígnelas o elimínelas primero.");

        db.Tutores.Remove(tutor);
        await db.SaveChangesAsync();
        return Resultado<bool>.Ok(true);
    }

    private async Task<Resultado<TutorDto>?> ValidarDuplicadosAsync(string nombre, string correo, Guid? idActual)
    {
        // SQL Server compara sin distinguir mayúsculas (collation por defecto)
        if (await db.Tutores.AnyAsync(t => t.Nombre == nombre && t.Id != idActual))
            return Resultado<TutorDto>.Falla(TipoError.Conflicto, "Ya existe un tutor con ese nombre.", "Nombre");

        if (await db.Tutores.AnyAsync(t => t.Correo == correo && t.Id != idActual))
            return Resultado<TutorDto>.Falla(TipoError.Conflicto, "Ya existe un tutor con ese correo.", "Correo");

        return null;
    }
}
