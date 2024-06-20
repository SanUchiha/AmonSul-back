using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;

namespace AS.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbamonsulContext _context;

        public IAccountRepository AccountRepository { get; private set; }
        public IUsuarioRepository UsuarioRepository { get; private set; }
        public IFaccionRepository FaccionRepository { get; private set; }
        public ITorneoRepository TorneoRepository { get; private set; }
        public IPartidaAmistosaRepository PartidaAmistosaRepository{ get; private set; }
        public Utilidades _utilidades;

        public UnitOfWork(DbamonsulContext context, Utilidades utilidades)
        {
            _context = context;
            _utilidades = utilidades;
            AccountRepository = new AccountRepository(context, utilidades);
            UsuarioRepository = new UsuarioRepository(context, utilidades);
            FaccionRepository = new FaccionRepository(context);
            TorneoRepository = new TorneoRepository(context);
            PartidaAmistosaRepository = new PartidaAmistosaRepository(context);
        }

        public void Dispose() => _context.Dispose();
        public void SaveChanges() => _context.SaveChanges();
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
