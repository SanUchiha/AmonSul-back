using AS.Application.DTOs.Elo;
using AS.Application.DTOs.Inscripcion;
using AS.Application.DTOs.PartidaTorneo;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Inscripcion;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AS.Utils.Statics;
using AutoMapper;

namespace AS.Application.Services;

public class PartidaTorneoApplication(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IEloApplication eloApplication,
    IEmailApplicacion emailApplicacion) : IPartidaTorneoApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IEloApplication _eloApplication = eloApplication;
    private readonly IEmailApplicacion _emailApplicacion = emailApplicacion;

    public async Task<bool> Register(AddPairingTorneoDTO addPairingTorneoDTO)
    {
        PartidaTorneo partida = _mapper.Map<PartidaTorneo>(addPairingTorneoDTO);
        
        bool response = 
            await _unitOfWork.PartidaTorneoRepository.Register(partida);

        return response;
    }
        
    public async Task<bool> Delete(int idPartida) =>
        await _unitOfWork.PartidaTorneoRepository.Delete(idPartida);

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

        if (request.FechaPartida is null)
            existingEntity.FechaPartida = DateOnly.FromDateTime(DateTime.Now);

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

    public async Task<List<ViewPartidaTorneoDTO>> GetPartidasTorneosByUsuario(int idUsuario)
    {
        List<PartidaTorneo> partidasRaw = await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneosByUsuario(idUsuario);

        List<ViewPartidaTorneoDTO> response = _mapper.Map<List<ViewPartidaTorneoDTO>>(partidasRaw);

        foreach (var partida in response)
        {
            partida.NickUsuario1 = partida.IdUsuario1Navigation!.Nick;
            partida.NickUsuario2 = partida.IdUsuario2Navigation!.Nick;

            if (partida.GanadorPartidaTorneo != 0)
            {
                if (partida.GanadorPartidaTorneo == partida.IdUsuario2) partida.GanadorPartidaNick = partida.NickUsuario2;
                else if(partida.GanadorPartidaTorneo == partida.IdUsuario1) partida.GanadorPartidaNick = partida.NickUsuario1;
                else partida.GanadorPartidaNick = null;
            }
        }
        return response;
    }

    public async Task<bool> GenerateRound(GenerarRondaDTO generarRondaDTO)
    {
        await ActualizarEloAsync(generarRondaDTO);
        
        // Traer todas las inscripciones del torneo
        List<InscripcionTorneo> inscripciones = 
            await _unitOfWork.InscripcionRepository.GetInscripcionesByTorneo(generarRondaDTO.IdTorneo);
        Torneo torneo = await _unitOfWork.TorneoRepository.GetById(generarRondaDTO.IdTorneo);

        List<Usuario> jugadores = [];
        List<string> destinatarios = [];

        List<int> usuarioIds = 
            inscripciones.Select(i => i.IdUsuario).Distinct().ToList();

        List<Usuario> usuarios = 
            await _unitOfWork.UsuarioRepository.GetUsuariosByIds(usuarioIds);

        foreach (var usuario in usuarios)
        {
            jugadores.Add(usuario);
            destinatarios.Add(usuario.Email);
        }

        // Generar pairing predefinidos
        List<EmparejamientoDTO>? emparejamientos = generarRondaDTO.Emparejamientos ?? [];
        List<EmparejamientoDTO> emparejamientosAleatorios = [];

        // Generar otras rondas
        if (generarRondaDTO.IdRonda > 1)
            await GenerarOtrasRondasAsync(
                generarRondaDTO, 
                torneo, 
                emparejamientos, 
                inscripciones);

        // Generar primera ronda    
        else await GenerarPrimeraRondaAsync(
                generarRondaDTO,
                torneo,
                emparejamientos,
                emparejamientosAleatorios,
                inscripciones,
                jugadores);

        if (destinatarios.Count > 0)
            _ = Task.Run(() => _emailApplicacion.SendEmailRonda(
                torneo.NombreTorneo!,
                generarRondaDTO.IdRonda,
                destinatarios));

        return true;
    }

    private async Task GenerarPrimeraRondaAsync(
        GenerarRondaDTO generarRondaDTO, 
        Torneo torneo, 
        List<EmparejamientoDTO> emparejamientos, 
        List<EmparejamientoDTO> emparejamientosAleatorios, 
        List<InscripcionTorneo> inscripciones,
        List<Usuario> jugadores)
    {
        List<Usuario> jugadoresSinEmparejar =
            jugadores.Where(j =>
                !emparejamientos.Any
                    (e => e.Jugador1.IdUsuario == j.IdUsuario || e.Jugador2.IdUsuario == j.IdUsuario))
            .ToList();

        Random random = new();
        // 2. Mezclamos a los jugadores
        jugadoresSinEmparejar = [.. jugadoresSinEmparejar.OrderBy(x => random.Next())];
        List<EmparejamientoDTO> listaEmparejadosLuzVsOscuridad = [];

        // Luz vs Oscuridad SI
        if (generarRondaDTO.LuzVsOscCheck)
        {
            listaEmparejadosLuzVsOscuridad = 
                await GenerarPrimeraRondaLuzVsOscuridadAsync(
                    torneo,
                    random,
                    jugadoresSinEmparejar,
                    inscripciones,
                    generarRondaDTO,
                    emparejamientos);
        }
        //Luz VS Osc NO
        else
        {
            // Misma comunidad NO
            if (!generarRondaDTO.MismaComunidadCheck)
                await GenerarPrimeraRondaNoPermiteMismaComunidadAsync(
                    torneo,
                    random,
                    jugadoresSinEmparejar,
                    inscripciones,
                    generarRondaDTO,
                    emparejamientosAleatorios,
                    emparejamientos);

            // Misma comunidad SI
            else await GenerarPrimeraRondaSiPermiteMismaComunidadAsync(
                    torneo,
                    jugadoresSinEmparejar,
                    inscripciones,
                    generarRondaDTO,
                    emparejamientos);
        }
    }

    private async Task<List<EmparejamientoDTO>> GenerarPrimeraRondaLuzVsOscuridadAsync(
        Torneo torneo, 
        Random random, 
        List<Usuario> jugadoresSinEmparejar, 
        List<InscripcionTorneo> inscripciones, 
        GenerarRondaDTO generarRondaDTO, 
        List<EmparejamientoDTO> emparejamientos)
    {
        List<InscripcionTorneo> listaJugadoresLuz = [];
        List<InscripcionTorneo> listaJugadoresOscuridad = [];
        List<InscripcionTorneo> inscripcionesSinEmparejar = inscripciones
            .Where(i => jugadoresSinEmparejar.Any(j => j.IdUsuario == i.IdUsuario))
            .ToList();

        foreach (InscripcionTorneo inscripcion in inscripcionesSinEmparejar)
        {
            List<Lista> lista = [.. inscripcion.Lista];
            if (lista != null && lista.Count > 0)
            {
                string? bando = lista[0].Bando;
                if (bando == "good") listaJugadoresLuz.Add(inscripcion);
                else listaJugadoresOscuridad.Add(inscripcion);
            }
        }

        listaJugadoresLuz = [.. listaJugadoresLuz.OrderBy(x => random.Next())];
        listaJugadoresOscuridad = [.. listaJugadoresOscuridad.OrderBy(x => random.Next())];

        while (jugadoresSinEmparejar.Count >= 2
               && listaJugadoresLuz.Count > 0
               && listaJugadoresOscuridad.Count > 0)
        {
            // Cruzamos los jugadores de cada lista
            InscripcionTorneo inscripcionLuz = listaJugadoresLuz[0];
            InscripcionTorneo inscripcionOscuridad = listaJugadoresOscuridad[0];

            Usuario? jugLuz = jugadoresSinEmparejar
                .FirstOrDefault(x => x.IdUsuario == inscripcionLuz.IdUsuario);
            Usuario? jugOscuridad = jugadoresSinEmparejar
                .FirstOrDefault(x => x.IdUsuario == inscripcionOscuridad.IdUsuario);
            if (jugLuz == null || jugOscuridad == null) continue;

            EmparejamientoDTO nuevoEmparejamiento = new()
            {
                Jugador1 = new JugadorEmparejamientoDTO { IdUsuario = jugLuz.IdUsuario, Nick = jugLuz.Nick },
                Jugador2 = new JugadorEmparejamientoDTO { IdUsuario = jugOscuridad.IdUsuario, Nick = jugOscuridad.Nick }
            };

            // Añadir el emparejamiento a la lista
            emparejamientos.Add(nuevoEmparejamiento);

            // Remover los jugadores emparejados
            jugadoresSinEmparejar.Remove(jugLuz);
            jugadoresSinEmparejar.Remove(jugOscuridad);
            listaJugadoresLuz.Remove(inscripcionLuz);
            listaJugadoresOscuridad.Remove(inscripcionOscuridad);
        }
        while (jugadoresSinEmparejar.Count >= 2)
        {
            Usuario usuario1 = jugadoresSinEmparejar[0];
            Usuario usuario2 = jugadoresSinEmparejar[1];
            

            EmparejamientoDTO nuevoEmparejamiento = new()
            {
                Jugador1 = new JugadorEmparejamientoDTO { IdUsuario = usuario1.IdUsuario, Nick = usuario1.Nick },
                Jugador2 = new JugadorEmparejamientoDTO { IdUsuario = usuario2.IdUsuario, Nick = usuario2.Nick }
            };

            // Añadir el emparejamiento a la lista
            emparejamientos.Add(nuevoEmparejamiento);

            // Remover los jugadores emparejados
            jugadoresSinEmparejar.Remove(usuario1);
            jugadoresSinEmparejar.Remove(usuario2);
        }

        //Controlamos bye
        if(jugadoresSinEmparejar.Count > 0)
        {
            EmparejamientoDTO nuevoEmparejamiento = new()
            {
                Jugador1 = new JugadorEmparejamientoDTO { IdUsuario = jugadoresSinEmparejar[0].IdUsuario, Nick = jugadoresSinEmparejar[0].Nick },
                Jugador2 = new JugadorEmparejamientoDTO { IdUsuario = 568, Nick = "BYE"}
            };

            emparejamientos.Add(nuevoEmparejamiento);

            CrearInscripcionDTO inscripcionTorneo = new()
            {
                IdTorneo = torneo.IdTorneo,
                IdUsuario = 568,
            };
            ResultInscripcionTorneoDTO registro = await _unitOfWork.InscripcionRepository.Register(
            _mapper.Map<InscripcionTorneo>(inscripcionTorneo));
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

        return emparejamientos;
    }

    private async Task GenerarPrimeraRondaSiPermiteMismaComunidadAsync(
        Torneo torneo, 
        List<Usuario> jugadoresSinEmparejar, 
        List<InscripcionTorneo> inscripciones,
        GenerarRondaDTO generarRondaDTO, 
        List<EmparejamientoDTO> emparejamientos)
    {
        // 4. Los emparejamos 
        while (jugadoresSinEmparejar.Count >= 2)
        {
            Usuario jugador1, jugador2;
            bool emparejamientoValido = false;

            do
            {
                // Seleccionamos dos jugadores aleatorios
                jugador1 = jugadoresSinEmparejar[0];
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

        //Controlamos bye
        if (jugadoresSinEmparejar.Count > 0)
        {
            EmparejamientoDTO nuevoEmparejamiento = new()
            {
                Jugador1 = new JugadorEmparejamientoDTO { IdUsuario = jugadoresSinEmparejar[0].IdUsuario, Nick = jugadoresSinEmparejar[0].Nick },
                Jugador2 = new JugadorEmparejamientoDTO { IdUsuario = 568, Nick = "BYE" }
            };

            emparejamientos.Add(nuevoEmparejamiento);
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

    private async Task GenerarPrimeraRondaNoPermiteMismaComunidadAsync(
        Torneo torneo, 
        Random random,
        List<Usuario> jugadoresSinEmparejar,
        List<InscripcionTorneo> inscripciones,
        GenerarRondaDTO generarRondaDTO,
        List<EmparejamientoDTO> emparejamientosAleatorios, 
        List<EmparejamientoDTO> emparejamientos)
    {
        // Agrupar jugadores por facción
        Dictionary<int, List<Usuario>> jugadoresPorFaccion = [];
        foreach (var jugador in jugadoresSinEmparejar)
        {
            int idFaccion = jugador.IdFaccion ?? -1;
            if (!jugadoresPorFaccion.TryGetValue(idFaccion, out List<Usuario>? value))
            {
                value = ([]);
                jugadoresPorFaccion[idFaccion] = value;
            }

            value.Add(jugador);
        }

        while (jugadoresPorFaccion.Any(f => f.Value.Count > 0) && jugadoresSinEmparejar.Count > 0)
        {
            List<KeyValuePair<int, List<Usuario>>> faccionesRestantes = jugadoresPorFaccion.Where(f => f.Value.Count > 0).ToList();

            // Seleccionamos aleatoriamente una facción con jugadores
            KeyValuePair<int, List<Usuario>> randomFaccion = faccionesRestantes[random.Next(faccionesRestantes.Count)];
            Usuario jugador1 = randomFaccion.Value[0]; // Primer jugador

            // Remover jugador1 de su facción
            jugadoresSinEmparejar.Remove(jugador1);
            randomFaccion.Value.RemoveAt(0);

            // Si no hay mas jugadores, removemos facción
            if (randomFaccion.Value.Count == 0)
                jugadoresPorFaccion.Remove(randomFaccion.Key);

            // Buscar un jugador de una facción diferente
            Usuario? jugador2 = jugadoresPorFaccion
                .Where(f => f.Key != randomFaccion.Key && f.Value.Count > 0)
                .SelectMany(f => f.Value)
                .FirstOrDefault();

            // Remover al jugador 2 de su faccion
            if (jugador2 == null)
            {
                // Si no hay un jugador disponible de una facción diferente, selecciona un jugador de la misma facción
                if (randomFaccion.Value.Count > 0)
                {
                    jugador2 = randomFaccion.Value[0];
                    jugadoresSinEmparejar.Remove(jugador2);
                    randomFaccion.Value.RemoveAt(0);
                    if (randomFaccion.Value.Count == 0)
                        jugadoresPorFaccion.Remove(randomFaccion.Key);
                }
                else
                {
                    break;
                }
            }
            else
            {
                // Remover jugador2 de su respectiva facción
                int idFaccionJugador2 = jugador2.IdFaccion ?? -1;
                jugadoresSinEmparejar.Remove(jugador2);
                jugadoresPorFaccion[idFaccionJugador2].Remove(jugador2);

                // Eliminar la facción si no hay más jugadores
                if (jugadoresPorFaccion[idFaccionJugador2].Count == 0)
                    jugadoresPorFaccion.Remove(idFaccionJugador2);
            }

            // Crear el emparejamiento
            emparejamientosAleatorios.Add(new EmparejamientoDTO
            {
                Jugador1 = new JugadorEmparejamientoDTO
                {
                    IdUsuario = jugador1.IdUsuario,
                    Nick = jugador1.Nick,
                    IdFaccion = jugador1.IdFaccion,
                },
                Jugador2 = new JugadorEmparejamientoDTO
                {
                    IdUsuario = jugador2.IdUsuario,
                    Nick = jugador2.Nick,
                    IdFaccion = jugador2.IdFaccion,
                }
            });
        }

        // Caso en que queda un solo jugador sin emparejar
        if (jugadoresSinEmparejar.Count == 1)
        {
            Usuario ultimoJugador = jugadoresSinEmparejar[0];
            jugadoresSinEmparejar.RemoveAt(0);

            // Buscar emparejamiento con facciones distintas y reasignar si es posible
            foreach (var emparejamiento in emparejamientos)
            {
                if (emparejamiento.Jugador1.IdUsuario != ultimoJugador.IdUsuario &&
                    emparejamiento.Jugador2.IdUsuario != ultimoJugador.IdUsuario)
                {
                    // Remplazar Jugador2 del emparejamiento para mantener diversidad
                    JugadorEmparejamientoDTO jugadorExistente = emparejamiento.Jugador2;
                    emparejamiento.Jugador2 = new JugadorEmparejamientoDTO { IdUsuario = ultimoJugador.IdUsuario, Nick = ultimoJugador.Nick };

                    // Crear un emparejamiento nuevo con el jugador anterior
                    EmparejamientoDTO ajusteEmparejamiento = new()
                    {
                        Jugador1 = new JugadorEmparejamientoDTO { IdUsuario = jugadorExistente.IdUsuario, Nick = jugadorExistente.Nick },
                        Jugador2 = new JugadorEmparejamientoDTO { IdUsuario = ultimoJugador.IdUsuario, Nick = ultimoJugador.Nick }
                    };

                    emparejamientos.Add(ajusteEmparejamiento);
                    break;
                }
            }
        }

        // Comprobar que los emparejamientos son validos.
        List<EmparejamientoDTO> emparejamientosInvalidos = emparejamientosAleatorios
            .Where(e => e.Jugador1.IdFaccion == e.Jugador2.IdFaccion)
            .ToList();

        // Modificar emparejamientos inválidos
        foreach (var emparejamiento in emparejamientosInvalidos)
        {
            bool emparejamientoValido = false;
            do
            {
                // Obtener el jugador que necesita ser reemplazado
                JugadorEmparejamientoDTO jugadorACambiar = emparejamiento.Jugador2;

                // Buscar un emparejamiento alternativo donde podamos tomar el jugador para reemplazo
                EmparejamientoDTO? emparejamientoAlternativo = emparejamientosAleatorios
                    .FirstOrDefault(e => e.Jugador1.IdUsuario != jugadorACambiar.IdUsuario &&
                                         e.Jugador2.IdUsuario != jugadorACambiar.IdUsuario &&
                                         e.Jugador1.IdFaccion != emparejamiento.Jugador1.IdFaccion &&
                                         e.Jugador2.IdFaccion != emparejamiento.Jugador1.IdFaccion);

                if (emparejamientoAlternativo != null)
                {
                    // Intercambiar jugadores
                    JugadorEmparejamientoDTO jugadorAlternativo = emparejamientoAlternativo.Jugador2;
                    if (jugadorACambiar.IdFaccion != emparejamientoAlternativo.Jugador1.IdFaccion &&
                        emparejamiento.Jugador1.IdFaccion != jugadorAlternativo.IdFaccion)
                    {
                        // Realizar el intercambio
                        emparejamiento.Jugador2 = new JugadorEmparejamientoDTO
                        {
                            IdUsuario = jugadorAlternativo.IdUsuario,
                            Nick = jugadorAlternativo.Nick,
                            IdFaccion = jugadorAlternativo.IdFaccion
                        };

                        emparejamientoAlternativo.Jugador2 = new JugadorEmparejamientoDTO
                        {
                            IdUsuario = jugadorACambiar.IdUsuario,
                            Nick = jugadorACambiar.Nick,
                            IdFaccion = jugadorACambiar.IdFaccion
                        };
                        emparejamientoValido = true;
                    }
                    else emparejamientoValido = false;
                }
            }
            while (!emparejamientoValido);

        }

        emparejamientos.AddRange(emparejamientosAleatorios);

        //Controlamos bye
        if (jugadoresSinEmparejar.Count > 0)
        {
            EmparejamientoDTO nuevoEmparejamiento = new()
            {
                Jugador1 = new JugadorEmparejamientoDTO { IdUsuario = jugadoresSinEmparejar[0].IdUsuario, Nick = jugadoresSinEmparejar[0].Nick },
                Jugador2 = new JugadorEmparejamientoDTO { IdUsuario = 568, Nick = "BYE" }
            };

            emparejamientos.Add(nuevoEmparejamiento);
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

    private async Task GenerarOtrasRondasAsync(
        GenerarRondaDTO generarRondaDTO, 
        Torneo torneo, 
        List<EmparejamientoDTO> emparejamientos, 
        List<InscripcionTorneo> inscripciones)
    {
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

    private async Task ActualizarEloAsync(GenerarRondaDTO generarRondaDTO)
    {
        //Si es segunda ronda o mas actualizamos el elo de la ronda anterior
        if (generarRondaDTO.IdRonda > 1)
        {
            // Nos traemos todas las partidas de la ronda
            List<PartidaTorneo> partidas =
                await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneoByRonda(
                    generarRondaDTO.IdTorneo,
                    generarRondaDTO.IdRonda - 1);

            //Si tiene mas de 7 partidas cuenta para el ELO
            if (partidas.Count > 7)
            {
                //Actualizamos el elo de los jugadores
                foreach (PartidaTorneo partida in partidas)
                {
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
                    if (partida.GanadorPartidaTorneo == null)
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

                    if(createElo1.IdUsuario != 568)
                        await _eloApplication.RegisterElo(createElo1);

                    //Elo jugador 2
                    CreateEloDTO createElo2 = new()
                    {
                        IdUsuario = (int)partida.IdUsuario2,
                        PuntuacionElo = nuevoEloJugador2
                    };

                    if (createElo2.IdUsuario != 568)
                        await _eloApplication.RegisterElo(createElo2);
                }
            }
        }
    }

    public async Task<bool> EdtarPairing(UpdatePairingTorneoDTO request)
    {
        PartidaTorneo existingEntity = 
            await _unitOfWork.PartidaTorneoRepository.GetById(request.IdPartidaTorneo);
        if (existingEntity == null) return false;

        List<Usuario> inscripciones = await _unitOfWork.UsuarioRepository.GetUsuariosByTorneo(existingEntity.IdTorneo);

        Usuario? inscripcion1 = inscripciones.FirstOrDefault(x => x.IdUsuario == request.IdUsuario1);
        Usuario? inscripcion2 = inscripciones.FirstOrDefault(x => x.IdUsuario == request.IdUsuario2);

        if (inscripcion1 is null || inscripcion2 is null) return false;

        existingEntity.IdUsuario1 = inscripcion1.IdUsuario;
        existingEntity.IdUsuario2 = inscripcion2.IdUsuario;
        existingEntity.EjercitoUsuario1 = 
            inscripcion1.InscripcionTorneos.FirstOrDefault()?.Lista?.FirstOrDefault()?.Ejercito ?? "N/A";
        existingEntity.EjercitoUsuario2 = 
            inscripcion2.InscripcionTorneos.FirstOrDefault()?.Lista?.FirstOrDefault()?.Ejercito ?? "N/A";

        return await _unitOfWork.PartidaTorneoRepository.Edit(existingEntity); ;
    }
}
