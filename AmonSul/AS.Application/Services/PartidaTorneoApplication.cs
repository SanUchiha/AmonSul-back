using AS.Application.DTOs.PartidaTorneo;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;

namespace AS.Application.Services;

public class PartidaTorneoApplication : IPartidaTorneoApplication
{
    private readonly IUnitOfWork _unitOfWork;

    public PartidaTorneoApplication(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<bool> Register(PartidaTorneo partidaTorneo)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Delete(int idPartida)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Edit(PartidaTorneo partidaTorneo)
    {
        throw new NotImplementedException();
    }

    public Task<PartidaTorneo> GetById(int idPartida)
    {
        throw new NotImplementedException();
    }

    public Task<List<PartidaTorneo>> GetPartidasTorneo(int idTorneo)
    {
        throw new NotImplementedException();
    }

    public Task<List<PartidaTorneo>> GetPartidasTorneoByRonda(int idTorneo, int ronda)
    {
        throw new NotImplementedException();
    }

    public Task<List<PartidaTorneo>> GetPartidasTorneoByUsuario(int idTorneo, int idUsuario)
    {
        throw new NotImplementedException();
    }

    public Task<List<PartidaTorneo>> GetPartidasTorneos()
    {
        throw new NotImplementedException();
    }

    public Task<List<PartidaTorneo>> GetPartidasTorneosByUsuario(int idUsuario)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> GenerateRound(GenerarRondaDTO generarRondaDTO)
    {
        // Traernos todas las inscripciones del torneo
        List<InscripcionTorneo> inscripciones = await _unitOfWork.InscripcionRepository.GetInscripcionesByTorneo(generarRondaDTO.IdTorneo);
        Torneo torneo = await _unitOfWork.TorneoRepository.GetById(generarRondaDTO.IdTorneo);

        List<Usuario> jugadores = [];

        foreach (var item in inscripciones)
        {
            Usuario jugador = await _unitOfWork.UsuarioRepository.GetById(item.IdUsuario);
            jugadores.Add(jugador);
        }

        // Generar pairing predefinidos
        List<EmparejamientoDTO>? emparejamientos = generarRondaDTO.Emparejamientos ?? [];

        // Generarmos los pairing restantes en base a la configuracion
        // 1. Seleccionamos los jugadores que estan sin emparejar
        List<Usuario> jugadoresSinEmparejar = 
            jugadores.Where(j =>
                !emparejamientos.Any
                    (e => e.Jugador1.IdUsuario == j.IdUsuario || e.Jugador2.IdUsuario == j.IdUsuario))
            .ToList();

        Random random = new();
        // 2. Mezclamos a los jugadores
        jugadoresSinEmparejar = [.. jugadoresSinEmparejar.OrderBy(x => random.Next())];

        // 3. Los emparejamos 
        while (jugadoresSinEmparejar.Count >= 2)
        {
            Usuario jugador1, jugador2;
            bool emparejamientoValido = false;

            do 
            {
                // Seleccionamos dos jugadores aleatorios
                jugador1 = jugadoresSinEmparejar[new Random().Next(jugadoresSinEmparejar.Count)];
                jugador2 = jugadoresSinEmparejar[new Random().Next(jugadoresSinEmparejar.Count)];

                if (jugador1.IdUsuario == jugador2.IdUsuario) continue;
                if (generarRondaDTO.MismaComunidadCheck || jugador1.IdFaccion != jugador2.IdFaccion)
                {
                    emparejamientoValido = true;
                }

            }
            while (!emparejamientoValido);

            // Crear el nuevo emparejamiento
            var nuevoEmparejamiento = new EmparejamientoDTO
            {
                Jugador1 = new JugadorEmparejamientoDTO { IdUsuario = jugador1.IdUsuario, Nick = jugador1.Nick },
                Jugador2 = new JugadorEmparejamientoDTO { IdUsuario = jugador2.IdUsuario, Nick = jugador2.Nick }
            };

            // Añadir el emparejamiento a la lista
            emparejamientos.Add(nuevoEmparejamiento);

            // Remover los jugadores emparejados
            jugadoresSinEmparejar.Remove(jugador1);
            jugadoresSinEmparejar.Remove(jugador2);
        }

        // Crear las partidas
        foreach (var item in emparejamientos)
        {
            PartidaTorneo nuevaPartida = new()
            {
                IdTorneo = generarRondaDTO.IdTorneo,
                IdUsuario1 = item.Jugador1.IdUsuario,
                IdUsuario2 = item.Jugador2.IdUsuario,
                FechaPartida = torneo.FechaInicioTorneo,
                NumeroRonda = generarRondaDTO.IdRonda,
                EsElo = generarRondaDTO.EsEloCheck,
                EsMatchedPlayPartida = torneo.EsMatchedPlayTorneo,
                EscenarioPartida = "",
                PuntosPartida = torneo.PuntosTorneo,

            };

            await _unitOfWork.PartidaTorneoRepository.Register(nuevaPartida);
        }

        return true;
    }
}
