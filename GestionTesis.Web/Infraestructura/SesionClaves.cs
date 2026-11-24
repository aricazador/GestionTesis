namespace GestionTesis.Web.Infraestructura;

/// <summary>Nombres de las "cajitas" donde guardamos datos en la Session.</summary>
public static class SesionClaves
{
    public const string Token = "Token";
    public const string Usuario = "Usuario";
    public const string Rol = "Rol";
}

/// <summary>Nombre del HttpClient con nombre que apunta a la API.</summary>
public static class ApiCliente
{
    public const string Nombre = "Api";
}
