using AS.Domain.Models;

namespace AS.Application.Interfaces;

public interface IEloLigaApplication
{
    Task SincronizarPartidaAsync(PartidaTorneo partida);
    Task QuitarDeJugadorAsync(int idTorneo, int idUsuario);
}
