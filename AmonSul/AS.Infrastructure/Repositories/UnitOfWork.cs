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
        public Utilidades _utilidades;

        //public IUsuarioRepository UsuariosRepository => throw new NotImplementedException();

        public UnitOfWork(DbamonsulContext context, Utilidades utilidades)
        {
            _context = context;
            _utilidades = utilidades;
            AccountRepository = new AccountRepository(context, utilidades);
            UsuarioRepository = new UsuarioRepository(context, utilidades);
            FaccionRepository = new FaccionRepository(context);
        }

        public void Dispose() => _context.Dispose();
        public void SaveChanges() => _context.SaveChanges();
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
