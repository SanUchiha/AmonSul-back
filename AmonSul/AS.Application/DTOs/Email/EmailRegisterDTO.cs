namespace AS.Application.DTOs.Email;

public class EmailRegisterDTO
{
    public required string EmailTo { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
}
