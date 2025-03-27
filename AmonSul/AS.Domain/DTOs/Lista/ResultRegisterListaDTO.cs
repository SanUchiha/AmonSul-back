namespace AS.Domain.DTOs.Lista;

public class ResultRegisterListarDTO
{
    public bool Result { get; set; }
    public string? Mensaje { get; set; } = string.Empty;
    public int? IdLista { get; set; }
}
