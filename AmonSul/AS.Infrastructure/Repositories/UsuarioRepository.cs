using AS.Domain.DTOs.Elos;
using AS.Domain.DTOs.Ganador;
using AS.Domain.DTOs.Torneo;
using AS.Domain.DTOs.Usuario;
using AS.Domain.Exceptions;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

public class UsuarioRepository(DbamonsulContext dbamonsulContext) : IUsuarioRepository
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;

    public async Task<bool> Register(Usuario usuario)
    {
        try
        {
            await _dbamonsulContext.AddAsync(usuario);
            await _dbamonsulContext.SaveChangesAsync();
            return true;
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            if (ex.Message.Contains("UQ__Usuario__7D3471B6867373C0"))
            {
                throw new UniqueKeyViolationException("El nick ya está en uso.", "Nick");
            }
            else if (ex.Message.Contains("UQ__Usuario__A9D105344761A97B"))
            {
                throw new UniqueKeyViolationException("El correo electrónico ya está en uso.", "Email");
            }
            throw new UniqueKeyViolationException("Infracción de la restricción UNIQUE KEY.", "Unknown");
        }
        catch (Exception ex)
        {
            if (ex.InnerException!.Message.Contains("UQ__Usuario__7D3471B6867373C0"))
            {
                throw new UniqueKeyViolationException("El nick ya está en uso.", "Nick");
            }
            else if (ex.InnerException.Message.Contains("UQ__Usuario__A9D105344761A97B"))
            {
                throw new UniqueKeyViolationException("El correo electrónico ya está en uso.", "Email");
            }
            throw new Exception("Ocurrio un problema en el servidor.");
        }
    }

    public async Task<bool> Edit(Usuario usuario)
    {
        try
        {
            _dbamonsulContext.Usuarios.Update(usuario);
            await _dbamonsulContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new Exception("El usuario no se pudo actualizar porque fue modificado por otro usuario.");
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al actualizar el usuario.", ex);
        }
    }

    public async Task<Usuario> GetById(int idUsuario)
    {
        try
        {
            var response = await _dbamonsulContext.Usuarios
                .Include(u => u.Elos)
                .Include(u => u.IdFaccionNavigation)
                .Include(u => u.InscripcionTorneos)
                .Include(u => u.PartidaAmistosaGanadorPartidaNavigations)
                .Include(u => u.PartidaAmistosaIdUsuario1Navigations)
                .Include(u => u.PartidaAmistosaIdUsuario2Navigations)
                .Include(u => u.PartidaTorneoIdUsuario1Navigations)
                .Include(u => u.PartidaTorneoIdUsuario2Navigations)
                .Include(u => u.Torneos)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
            return response ?? throw new Exception("Usuario no encontrado");
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al obtener el usuario por ID.", ex);
        }
    }

    public async Task<UsuarioEmailDto> GetEmailNickById(int idUsuario)
    {
        try
        {
            UsuarioEmailDto? response = await _dbamonsulContext.Usuarios
                .Where(u => u.IdUsuario == idUsuario)
                .Select(u => new UsuarioEmailDto
                {
                    Email = u.Email,
                    Nick = u.Nick
                })
                .FirstOrDefaultAsync();

            return response ?? throw new Exception("Usuario no encontrado");
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al obtener el usuario por ID.", ex);
        }
    }

    public async Task<List<Usuario>> GetAll()
    {
        try
        {
            var usuarios = await _dbamonsulContext.Usuarios
                .ToListAsync();

            if (usuarios.Count == 0) return [];

            return usuarios;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al obtener el usuario por ID.", ex);
        }
    }

    public async Task<List<string>> GetAllEmail()
    {
        try
        {
            List<string> emails = await _dbamonsulContext.Usuarios
                .AsNoTracking()
                .Select(u => u.Email)
                .ToListAsync();

            return emails ?? [];
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al obtener los emails.", ex);
        }
    }

    public async Task<bool> Delete(string email)
    {
        try
        {
            var usuarioEncontrado = await this.GetByEmail(email) ?? throw new Exception("Ocurrió un problema al eliminar el usuario.");
            _dbamonsulContext.Usuarios.Remove(usuarioEncontrado);
            await _dbamonsulContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al eliminar el usuario.", ex);
        }
    }

    public async Task<Usuario> GetByEmail(string email)
    {
        try
        {
            var response = await _dbamonsulContext.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            if (response == null) return null!;
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception($"{ex.Message}");
        }
    }

    public async Task<Usuario> GetByNick(string nick)
    {
        try
        {
            var response = await _dbamonsulContext.Usuarios.FirstOrDefaultAsync(u => u.Nick == nick);
            if (response == null) return null!;
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception($"{ex.Message}");
        }
    }

    public async Task<Usuario> GetUsuario(string email)
    {
        try
        {
            var response = await _dbamonsulContext.Usuarios.FirstOrDefaultAsync(u => u.Email== email);
            if (response == null) return null!;
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception($"{ex.Message}");
        }
    }

    public async Task<List<Usuario>> GetByIds(List<int> usuarioIds)
    {
        try
        {
            var response = await _dbamonsulContext.Usuarios
                .Include(u => u.Elos)
                .Include(u => u.IdFaccionNavigation)
                .Include(u => u.InscripcionTorneos)
                .Include(u => u.PartidaAmistosaGanadorPartidaNavigations)
                .Include(u => u.PartidaAmistosaIdUsuario1Navigations)
                .Include(u => u.PartidaAmistosaIdUsuario2Navigations)
                .Include(u => u.PartidaTorneoIdUsuario1Navigations)
                .Include(u => u.PartidaTorneoIdUsuario2Navigations)
                .Include(u => u.Torneos)
                .Where(u => usuarioIds.Contains(u.IdUsuario))
                .ToListAsync();

            return response ?? throw new Exception("No se encontraron usuarios.");
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al obtener los usuarios por IDs.", ex);
        }
    }

    public async Task<string> GetComunidadNameByIdUser(int idUser)
    {
        try
        {
            Usuario? user = await _dbamonsulContext.Usuarios
                .Include(u => u.IdFaccionNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUser);

            if (user!.IdFaccion == null) return "";

            string response = user.IdFaccionNavigation!.NombreFaccion;

            return response;
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Ocurrió un problema al obtener el nombre de la facción por idUsuario.", ex);
        }
    }

    public async Task<List<UsersElosDTO>> GetAllUserWithElo()
    {
        try
        {
            List<UsersElosDTO> result = 
                await _dbamonsulContext.Usuarios
                    .Select(u => new UsersElosDTO
                    {
                        IdUsuario = u.IdUsuario,
                        Elos= u.Elos
                            .Select(e => new EloPuntuacionDTO 
                            {
                                PuntuacionElo = e.PuntuacionElo,
                                FechaRegistroElo = e.FechaElo,
                            }).ToList()                     
                    })
                    .ToListAsync();

            return result;
                
        }
        catch (Exception ex) 
        {
            throw new Exception(
               "Ocurrió un problema al obtener el elo de los usuarios.", ex);
        }
    }

    public async Task<List<GanadorNickDTO>> GetAllSoloNicks()
    {
        try
        {
            var usuarios = await _dbamonsulContext.Usuarios
                .Select(u => new GanadorNickDTO
                {
                    IdUsuario = u.IdUsuario,
                    Nick = u.Nick
                })
                .ToListAsync();

            if (usuarios.Count == 0) return [];

            return usuarios;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al obtener el nick de todos los usuarios.", ex);
        }
    }
}