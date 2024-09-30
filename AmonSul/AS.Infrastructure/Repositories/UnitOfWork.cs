using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;

namespace AS.Infrastructure.Repositories;

public class UnitOfWork(DbamonsulContext context, Utilidades utilidades) : IUnitOfWork
{
    private readonly DbamonsulContext _context = context;

    public IAccountRepository AccountRepository { get; private set; } = new AccountRepository(context, utilidades);
    public IUsuarioRepository UsuarioRepository { get; private set; } = new UsuarioRepository(context);
    public IFaccionRepository FaccionRepository { get; private set; } = new FaccionRepository(context);
    public ITorneoRepository TorneoRepository { get; private set; } = new TorneoRepository(context);
    public IPartidaAmistosaRepository PartidaAmistosaRepository { get; private set; } = new PartidaAmistosaRepository(context);
    public IInscripcionRepository InscripcionRepository { get; private set; } = new InscripcionRepository(context);
    public IListaRepository ListaRepository { get; private set; } = new ListaRepository(context);
    public IEloRepository EloRepository { get; private set; } = new EloRepository(context);
    public IPartidaTorneoRepository PartidaTorneoRepository { get; private set; } = new PartidaTorneoRepository(context);
    public IGanadorRepository GanadorRepository { get; private set; } = new GanadorRepository(context);
    public Utilidades _utilidades = utilidades;

    public void Dispose() => _context.Dispose();
    public void SaveChanges() => _context.SaveChanges();
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
