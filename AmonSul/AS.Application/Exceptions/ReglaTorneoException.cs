namespace AS.Application.Exceptions;

/// <summary>
/// Un cambio que las reglas del torneo no permiten; el mensaje se le enseña al usuario tal cual
/// </summary>
public class ReglaTorneoException(string message) : Exception(message) { }
