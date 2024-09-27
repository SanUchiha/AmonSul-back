using AS.Application.DTOs.Elo;
using AS.Application.DTOs.PartidaTorneo;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AS.Utils.Statics;
using AutoMapper;

namespace AS.Application.Services;

public class PartidaTorneoApplication(IUnitOfWork unitOfWork, IMapper mapper, IEloApplication eloApplication) : IPartidaTorneoApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IEloApplication _eloApplication = eloApplication;

    public Task<bool> Register(PartidaTorneo partidaTorneo)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Delete(int idPartida)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> Edit(UpdatePartidaTorneoDTO request)
    {
        // Obtener la entidad existente
        PartidaTorneo existingEntity = await _unitOfWork.PartidaTorneoRepository.GetById(request.IdPartidaTorneo);
        if (existingEntity == null) return false;

        // Actualizar las propiedades que no son nulas
        if (request.ResultadoUsuario1.HasValue)
            existingEntity.ResultadoUsuario1 = request.ResultadoUsuario1.Value;

        if (request.ResultadoUsuario2.HasValue)
            existingEntity.ResultadoUsuario2 = request.ResultadoUsuario2.Value;

        if (request.EscenarioPartida != null)
            existingEntity.EscenarioPartida = request.EscenarioPartida;

        if (request.GanadorPartidaTorneo.HasValue)
            existingEntity.GanadorPartidaTorneo = request.GanadorPartidaTorneo.Value;

        if (request.PartidaValidadaUsuario1.HasValue)
            existingEntity.PartidaValidadaUsuario1 = request.PartidaValidadaUsuario1.Value;

        if (request.PartidaValidadaUsuario2.HasValue)
            existingEntity.PartidaValidadaUsuario2 = request.PartidaValidadaUsuario2.Value;

        if (request.LiderMuertoUsuario1.HasValue)
            existingEntity.LiderMuertoUsuario1 = request.LiderMuertoUsuario1.Value;

        if (request.LiderMuertoUsuario2.HasValue)
            existingEntity.LiderMuertoUsuario2 = request.LiderMuertoUsuario2.Value;

        // Guardar los cambios en la base de datos
        bool result = await _unitOfWork.PartidaTorneoRepository.Edit(existingEntity);

        return result;
    }

    public Task<PartidaTorneo> GetById(int idPartida)
    {
        throw new NotImplementedException();
    }

    public async Task<List<PartidaTorneoDTO>> GetPartidasTorneo(int idTorneo)
    {
        List<PartidaTorneo> rawPartidas = await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneo(idTorneo);

        List<PartidaTorneoDTO> partidas = _mapper.Map<List<PartidaTorneoDTO>>(rawPartidas);

        return partidas;
    }

    public async Task<List<PartidaTorneoDTO>> GetPartidasTorneoByRonda(int idTorneo, int ronda)
    {
        List<PartidaTorneo> rawPartidas = await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneoByRonda(idTorneo, ronda);

        List<PartidaTorneoDTO> partidas = _mapper.Map<List<PartidaTorneoDTO>>(rawPartidas);

        return partidas;
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
        //Si es segunda ronda actualizamos el elo de la ronda anterior
        if (generarRondaDTO.IdRonda > 1)
        {
            int numeroRonda = generarRondaDTO.IdRonda;

            // Nos traemos todas las partidas de la ronda
            List<PartidaTorneo> partidas =
                await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneoByRonda(
                    generarRondaDTO.IdTorneo,
                    generarRondaDTO.IdRonda - 1);

            //Actualizamos el elo de los jugadores
            foreach (PartidaTorneo partida in partidas)
            {
                if(partida.GanadorPartidaTorneo == null) continue;

                //1. Actualizar el elo para los jugadores
                int eloJugador1 = await _eloApplication.GetLastElo((int)partida.IdUsuario1!);
                int eloJugador2 = await _eloApplication.GetLastElo((int)partida.IdUsuario2!);

                double scoreGanador = 1.0;
                double scorePerdedor = 0.0;
                double scoreEmpate = 0.5;
                int nuevoEloJugador1 = 800;
                int nuevoEloJugador2 = 800;

                //GanaJugador1
                if (partida.GanadorPartidaTorneo == partida.IdUsuario1)
                {
                    nuevoEloJugador1 = EloRating.CalculateNewRating(eloJugador1, eloJugador2, scoreGanador);
                    nuevoEloJugador2 = EloRating.CalculateNewRating(eloJugador2, eloJugador1, scorePerdedor);
                }
                //GanaJugador2
                if (partida.GanadorPartidaTorneo == partida.IdUsuario2)
                {
                    nuevoEloJugador1 = EloRating.CalculateNewRating(eloJugador1, eloJugador2, scorePerdedor);
                    nuevoEloJugador2 = EloRating.CalculateNewRating(eloJugador2, eloJugador1, scoreGanador);
                }
                //Empate
                if (partida.GanadorPartidaTorneo == 0)
                {
                    nuevoEloJugador1 = EloRating.CalculateNewRating(eloJugador1, eloJugador2, scoreEmpate);
                    nuevoEloJugador2 = EloRating.CalculateNewRating(eloJugador2, eloJugador1, scoreEmpate);
                }

                //Elo jugador 1
                CreateEloDTO createElo1 = new()
                {
                    IdUsuario = (int)partida.IdUsuario1,
                    PuntuacionElo = nuevoEloJugador1
                };
                await _eloApplication.RegisterElo(createElo1);

                //Elo jugador 2
                CreateEloDTO createElo2 = new()
                {
                    IdUsuario = (int)partida.IdUsuario2,
                    PuntuacionElo = nuevoEloJugador2
                };
                await _eloApplication.RegisterElo(createElo2);
            }
        }
    
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

        // 3. Emparejamos
        // Misma comunidad NO
        if (!generarRondaDTO.MismaComunidadCheck)
        {
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
                    if (jugador1.IdFaccion == jugador2.IdFaccion) continue;

                    emparejamientoValido = true;

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

                foreach (var inscripcion in inscripciones)
                {
                    if (inscripcion.IdUsuario == item.Jugador1.IdUsuario)
                        if (inscripcion.Lista.Count != 0)
                            nuevaPartida.EjercitoUsuario1 = inscripcion.Lista.ToList()[0].Ejercito;

                    if (inscripcion.IdUsuario == item.Jugador2.IdUsuario)
                        if (inscripcion.Lista.Count != 0)
                            nuevaPartida.EjercitoUsuario2 = inscripcion.Lista.ToList()[0].Ejercito;
                }

                await _unitOfWork.PartidaTorneoRepository.Register(nuevaPartida);
            }
        }

        // Misma comunidad SI
        else
        {
            // 4. Los emparejamos 
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
                    
                    emparejamientoValido = true;

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

                foreach (var inscripcion in inscripciones)
                {
                    if (inscripcion.IdUsuario == item.Jugador1.IdUsuario)
                        if (inscripcion.Lista.Count != 0)
                            nuevaPartida.EjercitoUsuario1 = inscripcion.Lista.ToList()[0].Ejercito;

                    if (inscripcion.IdUsuario == item.Jugador2.IdUsuario)
                        if (inscripcion.Lista.Count != 0)
                            nuevaPartida.EjercitoUsuario2 = inscripcion.Lista.ToList()[0].Ejercito;
                }

                await _unitOfWork.PartidaTorneoRepository.Register(nuevaPartida);
            }

        }

        return true;
    }
}
