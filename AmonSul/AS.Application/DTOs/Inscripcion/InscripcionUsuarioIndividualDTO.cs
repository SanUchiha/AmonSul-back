﻿using AS.Domain.DTOs.Torneo;

namespace AS.Application.DTOs.Inscripcion;

public class InscripcionUsuarioIndividualDTO
{
    public int IdInscripcion { get; set; }
    public int IdTorneo { get; set; }
    public string? NombreTorneo { get; set; }
    public int IdUsuario { get; set; }
    public int IdOrganizador { get; set; }
    public string? Nick { get; set; }
    public string? Email { get; set; }
    public DateOnly? FechaInscripcion { get; set; }
    public string? EstadoLista { get; set; }
    public DateOnly? FechaEntregaLista { get; set; }
    public string? EsPago { get; set; }
    public TorneoViewDTO? Torneo { get; set; }
    public int PuntosExtra { get; set; }
}
