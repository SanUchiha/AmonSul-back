namespace AS.Domain.DTOs.Elos;

public class UsersElosDTO
{
    public int IdUsuario { get; set; }
    public List<EloPuntuacionDTO> Elos { get; set; } = [];
}
