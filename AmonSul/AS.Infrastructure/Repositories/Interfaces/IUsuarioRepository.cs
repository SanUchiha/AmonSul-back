using AS.Domain.DTOs.Elos;
using AS.Domain.DTOs.Ganador;
using AS.Domain.DTOs.Usuario;
using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario> GetById(int id);
    Task<Usuario> GetByEmail(string email);
    Task<Usuario> GetByNick(string nick);
    Task<List<Usuario>> GetAll();
    Task<bool> Edit(Usuario usuario);
    Task<bool> Register(Usuario usuario);
    Task<bool> Delete(string email);
    Task<Usuario> GetUsuario(string id);

    Task<UsuarioEmailDto> GetEmailNickById(int id);
    Task<List<string>> GetAllEmail();
    Task<List<Usuario>> GetByIds(List<int> usuarioIds);
    Task<string> GetComunidadNameByIdUser(int idUser);
    Task<List<UsersElosDTO>> GetAllUserWithElo();
    Task<List<GanadorNickDTO>> GetAllSoloNicks();
    Task<List<Usuario>> GetUsuariosNoInscritosTorneoAsync(int idTorneo);
}
