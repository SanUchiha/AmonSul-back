using AS.Application.DTOs.Torneo;
using AS.Domain.DTOs.Equipo;
using AS.Domain.DTOs.Inscripcion;
using AS.Domain.Models;
using AS.Infrastructure.Data;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

public class InscripcionRepository(DbamonsulContext dbamonsulContext) : IInscripcionRepository
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;

    public async Task<bool> AddUsuarioToEquipoAsync(EquipoUsuario equipoUsuario)
    {
        _dbamonsulContext.EquipoUsuario.Add(equipoUsuario);
        return await _dbamonsulContext.SaveChangesAsync() > 0;
    }

    public Task<bool> CambiarEstadoInscripcion(InscripcionTorneo actualizarEstadoInscripcion)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> CambiarEstadoLista(int idInscripcion, string estadoLista)
    {
        InscripcionTorneo? inscripcion = 
            await _dbamonsulContext.InscripcionTorneos
                .FirstOrDefaultAsync(i => i.IdInscripcion == idInscripcion);
        if(inscripcion == null) return false;

        inscripcion.EstadoLista = estadoLista;

        _dbamonsulContext.InscripcionTorneos.Update(inscripcion);
        await _dbamonsulContext.SaveChangesAsync();
        return true;
    }

    public Task<bool> CambiarEstadoPago(InscripcionTorneo actualizarEstadoPago)
    {
        throw new NotImplementedException();
    }

    public async Task<Equipo> CreateEquipoAsync(Equipo equipo)
    {
        _dbamonsulContext.Equipo.Add(equipo);
        await _dbamonsulContext.SaveChangesAsync();
        return equipo;
    }

    public async Task<InscripcionTorneo> Delete(int idInscripcion)
    {
        var inscripcion = await _dbamonsulContext.InscripcionTorneos
                                                 .Include(i => i.Lista)
                                                 .FirstOrDefaultAsync(i => i.IdInscripcion == idInscripcion);
        if (inscripcion == null) return null!;
        // Eliminar participaciones relacionadas para evitar violación de FK
        var participaciones = _dbamonsulContext.ParticipacionTorneos
            .Where(p => p.IdInscripcion == inscripcion.IdInscripcion);
        _dbamonsulContext.ParticipacionTorneos.RemoveRange(participaciones);

        _dbamonsulContext.InscripcionTorneos.Remove(inscripcion);
        await _dbamonsulContext.SaveChangesAsync();
        return inscripcion;
    }

    public async Task<bool> DeleteEquipoAsync(int idEquipo)
    {
        Equipo? equipo = await GetEquipoByIdAsync(idEquipo);
        if (equipo == null) return false;

        try
        {
            using var transaction = await _dbamonsulContext.Database.BeginTransactionAsync();

            foreach (var inscripcion in equipo.InscripcionTorneos)
            {
                // Eliminar participaciones relacionadas con esta inscripción
                var participaciones = _dbamonsulContext.ParticipacionTorneos
                    .Where(p => p.IdInscripcion == inscripcion.IdInscripcion);
                _dbamonsulContext.ParticipacionTorneos.RemoveRange(participaciones);

                _dbamonsulContext.Listas.RemoveRange(inscripcion.Lista);
            }
            _dbamonsulContext.InscripcionTorneos.RemoveRange(equipo.InscripcionTorneos);
            _dbamonsulContext.EquipoUsuario.RemoveRange(equipo.Miembros);
            _dbamonsulContext.Equipo.Remove(equipo);

            await _dbamonsulContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch (DbUpdateException dbEx)
        {
            Console.WriteLine($"Error de base de datos: {dbEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error general: {ex.Message}");
        }
        return false;
    }

    public async Task<bool> DeleteMiembroAsync(int idInscripcion)
    {
        InscripcionTorneo inscripcion = await GetInscripcionById(idInscripcion);
        if (inscripcion == null) return false; 

        EquipoUsuario? miembro = await GetMiembroEquipoAsync(inscripcion.IdEquipo, inscripcion.IdUsuario);
        if (miembro == null) return false;

        try
        {
            using var transaction = await _dbamonsulContext.Database.BeginTransactionAsync();

            _dbamonsulContext.Listas.RemoveRange(inscripcion.Lista);            
            // Eliminar participaciones relacionadas con esta inscripción
            var participaciones = _dbamonsulContext.ParticipacionTorneos
                .Where(p => p.IdInscripcion == inscripcion.IdInscripcion);
            _dbamonsulContext.ParticipacionTorneos.RemoveRange(participaciones);

            _dbamonsulContext.InscripcionTorneos.Remove(inscripcion);
            _dbamonsulContext.EquipoUsuario.Remove(miembro);

            await _dbamonsulContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch (DbUpdateException dbEx)
        {
            Console.WriteLine($"Error de base de datos: {dbEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error general: {ex.Message}");
        }
        return false;
    }

    private async Task<EquipoUsuario?> GetMiembroEquipoAsync(int? idEquipo, int idUsuario) => 
        await _dbamonsulContext.EquipoUsuario
            .FirstOrDefaultAsync(x => x.IdEquipo == idEquipo && x.IdUsuario == idUsuario);

    public async Task<bool> EstaApuntadoAsync(int idUsuario, int idTorneo)
    {
        InscripcionTorneo? inscripcionTorneo = await _dbamonsulContext.InscripcionTorneos
            .FirstOrDefaultAsync(it => it.IdTorneo == idTorneo && it.IdUsuario == idUsuario);

        if (inscripcionTorneo == null) return false;
        return true;
    }

    public async Task<List<EquipoDTO>> GetAllEquiposByTorneoAsync(int idTorneo) =>
        await _dbamonsulContext.Equipo
            .Where(e => e.InscripcionTorneos.Any(i => i.IdTorneo == idTorneo))
            .Select(e => new
            {
                e.IdEquipo,
                e.NombreEquipo,
                e.IdCapitan,
                Inscripciones = e.InscripcionTorneos.Where(i => i.IdTorneo == idTorneo)
                .Select(i => new
                {
                    i.IdInscripcion,
                    i.IdTorneo,
                    i.IdUsuario,
                    i.FechaInscripcion,
                    i.EstadoLista,
                    i.FechaEntregaLista,
                    i.EsPago,
                    i.IdUsuarioNavigation!.Nick,
                    Lista = i.Lista.FirstOrDefault()
                }).ToList()
            })
            .ToListAsync()
            .ContinueWith(task => task.Result.Select(e => new EquipoDTO
            {
                IdEquipo = e.IdEquipo,
                NombreEquipo = e.NombreEquipo,
                IdCapitan = e.IdCapitan,
                Inscripciones = [.. e.Inscripciones.Select(i => new InscripcionTorneoDTO
                {
                    IdInscripcion = i.IdInscripcion,
                    IdTorneo = i.IdTorneo,
                    IdUsuario = i.IdUsuario,
                    FechaInscripcion = i.FechaInscripcion,
                    EstadoLista = i.EstadoLista,
                    FechaEntregaLista = i.FechaEntregaLista,
                    EsPago = i.EsPago,
                    Nick = i.Nick,
                    IdLista = i.Lista != null ? i.Lista.IdLista : 0,
                    Ejercito = i.Lista != null ? i.Lista.Ejercito ?? null : null
                })]
            }).ToList());

    public async Task<List<InscripcionTorneo>> GetAllInscripcionesByEquipoAsync(int idEquipo)
    {
        List<InscripcionTorneo> insc = await _dbamonsulContext.InscripcionTorneos
            .Include(it => it.Lista)
            .Include(it => it.IdUsuarioNavigation)
            .Where(it => it.IdEquipo == idEquipo)
            .ToListAsync();

        return insc!;
    }

    public async Task<Equipo?> GetEquipoByIdAsync(int id) => 
        await _dbamonsulContext.Equipo
            .Include(e => e.Miembros)
            .Include(e => e.InscripcionTorneos)
                    .ThenInclude(i => i.Lista)
            .FirstOrDefaultAsync(e => e.IdEquipo == id);

    /// <summary>
    /// Equipo del jugador en el torneo, con sus inscripciones y listas; null si no juega en ningún equipo
    /// </summary>
    public async Task<Equipo?> GetEquipoByUsuarioAndTorneoAsync(int idUsuario, int idTorneo)
    {
        int? idEquipo = await _dbamonsulContext.InscripcionTorneos
            .Where(it => it.IdUsuario == idUsuario && it.IdTorneo == idTorneo && it.IdEquipo != null)
            .Select(it => it.IdEquipo)
            .FirstOrDefaultAsync();

        return idEquipo is null ? null : await GetEquipoByIdAsync(idEquipo.Value);
    }

    /// <summary>
    /// Solo torneo y jugador de la inscripción, sin listas ni navegaciones
    /// </summary>
    public async Task<InscripcionTorneo?> GetTorneoYUsuario(int idInscripcion) =>
        await _dbamonsulContext
            .InscripcionTorneos.AsNoTracking()
            .Where(it => it.IdInscripcion == idInscripcion)
            .Select(it => new InscripcionTorneo
            {
                IdInscripcion = it.IdInscripcion,
                IdTorneo = it.IdTorneo,
                IdUsuario = it.IdUsuario,
            })
            .FirstOrDefaultAsync();

    public async Task<InscripcionTorneo> GetInscripcionById(int idInscripcion)
    {
        var insc = await _dbamonsulContext.InscripcionTorneos
            .Include(it => it.Lista)
            .Include(it => it.IdTorneoNavigation)
            .FirstOrDefaultAsync(it => it.IdInscripcion == idInscripcion);

        return insc!;
    }

    public async Task<List<InscripcionTorneo>> GetInscripciones()
    {
        return await _dbamonsulContext.InscripcionTorneos.ToListAsync();
    }

    public async Task<List<InscripcionTorneo>> GetInscripcionesByTorneo(int idTorneo) =>
        await _dbamonsulContext.InscripcionTorneos
            .Include(it => it.IdUsuarioNavigation) // Incluye el usuario relacionado
            .Include(it => it.Lista)              // Incluye la lista relacionada
            .Where(it => it.IdTorneo == idTorneo) // Filtra por el torneo
            .ToListAsync();

    public async Task<List<InscripcionTorneo>> GetInscripcionesEquipoByUser(int idUsuario) => 
        await _dbamonsulContext.InscripcionTorneos
                .Include(it => it.IdTorneoNavigation)
                .Include(it => it.IdUsuarioNavigation)
                .Where(it => it.IdUsuario == idUsuario && it.IdEquipo != null)
                .ToListAsync();

    public async Task<List<InscripcionTorneo>> GetInscripcionesIndividualByUser(int idUsuario) => 
        await _dbamonsulContext.InscripcionTorneos
                .Include(it => it.IdTorneoNavigation)
                .Include(it => it.IdUsuarioNavigation)
                .Where(it => it.IdUsuario == idUsuario && it.IdEquipo == null)
                .ToListAsync();

    public async Task<ResultInscripcionTorneoDTO> Register(InscripcionTorneo inscripcionTorneo)
    {
        try
        {
            _dbamonsulContext.InscripcionTorneos.Add(inscripcionTorneo);
            await _dbamonsulContext.SaveChangesAsync();

            return new ResultInscripcionTorneoDTO
            {
                Result = true,
                IdInscripcion = inscripcionTorneo.IdInscripcion
            };
        }
        catch (DbUpdateException ex)
        {
            return new ResultInscripcionTorneoDTO
            {
                Result = false,
                Mensaje = ex.ToString(),
            };
        }
    }

    /// <summary>
    /// Solo el ejército elegido: Update marcaría también el torneo cargado y lo reescribiría entero
    /// </summary>
    public async Task<bool> ActualizarEjercitoElegidoAsync(int idInscripcion, string ejercito)
    {
        InscripcionTorneo? inscripcion = await _dbamonsulContext.InscripcionTorneos.FirstOrDefaultAsync(i =>
            i.IdInscripcion == idInscripcion
        );
        if (inscripcion is null)
            return false;

        inscripcion.EjercitoElegido = ejercito;
        await _dbamonsulContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Solo el grupo de cada inscripción, en un solo guardado
    /// </summary>
    public async Task ActualizarGruposAsync(IReadOnlyDictionary<int, int> grupoPorInscripcion)
    {
        List<int> ids = [.. grupoPorInscripcion.Keys];
        List<InscripcionTorneo> inscripciones = await _dbamonsulContext.InscripcionTorneos
            .Where(i => ids.Contains(i.IdInscripcion))
            .ToListAsync();
        foreach (InscripcionTorneo inscripcion in inscripciones)
            inscripcion.Grupo = grupoPorInscripcion[inscripcion.IdInscripcion];
        await _dbamonsulContext.SaveChangesAsync();
    }

    /// <summary>
    /// El hueco en el cuadro de cada inscripción del torneo: a las que no vienen se les quita
    /// </summary>
    public async Task ActualizarPosicionesCuadroAsync(int idTorneo, IReadOnlyDictionary<int, int> posicionPorInscripcion)
    {
        List<InscripcionTorneo> inscripciones = await _dbamonsulContext.InscripcionTorneos
            .Where(i => i.IdTorneo == idTorneo)
            .ToListAsync();
        foreach (InscripcionTorneo inscripcion in inscripciones)
            inscripcion.PosicionCuadro = posicionPorInscripcion.TryGetValue(inscripcion.IdInscripcion, out int posicion)
                ? posicion
                : null;
        await _dbamonsulContext.SaveChangesAsync();
    }

    /// <summary>
    /// Solo el estado, sin tocar sus partidas
    /// </summary>
    public async Task ActualizarEstadoInscripcionAsync(int idInscripcion, string estado)
    {
        InscripcionTorneo? inscripcion = await _dbamonsulContext.InscripcionTorneos.FirstOrDefaultAsync(i =>
            i.IdInscripcion == idInscripcion
        );
        if (inscripcion is null)
            return;
        inscripcion.EstadoInscripcion = estado;
        await _dbamonsulContext.SaveChangesAsync();
    }

    /// <summary>
    /// Borra todas las partidas del jugador en el torneo y cambia el estado de su inscripción en un solo guardado,
    /// para que no quede a medias si algo falla
    /// </summary>
    public async Task<bool> RetirarAsync(int idInscripcion, string estado)
    {
        InscripcionTorneo? inscripcion = await _dbamonsulContext.InscripcionTorneos.FirstOrDefaultAsync(i =>
            i.IdInscripcion == idInscripcion
        );
        if (inscripcion is null)
            return false;

        List<PartidaTorneo> partidas = await _dbamonsulContext
            .PartidaTorneos.Where(p =>
                p.IdTorneo == inscripcion.IdTorneo
                && (p.IdUsuario1 == inscripcion.IdUsuario || p.IdUsuario2 == inscripcion.IdUsuario)
            )
            .ToListAsync();

        _dbamonsulContext.PartidaTorneos.RemoveRange(partidas);
        inscripcion.EstadoInscripcion = estado;
        await _dbamonsulContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Update(InscripcionTorneo inscripcionTorneo)
    {
        try
        {
            _dbamonsulContext.InscripcionTorneos.Update(inscripcionTorneo);
            await _dbamonsulContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new Exception("La inscripción no se pudo actualizar porque fue modificado por otro usuario.");
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al actualizar la inscripción.", ex);
        }
    }

    public Task<List<InscripcionTorneoEmparejamientoDTO>> GetInscripcionesByEquipoIdAsync(int idEquipo)
    {
        throw new NotImplementedException();
    }

    public async Task<int?> GetIdEquipoByIdUsuarioAndIdTorneoAsync(int idUsuario, int idTorneo) =>
        await _dbamonsulContext.InscripcionTorneos
            .Where(it => it.IdUsuario == idUsuario && it.IdEquipo != null && it.IdTorneo == idTorneo)
            .Select(it => it.IdEquipo!.Value)
            .FirstAsync();

    public async Task<List<Equipo>> GetEquiposDisponiblesAsync(int idTorneo)
    {
        List<InscripcionTorneo> inscripcionTorneos = await _dbamonsulContext.InscripcionTorneos
            .Where(it => it.IdTorneo == idTorneo)
            .Include(i=>i.IdEquipoNavigation)
            .ToListAsync();

        List<InscripcionTorneo> inscripcionesUnicas = [.. inscripcionTorneos
            .GroupBy(i => i.IdEquipo)
            .Select(g => g.First())];

        if (inscripcionesUnicas.Count <= 0) return [];

        List<Equipo> equipos = [];

        foreach (var item in inscripcionesUnicas)
        {
            Equipo equipo = new()
            {
                IdEquipo = item.IdEquipoNavigation!.IdEquipo,
                NombreEquipo = item.IdEquipoNavigation.NombreEquipo,
            };

            equipos.Add(equipo);
        }

        return equipos;
    }
}
