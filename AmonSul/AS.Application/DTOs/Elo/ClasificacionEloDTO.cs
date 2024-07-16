namespace AS.Application.DTOs.Elo
{
    public class ClasificacionEloDTO
    {
        //public int IdUsuario { get; set; }
        public required string Nick { get; set; }
        //public required string Email { get; set; }
        public int Elo { get; set; }
        //public int MejorElo { get; set; }
        //public int PeorElo { get; set; }
        public int Partidas { get; set; }
        public int Ganadas { get; set; }
        public int Empatadas { get; set; }
        public int Perdidas { get; set; }
    }
}
