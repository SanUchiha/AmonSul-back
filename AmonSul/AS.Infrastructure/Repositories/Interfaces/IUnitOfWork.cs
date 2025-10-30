using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IUnitOfWork: IDisposable
{
    IAccountRepository AccountRepository { get; }
    IUsuarioRepository UsuarioRepository { get; }
    IFaccionRepository FaccionRepository { get; }
    ITorneoRepository TorneoRepository { get; }
    IPartidaAmistosaRepository PartidaAmistosaRepository { get; }
    IEloRepository EloRepository { get; }
    IInscripcionRepository InscripcionRepository{ get; }
    IListaRepository ListaRepository { get; }
    IPartidaTorneoRepository PartidaTorneoRepository { get; }
    IGanadorRepository GanadorRepository { get; }
    ILigaRepository LigaRepository { get; }
    IParticipacionTorneoRepository ParticipacionTorneoRepository { get; }
    IClasificacionEloCacheRepository ClasificacionEloCacheRepository { get; }

    void SaveChanges();
    Task SaveChangesAsync();
}
