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
                .Include(u => u.ClasificacionGenerals)
                .Include(u => u.ClasificacionTorneos)
                .Include(u => u.Comentarios)
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
            var response = await _dbamonsulContext.Usuarios
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
}