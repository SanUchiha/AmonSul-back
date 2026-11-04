namespace AS.Domain.Models;

// El móvil al que mandar avisos: cada instalación de la app tiene su token de Firebase
public class DispositivoPush
{
    public int IdDispositivoPush { get; set; }
    public int IdUsuario { get; set; }
    public required string Token { get; set; }
    public required string Plataforma { get; set; }
    public DateTime FechaCreacion { get; set; }
}
