namespace AS.Application.DTOs.Elo
{
    public class ClasificacionElo
    {
        public int IdUsuario { get; set; }
        public required string Nick { get; set; }
        public required string Email { get; set; }
        public int Elo { get; set; }
        public int NumeroPartidas { get; set; }
        public int Victorias { get; set; }
        public int Empates { get; set; }
        public int Derrotas { get; set; }
    }
}
