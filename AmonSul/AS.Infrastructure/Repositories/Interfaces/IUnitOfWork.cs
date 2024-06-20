namespace AS.Infrastructure.Repositories.Interfaces
{
    public interface IUnitOfWork: IDisposable
    {
        // Declaramos todas las interfaces a nivel de repo
        IAccountRepository AccountRepository { get; }
        IUsuarioRepository UsuarioRepository { get; }
        IFaccionRepository FaccionRepository { get; }
        ITorneoRepository TorneoRepository { get; }

        void SaveChanges();
        Task SaveChangesAsync();
    }
}
