namespace AS.Application.DTOs.Elo
{
    public class ViewEloDTO
    {
        public int IdUsuario { get; set; }
        public required string Nick { get; set; }
        public List<EloDTO> Elos { get; set; } = [];
    }
}
