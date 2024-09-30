namespace AS.Application.DTOs.Email;

public class EmailPagoDTO
{
    public required string EmailTo{ get; set; }
    public required string EstadoPago { get; set; }
    public required string NombreTorneo { get; set; }
}
