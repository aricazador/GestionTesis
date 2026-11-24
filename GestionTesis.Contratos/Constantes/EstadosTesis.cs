namespace GestionTesis.Contratos.Constantes;

/// <summary>Estados permitidos para una tesis.</summary>
public static class EstadosTesis
{
    public const string Propuesta = "Propuesta";
    public const string EnDesarrollo = "En desarrollo";
    public const string Aprobada = "Aprobada";
    public const string Rechazada = "Rechazada";

    public static readonly string[] Todos = [Propuesta, EnDesarrollo, Aprobada, Rechazada];

    /// <summary>Expresión regular usada por [RegularExpression] en el DTO.</summary>
    public const string Patron = "^(Propuesta|En desarrollo|Aprobada|Rechazada)$";
}
