namespace AS.Application.DTOs.Elo
{
    public class ClasificacionElo
    {
        //public int IdUsuario { get; set; }
        public required string Nick { get; set; }
        //public required string Email { get; set; }
        public int Elo { get; set; }
        //public int MejorElo { get; set; }
        //public int PeorElo { get; set; }
        public int Games { get; set; }
        public int Win { get; set; }
        public int Draw { get; set; }
        public int Lost { get; set; }
    }
}
