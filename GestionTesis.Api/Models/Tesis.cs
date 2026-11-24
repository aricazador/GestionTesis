namespace GestionTesis.Api.Models;

/// <summary>Entidad Tesis = una fila de la tabla Tesis.</summary>
public class Tesis
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Estudiante { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;

    /// <summary>Ruta relativa de la imagen guardada en wwwroot/imagenes (ej: /imagenes/abc.jpg).</summary>
    public string? ImagenRuta { get; set; }

    // Llave foránea + navegación hacia el tutor
    public Guid TutorId { get; set; }
    public Tutor? Tutor { get; set; }
}
