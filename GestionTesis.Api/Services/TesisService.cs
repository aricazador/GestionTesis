using GestionTesis.Api.Data;
using GestionTesis.Api.Interfaces;
using GestionTesis.Api.Models;
using GestionTesis.Contratos.Constantes;
using GestionTesis.Contratos.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GestionTesis.Api.Services;

/// <summary>Reglas de negocio de las tesis: títulos únicos, máximo 3 por tutor e imágenes.</summary>
public class TesisService(AppDbContext db, IWebHostEnvironment env) : ITesisService
{
    private const string CarpetaImagenes = "imagenes";
    private const string PrefijoMuestras = "/imagenes/muestras/";

    // Proyección reutilizable: de entidad Tesis (+ su Tutor) a TesisDto
    private static IQueryable<TesisDto> Proyectar(IQueryable<Tesis> consulta) =>
        consulta.Select(t => new TesisDto
        {
            Id = t.Id,
            Titulo = t.Titulo,
            Estudiante = t.Estudiante,
            Estado = t.Estado,
            ImagenRuta = t.ImagenRuta,
            TutorId = t.TutorId,
            TutorNombre = t.Tutor!.Nombre,
            TutorEspecialidad = t.Tutor!.Especialidad,
            TutorCorreo = t.Tutor!.Correo
        });

    public async Task<List<TesisDto>> ListarAsync() =>
        await Proyectar(db.Tesis.AsNoTracking().OrderBy(t => t.Titulo)).ToListAsync();

    /// <summary>Busca por título, estudiante, estado o nombre del tutor, y devuelve los datos del tutor.</summary>
    public async Task<List<TesisDto>> BuscarAsync(string? texto)
    {
        var consulta = db.Tesis.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            consulta = consulta.Where(x =>
                x.Titulo.Contains(t) ||
                x.Estudiante.Contains(t) ||
                x.Estado.Contains(t) ||
                x.Tutor!.Nombre.Contains(t));
        }

        return await Proyectar(consulta.OrderBy(x => x.Titulo)).ToListAsync();
    }

    public async Task<Resultado<TesisDto>> ObtenerAsync(Guid id)
    {
        var dto = await Proyectar(db.Tesis.AsNoTracking().Where(t => t.Id == id)).FirstOrDefaultAsync();
        return dto is null
            ? Resultado<TesisDto>.Falla(TipoError.NoEncontrado, "La tesis no existe.")
            : Resultado<TesisDto>.Ok(dto);
    }

    public async Task<Resultado<TesisDto>> CrearAsync(TesisGuardarDto dto)
    {
        var titulo = dto.Titulo.Trim();
        var tutorId = dto.TutorId!.Value;

        var error = await ValidarAsync(titulo, tutorId, idActual: null);
        if (error is not null) return error;

        var tesis = new Tesis
        {
            Id = Guid.NewGuid(),
            Titulo = titulo,
            Estudiante = dto.Estudiante.Trim(),
            Estado = dto.Estado,
            TutorId = tutorId
        };

        db.Tesis.Add(tesis);
        await db.SaveChangesAsync();
        return await ObtenerAsync(tesis.Id);
    }

    public async Task<Resultado<TesisDto>> EditarAsync(Guid id, TesisGuardarDto dto)
    {
        var tesis = await db.Tesis.FindAsync(id);
        if (tesis is null)
            return Resultado<TesisDto>.Falla(TipoError.NoEncontrado, "La tesis no existe.");

        var titulo = dto.Titulo.Trim();
        var tutorId = dto.TutorId!.Value;

        var error = await ValidarAsync(titulo, tutorId, idActual: id);
        if (error is not null) return error;

        tesis.Titulo = titulo;
        tesis.Estudiante = dto.Estudiante.Trim();
        tesis.Estado = dto.Estado;
        tesis.TutorId = tutorId;
        await db.SaveChangesAsync();

        return await ObtenerAsync(id);
    }

    public async Task<Resultado<bool>> EliminarAsync(Guid id)
    {
        var tesis = await db.Tesis.FindAsync(id);
        if (tesis is null)
            return Resultado<bool>.Falla(TipoError.NoEncontrado, "La tesis no existe.");

        var rutaVieja = tesis.ImagenRuta;
        db.Tesis.Remove(tesis);
        await db.SaveChangesAsync();
        BorrarArchivo(rutaVieja);

        return Resultado<bool>.Ok(true);
    }

    public async Task<Resultado<TesisDto>> SubirImagenAsync(Guid id, IFormFile? archivo)
    {
        var tesis = await db.Tesis.FindAsync(id);
        if (tesis is null)
            return Resultado<TesisDto>.Falla(TipoError.NoEncontrado, "La tesis no existe.");

        if (archivo is null || archivo.Length == 0)
            return Resultado<TesisDto>.Falla(TipoError.Invalido, "Debe seleccionar una imagen.", "Imagen");

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!Reglas.ExtensionesImagen.Contains(extension))
            return Resultado<TesisDto>.Falla(TipoError.Invalido,
                "Formato no permitido. Use: " + string.Join(", ", Reglas.ExtensionesImagen), "Imagen");

        if (archivo.Length > Reglas.TamanoMaximoImagen)
            return Resultado<TesisDto>.Falla(TipoError.Invalido, "La imagen no puede pesar más de 2 MB.", "Imagen");

        // Guardamos el archivo en wwwroot/imagenes con un nombre único (Guid)
        var carpeta = Path.Combine(RaizWeb(), CarpetaImagenes);
        Directory.CreateDirectory(carpeta);

        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        await using (var stream = new FileStream(Path.Combine(carpeta, nombreArchivo), FileMode.Create))
        {
            await archivo.CopyToAsync(stream);
        }

        var rutaVieja = tesis.ImagenRuta;
        tesis.ImagenRuta = $"/{CarpetaImagenes}/{nombreArchivo}";
        await db.SaveChangesAsync();
        BorrarArchivo(rutaVieja);

        return await ObtenerAsync(id);
    }

    // ------------------ Reglas de negocio ------------------

    private async Task<Resultado<TesisDto>?> ValidarAsync(string titulo, Guid tutorId, Guid? idActual)
    {
        // Regla: no puede duplicarse una tesis (mismo título)
        if (await db.Tesis.AnyAsync(t => t.Titulo == titulo && t.Id != idActual))
            return Resultado<TesisDto>.Falla(TipoError.Conflicto, "Ya existe una tesis con ese título.", "Titulo");

        var tutor = await db.Tutores.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tutorId);
        if (tutor is null)
            return Resultado<TesisDto>.Falla(TipoError.Invalido, "El tutor seleccionado no existe.", "TutorId");

        // Regla: un tutor solo puede tener 3 tesis (sin contar la tesis que estamos editando)
        var cantidad = await db.Tesis.CountAsync(t => t.TutorId == tutorId && t.Id != idActual);
        if (cantidad >= Reglas.MaximoTesisPorTutor)
            return Resultado<TesisDto>.Falla(TipoError.Conflicto,
                $"El tutor {tutor.Nombre} ya tiene {Reglas.MaximoTesisPorTutor} tesis (máximo permitido).", "TutorId");

        return null;
    }

    private string RaizWeb() =>
        env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");

    private void BorrarArchivo(string? rutaRelativa)
    {
        // Las imágenes de muestra (semillas) se comparten, por eso no se borran
        if (string.IsNullOrWhiteSpace(rutaRelativa) || rutaRelativa.StartsWith(PrefijoMuestras)) return;

        var ruta = Path.Combine(RaizWeb(), rutaRelativa.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(ruta)) File.Delete(ruta);
    }
}
