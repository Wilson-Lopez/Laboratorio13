namespace Veterinaria.Negocio.Excepciones;

// Se lanza cuando un dato del DTO de entrada no es válido y solo se puede comprobar
// en el negocio (por ejemplo, un propietario que no existe en la base de datos).
// El controlador la convierte en 400 Bad Request (ValidationProblem) indicando el campo.
public class DatoInvalidoException : Exception
{
    public string Campo { get; }

    public DatoInvalidoException(string campo, string mensaje) : base(mensaje)
    {
        Campo = campo;
    }
}
