namespace AS.Application.DTOs.Notificaciones;

public record ResultadoPushDTO(int Dispositivos, int Entregados, List<string> Errores);
