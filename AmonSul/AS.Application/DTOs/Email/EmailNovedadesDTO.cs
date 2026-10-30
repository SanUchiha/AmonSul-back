namespace AS.Application.DTOs.Email;

public class EmailNovedadesDTO
{
    public required string Asunto { get; set; }
    public required string Html { get; set; }
    // Solo a quien lo envía, para verlo antes de mandarlo a todos
    public bool SoloAMi { get; set; }
}
