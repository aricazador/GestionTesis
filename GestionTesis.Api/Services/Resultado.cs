namespace GestionTesis.Api.Services;

/// <summary>Tipo de error de negocio. El controlador lo traduce a un código HTTP.</summary>
public enum TipoError
{
    Ninguno,
    Invalido,      // 400
    NoAutorizado,  // 401
    Prohibido,     // 403
    NoEncontrado,  // 404
    Conflicto      // 409
}

/// <summary>
/// "Sobre" que devuelve cada servicio: o trae los datos (éxito) o trae el error.
/// Así el servicio no sabe nada de HTTP y el controlador decide el código.
/// </summary>
public class Resultado<T>
{
    public bool Exito => Error == TipoError.Ninguno;
    public T? Datos { get; private init; }
    public TipoError Error { get; private init; }
    public string Mensaje { get; private init; } = string.Empty;
    public string? Campo { get; private init; }

    public static Resultado<T> Ok(T datos) => new() { Datos = datos };

    public static Resultado<T> Falla(TipoError error, string mensaje, string? campo = null) =>
        new() { Error = error, Mensaje = mensaje, Campo = campo };
}
