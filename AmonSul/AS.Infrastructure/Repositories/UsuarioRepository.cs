using AS.Domain.DTOs.Elos;
using AS.Domain.DTOs.Ganador;
using AS.Domain.DTOs.Usuario;
using AS.Domain.Exceptions;
using AS.Domain.Models;
using AS.Infrastructure.Data;
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

    public async Task<bool> EditAsync(Usuario usuario)
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
            IQueryable<Usuario> query = _dbamonsulContext.Usuarios
                .AsNoTracking()              // lectura: menos overhead si no se va a modificar la entidad
                .Where(u => u.IdUsuario == idUsuario)
                .Include(u => u.Elos)
                .Include(u => u.IdFaccionNavigation)
                .Include(u => u.InscripcionTorneos)
                .Include(u => u.PartidaAmistosaGanadorPartidaNavigations)
                .Include(u => u.PartidaAmistosaIdUsuario1Navigations)
                .Include(u => u.PartidaAmistosaIdUsuario2Navigations)
                .Include(u => u.PartidaTorneoIdUsuario1Navigations)
                .Include(u => u.PartidaTorneoIdUsuario2Navigations)
                .Include(u => u.Torneos)
                .AsSplitQuery();

            Usuario? response = await query.FirstOrDefaultAsync();
            return response ?? throw new Exception("Usuario no encontrado");
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al obtener el usuario por ID.", ex);
        }
    }

    /// <summary>
    /// Usuario con solo lo que necesita el detalle de jugador (elos, facción e inscripciones),
    /// sin las partidas ni los torneos organizados con sus bases y cartel
    /// </summary>
    public async Task<Usuario> GetByIdDetalle(int idUsuario)
    {
        try
        {
            Usuario? response = await _dbamonsulContext.Usuarios
                .AsNoTracking()
                .Where(u => u.IdUsuario == idUsuario)
                .Include(u => u.Elos)
                .Include(u => u.IdFaccionNavigation)
                .Include(u => u.InscripcionTorneos)
                .AsSplitQuery()
                .FirstOrDefaultAsync();

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

    /// <summary>
    /// Usuarios solo con los campos de listado, sin Imagen (base64) ni datos personales
    /// </summary>
    public async Task<List<Usuario>> GetAllSinImagen()
    {
        try
        {
            // Sin las cuentas borradas: ya no son jugadores, solo nombres en resultados antiguos
            return await _dbamonsulContext.Usuarios
                .AsNoTracking()
                .Where(u => !u.Email.EndsWith("@" + DOMINIO_BORRADOS))
                .Select(u => new Usuario
                {
                    IdUsuario = u.IdUsuario,
                    Nick = u.Nick,
                    IdFaccion = u.IdFaccion,
                    Ciudad = u.Ciudad,
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al obtener los usuarios.", ex);
        }
    }

    public async Task<List<string>> GetAllEmail()
    {
        try
        {
            List<string> emails = await _dbamonsulContext.Usuarios
                .AsNoTracking()
                .Where(u => !u.Email.EndsWith("@" + DOMINIO_BORRADOS))
                .Select(u => u.Email)
                .ToListAsync();

            return emails ?? [];
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al obtener los emails.", ex);
        }
    }

    public async Task<Usuario> GetByEmail(string email)
    {
        try
        {
            Usuario? response = await _dbamonsulContext.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

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

    public async Task<List<Usuario>> GetUsuariosByIds(List<int> usuarioIds)
    {
        try
        {
            List<Usuario> response = await _dbamonsulContext.Usuarios
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

    public async Task<List<UsuarioSinEquipoDTO>> GetUsuariosNoInscritosTorneoAsync(int idTorneo)
    {
        return await _dbamonsulContext.Usuarios
                .Where(u => !u.InscripcionTorneos.Any(it => it.IdTorneo == idTorneo))
                .Select(u => new UsuarioSinEquipoDTO
                {
                    IdUsuario = u.IdUsuario,
                    Nick = u.Nick
                })
                .ToListAsync();
    }

    public async Task<List<Usuario>> GetUsuariosByTorneo(int idTorneo)
    {
        try
        {
            List<Usuario> usuarios = await _dbamonsulContext.Usuarios
                .Where(u => u.InscripcionTorneos.Any(i => i.IdTorneo == idTorneo))
                .Select(u => new Usuario
                {
                    IdUsuario = u.IdUsuario,
                    NombreUsuario = u.NombreUsuario,
                    PrimerApellido = u.PrimerApellido,
                    SegundoApellido = u.SegundoApellido,
                    Email = u.Email,
                    Contraseña = u.Contraseña,
                    Nick = u.Nick,
                    NickLGDA = u.NickLGDA,
                    Ciudad = u.Ciudad,
                    FechaRegistro = u.FechaRegistro,
                    FechaNacimiento = u.FechaNacimiento,
                    IdFaccion = u.IdFaccion,
                    Telefono = u.Telefono,
                    ProteccionDatos = u.ProteccionDatos,
                    Imagen = u.Imagen,
                    InscripcionTorneos = u.InscripcionTorneos
                        .Where(i => i.IdTorneo == idTorneo)
                        .Select(i => new InscripcionTorneo
                        {
                            IdInscripcion = i.IdInscripcion,
                            IdTorneo = i.IdTorneo,
                            IdUsuario = i.IdUsuario,
                            EstadoInscripcion = i.EstadoInscripcion,
                            FechaInscripcion = i.FechaInscripcion,
                            EstadoLista = i.EstadoLista,
                            FechaEntregaLista = i.FechaEntregaLista,
                            EsPago = i.EsPago,
                            IdEquipo = i.IdEquipo,
                            Lista = i.Lista.ToList() // Se incluye la lista de listas
                        })
                        .ToList()
                })
                .ToListAsync();

            return usuarios;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al obtener los usuarios inscritos en el torneo.", ex);
        }
    }

    /// <summary>
    /// De esos correos, los de quien no quiere recibir avisos
    /// </summary>
    public async Task<List<string>> GetEmailsSinNotificacionesAsync(IReadOnlyCollection<string> emails) =>
        await _dbamonsulContext.Usuarios
            .AsNoTracking()
            .Where(u => !u.PermiteNotificaciones && emails.Contains(u.Email))
            .Select(u => u.Email)
            .ToListAsync();

    public async Task<Usuario> GetByIdFast(int idUsuario) =>
        await _dbamonsulContext.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario)
        ?? throw new Exception("Usuario no encontrado");

    public async Task<Usuario> GetUsuarioSoloById(int idUsuario)
    {
        try
        {
            Usuario? response = await _dbamonsulContext.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
            return response ?? throw new Exception("Usuario no encontrado");
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al obtener el usuario por ID.", ex);
        }
    }

    public async Task<bool> GetProteccionDatos(int idUsuario)
    {
        Usuario? usuario 
            = await _dbamonsulContext.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario) 
            ?? throw new Exception("Usuario no encontrado");
        
        if (usuario.ProteccionDatos == null) return false;
        return true;
    }

    // Guarda el enlace nuevo para recuperar la contraseña, anulando el anterior. False si el correo no existe
    public async Task<bool> GuardarEnlaceContrasenaAsync(string email, string hash, DateTime caducidad) =>
        await _dbamonsulContext.Usuarios
            .Where(u => u.Email == email)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.HashEnlaceContrasena, hash)
                .SetProperty(u => u.CaducidadEnlaceContrasena, caducidad)) > 0;

    // Cambia la contraseña con un enlace vivo y lo gasta en la misma orden: si llegan dos peticiones con el mismo
    // enlace, solo una lo usa. Devuelve el correo de la cuenta, o null si el enlace no vale
    public async Task<string?> UsarEnlaceContrasenaAsync(string hash, string contrasenaCifrada)
    {
        DateTime ahora = DateTime.UtcNow;
        IQueryable<Usuario> conEnlaceVivo = _dbamonsulContext.Usuarios
            .Where(u => u.HashEnlaceContrasena == hash && u.CaducidadEnlaceContrasena > ahora);

        var cuenta = await conEnlaceVivo.Select(u => new { u.IdUsuario, u.Email }).FirstOrDefaultAsync();
        if (cuenta is null) return null;

        int cambiados = await conEnlaceVivo.ExecuteUpdateAsync(s => s
            .SetProperty(u => u.Contraseña, contrasenaCifrada)
            .SetProperty(u => u.HashEnlaceContrasena, (string?)null)
            .SetProperty(u => u.CaducidadEnlaceContrasena, (DateTime?)null));
        if (cambiados == 0) return null;

        await BorrarRefreshTokensAsync(cuenta.IdUsuario);
        return cuenta.Email;
    }

    // Cierra la sesión en todos sus dispositivos: al cambiar la contraseña o borrar la cuenta
    public async Task BorrarRefreshTokensAsync(int idUsuario) =>
        await _dbamonsulContext.RefreshTokens
            .Where(r => r.IdUsuario == idUsuario)
            .ExecuteDeleteAsync();

    public async Task<string?> GetContrasenaAsync(int idUsuario) =>
        await _dbamonsulContext.Usuarios
            .Where(u => u.IdUsuario == idUsuario)
            .Select(u => u.Contraseña)
            .FirstOrDefaultAsync();

    // Lo que impide borrar una cuenta sin dejar a medias a otros: eventos sin terminar en los que está inscrito o que
    // organiza
    public async Task<(bool Inscrito, bool Organiza)> TieneEventosSinTerminarAsync(int idUsuario)
    {
        DateOnly hoy = DateOnly.FromDateTime(DateTime.Today);
        bool inscrito = await _dbamonsulContext.InscripcionTorneos
            .AnyAsync(i => i.IdUsuario == idUsuario && i.IdTorneoNavigation!.FechaFinTorneo >= hoy);
        bool organiza = await _dbamonsulContext.Torneos
            .AnyAsync(t => t.IdUsuario == idUsuario && t.FechaFinTorneo >= hoy);
        return (inscrito, organiza);
    }

    /// <summary>
    /// Borra los datos personales de la cuenta y la deja como «Desconocido N»: la fila se queda para que sus
    /// resultados, partidas y clasificaciones sigan, sin nada que lo identifique. El nick y el correo son únicos,
    /// por eso llevan el id. Nadie puede volver a entrar: la contraseña queda vacía y el correo no existe
    /// </summary>
    public async Task<bool> AnonimizarAsync(int idUsuario)
    {
        string nick = $"Desconocido {idUsuario}";
        string email = $"borrado-{idUsuario}@{DOMINIO_BORRADOS}";

        using var transaccion = await _dbamonsulContext.Database.BeginTransactionAsync();

        int cambiados = await _dbamonsulContext.Usuarios
            .Where(u => u.IdUsuario == idUsuario)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.NombreUsuario, "Desconocido")
                .SetProperty(u => u.PrimerApellido, "")
                .SetProperty(u => u.SegundoApellido, (string?)null)
                .SetProperty(u => u.Email, email)
                .SetProperty(u => u.Contraseña, "")
                .SetProperty(u => u.Nick, nick)
                .SetProperty(u => u.NickLGDA, (string?)null)
                .SetProperty(u => u.Ciudad, (string?)null)
                .SetProperty(u => u.FechaNacimiento, DateOnly.MinValue)
                .SetProperty(u => u.IdFaccion, (int?)null)
                .SetProperty(u => u.Telefono, (string?)null)
                .SetProperty(u => u.ProteccionDatos, (bool?)null)
                .SetProperty(u => u.Imagen, (string?)null)
                .SetProperty(u => u.PermiteNotificaciones, false)
                .SetProperty(u => u.HashEnlaceContrasena, (string?)null)
                .SetProperty(u => u.CaducidadEnlaceContrasena, (DateTime?)null)
                .SetProperty(u => u.NumeroLogins, 0)
                .SetProperty(u => u.UltimoLogin, (DateTime?)null));
        if (cambiados == 0) return false;

        // La clasificación de ELO guarda una copia del nick
        await _dbamonsulContext.ClasificacionEloCache
            .Where(c => c.IdUsuario == idUsuario)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.Nick, nick).SetProperty(c => c.IdFaccion, 0));
        // Su correo ya no existe: ningún organizador lo vería, pero mejor que no quede marcado como compartido
        await _dbamonsulContext.InscripcionTorneos
            .Where(i => i.IdUsuario == idUsuario)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.CompartirEmail, (bool?)null));
        await BorrarRefreshTokensAsync(idUsuario);
        await _dbamonsulContext.DispositivosPush
            .Where(d => d.IdUsuario == idUsuario)
            .ExecuteDeleteAsync();

        await transaccion.CommitAsync();
        return true;
    }

    // Los correos de las cuentas borradas: no existen, a ellos no se manda nada
    public const string DOMINIO_BORRADOS = "amonsul.invalid";
}
