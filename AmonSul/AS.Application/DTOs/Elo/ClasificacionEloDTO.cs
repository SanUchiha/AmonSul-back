namespace AS.Application.DTOs.Elo
{
    public class ClasificacionEloDTO
    {
        public int IdUsuario { get; set; }
        public required string Nick { get; set; }
        public int Elo { get; set; }
        public int? IdFaccion { get; set; }
    }
}
