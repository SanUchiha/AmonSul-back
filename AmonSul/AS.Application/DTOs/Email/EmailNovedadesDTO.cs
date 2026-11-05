namespace AS.Application.DTOs.Email;

public class EmailNovedadesDTO
{
    public required string Asunto { get; set; }
    public required string Html { get; set; }
    // El texto de la notificación, que no admite HTML
    public string? Resumen { get; set; }
    // Solo a quien lo envía, para verlo antes de mandarlo a todos
    public bool SoloAMi { get; set; }
    // Temporal: para probar la notificación sin mandar el correo a todos
    public bool SoloPush { get; set; }
}
