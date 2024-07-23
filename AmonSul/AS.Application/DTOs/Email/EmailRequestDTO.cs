namespace AS.Application.DTOs.Email;

public class EmailRequestDTO
{
    public required List<string> EmailTo { get; set; } = [];
    public required string Subject { get; set; }
    public required string Body { get; set; }
}
