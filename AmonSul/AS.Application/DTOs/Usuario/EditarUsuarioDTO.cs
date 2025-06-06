﻿namespace AS.Application.DTOs.Usuario;

public class EditarUsuarioDTO
{
    public int IdUsuario { get; set; }
    public string? NombreUsuario { get; set; } = null!;
    public string? PrimerApellido { get; set; } = null!;
    public string? SegundoApellido { get; set; }
    public string? Email { get; set; } = null!;
    public string? Nick { get; set; } = null!;
    public string? Ciudad { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public int? IdFaccion { get; set; }
    public string? Telefono { get; set; }
    public string? NickLGDA { get; set; }
    public string? Imagen { get; set; }
}
