namespace AS.Application.DTOs.Email;

public class EmailSettings
{
    public required string SmtpServer { get; set; }
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string From { get; set; }
    public List<string> EmailsExcluidos { get; set; } = [];

    /// <summary>
    /// Interruptor general de los correos de aviso (nuevo torneo, rondas, inscripciones, pagos, listas, partidas,
    /// bienvenida y novedades). Apagado, no sale ninguno; las notificaciones push no dependen de esto. Los correos
    /// de la cuenta (recuperar la contraseña, aviso de privacidad y contacto) salen siempre. Si falta en la
    /// configuración, queda apagado: para encenderlos hay que poner AvisosActivados a true
    /// </summary>
    public bool AvisosActivados { get; set; } = false;
}
