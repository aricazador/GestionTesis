namespace GestionTesis.Api.Models;

/// <summary>Entidad Tutor = una fila de la tabla Tutores.</summary>
public class Tutor
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;

    // Navegación: un tutor tiene muchas tesis (máximo 3 por regla de negocio)
    public ICollection<Tesis> Tesis { get; set; } = new List<Tesis>();
}
