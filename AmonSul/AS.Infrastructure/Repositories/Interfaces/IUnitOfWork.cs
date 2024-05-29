namespace AS.Infrastructure.Repositories.Interfaces
{
    public interface IUnitOfWork: IDisposable
    {
        // Declaramos todas las interfaces a nivel de repo
        IAccountRepository AccountRepository { get; }
        IUsuarioRepository UsuariosRepository { get; }

        void SaveChanges();
        Task SaveChangesAsync();
    }
}
