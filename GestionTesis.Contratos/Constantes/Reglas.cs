namespace GestionTesis.Contratos.Constantes;

/// <summary>Reglas de negocio de la consigna.</summary>
public static class Reglas
{
    /// <summary>Un tutor solo puede tener como máximo 3 tesis.</summary>
    public const int MaximoTesisPorTutor = 3;

    /// <summary>Tamaño máximo de imagen: 2 MB.</summary>
    public const long TamanoMaximoImagen = 2 * 1024 * 1024;

    public static readonly string[] ExtensionesImagen = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
}
