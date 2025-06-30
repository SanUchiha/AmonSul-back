using AS.Application.DTOs.Inscripcion;
using AS.Application.DTOs.PartidaTorneo;
using AS.Application.DTOs.Torneo;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Equipo;
using AS.Domain.DTOs.Inscripcion;
using AS.Domain.DTOs.Lista;
using AS.Domain.DTOs.Torneo;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;
using MoreLinq.Extensions;

namespace AS.Application.Services;

public class PartidaTorneoApplication(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IEmailApplicacion emailApplicacion) : IPartidaTorneoApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
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
        // Traer todas las inscripciones del torneo
        List<InscripcionTorneo> inscripciones = 
            await _unitOfWork.InscripcionRepository.GetInscripcionesByTorneo(generarRondaDTO.IdTorneo);
        Torneo torneo = await _unitOfWork.TorneoRepository.GetById(generarRondaDTO.IdTorneo);

        List<Usuario> jugadores = [];
        List<string> destinatarios = [];

        List<int> usuarioIds =
            [.. inscripciones.Select(i => i.IdUsuario).Distinct()];

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
            [.. jugadores.Where(j =>
                !emparejamientos.Any
                    (e => e.Jugador1.IdUsuario == j.IdUsuario || e.Jugador2.IdUsuario == j.IdUsuario))];

        Random random = new();
        // 2. Mezclamos a los jugadores
        jugadoresSinEmparejar = [.. jugadoresSinEmparejar.OrderBy(x => random.Next())];
        List<EmparejamientoDTO> listaEmparejadosLuzVsOscuridad = [];

        // TorneoNarsil
        if (generarRondaDTO.IsTorneoNarsil)
        {
            await GenerarPrimeraRondaNoPermiteMismaComunidadAsync(
                    torneo,
                    random,
                    jugadoresSinEmparejar,
                    inscripciones,
                    generarRondaDTO,
                    emparejamientosAleatorios,
                    emparejamientos);
        }

        // Luz vs Oscuridad SI
        else if (generarRondaDTO.LuzVsOscCheck)
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
        List<InscripcionTorneo> inscripcionesSinEmparejar = 
            [.. inscripciones.Where(i => jugadoresSinEmparejar.Any(j => j.IdUsuario == i.IdUsuario))];

        foreach (InscripcionTorneo inscripcion in inscripcionesSinEmparejar)
        {
            List<Lista> lista = [.. inscripcion.Lista];
            if (lista != null && lista.Count > 0)
            {
                if (inscripcion.Lista.Count == 1)
                {
                    string? bando = lista.First().Bando;
                    if (bando == "good") listaJugadoresLuz.Add(inscripcion);
                    else listaJugadoresOscuridad.Add(inscripcion);
                }

                else
                {
                    bool todasMismoEjercito = inscripcion.Lista.All(x => x.Ejercito == inscripcion.Lista.First().Ejercito);
                    if (todasMismoEjercito)
                    {
                        string? bando = lista[0].Bando;
                        if (bando == "good") listaJugadoresLuz.Add(inscripcion);
                        else listaJugadoresOscuridad.Add(inscripcion);
                    }
                }
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
                    if(inscripcion.Lista.Count > 0)
                    {
                        if (inscripcion.Lista.Count == 1)
                            nuevaPartida.EjercitoUsuario1 = inscripcion.Lista.First().Ejercito;
                        else 
                        {
                            bool todasMismoEjercito = inscripcion.Lista.All(x => x.Ejercito == inscripcion.Lista.First().Ejercito);
                            if (todasMismoEjercito) nuevaPartida.EjercitoUsuario1 = inscripcion.Lista.First().Ejercito;
                        }
                    }

                if (inscripcion.IdUsuario == item.Jugador2.IdUsuario)
                    if (inscripcion.Lista.Count > 0)
                    {
                        if (inscripcion.Lista.Count == 1)
                            nuevaPartida.EjercitoUsuario2 = inscripcion.Lista.First().Ejercito;
                        else
                        {
                            bool todasMismoEjercito = inscripcion.Lista.All(x => x.Ejercito == inscripcion.Lista.First().Ejercito);
                            if (todasMismoEjercito) nuevaPartida.EjercitoUsuario2 = inscripcion.Lista.First().Ejercito;
                        }
                    }   
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
        // Controlamos BYE
        if (generarRondaDTO.OpcionImpares == "bye")
        {
            Random randomBye = new();
            int indiceAleatorio = random.Next(jugadoresSinEmparejar.Count); // genera un índice entre 0 y lista.Count - 1
            EmparejamientoDTO nuevoEmparejamiento = new()
            {
                Jugador1 = new JugadorEmparejamientoDTO { IdUsuario = jugadoresSinEmparejar[indiceAleatorio].IdUsuario, Nick = jugadoresSinEmparejar[indiceAleatorio].Nick },
                Jugador2 = new JugadorEmparejamientoDTO { IdUsuario = 568, Nick = "BYE" }
            };

            emparejamientos.Add(nuevoEmparejamiento);
            jugadoresSinEmparejar.Remove(jugadoresSinEmparejar[indiceAleatorio]);
        }
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
            List<KeyValuePair<int, List<Usuario>>> faccionesRestantes = [.. jugadoresPorFaccion.Where(f => f.Value.Count > 0)];

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
        List<EmparejamientoDTO> emparejamientosInvalidos = 
            [.. emparejamientosAleatorios.Where(e => e.Jugador1.IdFaccion == e.Jugador2.IdFaccion)];

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


        if (emparejamientos.Count > 0)
        {
            EmparejamientoDTO primero = emparejamientos[0];
            emparejamientos.RemoveAt(0);        // Elimina el primero
            emparejamientos.Add(primero);       // Lo añade al final
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

            int idInscripcion1 = 0;
            int idInscripcion2 = 0;

            foreach (var inscripcion in inscripciones)
            {
                if (inscripcion.IdUsuario == item.Jugador1.IdUsuario)
                    idInscripcion1 = inscripcion.IdInscripcion;
                    if (inscripcion.Lista.Count != 0)
                        nuevaPartida.EjercitoUsuario1 = inscripcion.Lista.ToList()[0].Ejercito;

                if (inscripcion.IdUsuario == item.Jugador2.IdUsuario)
                    idInscripcion2 = inscripcion.IdInscripcion;
                    if (inscripcion.Lista.Count != 0)
                        nuevaPartida.EjercitoUsuario2 = inscripcion.Lista.ToList()[0].Ejercito;
            }

            await _unitOfWork.PartidaTorneoRepository.Register(nuevaPartida);

            // Registramos la participacion en el torneo
            await RegisterParticipacionTorneo(nuevaPartida, idInscripcion1, idInscripcion2);
        }
    }

    private async Task RegisterParticipacionTorneo(PartidaTorneo nuevaPartida, int idInscripcion1, int idInscripcion2)
    {
        if(nuevaPartida.NumeroRonda != null)
        {
            ParticipacionTorneo participacionTorneo1 = new()
            {
                IdTorneo = nuevaPartida.IdTorneo,
                IdUsuario = nuevaPartida.IdUsuario1,
                IdRonda = nuevaPartida.NumeroRonda.Value,
                IdBando = 0,
                IdInscripcion = idInscripcion1
            };

            await _unitOfWork.ParticipacionTorneoRepository.Register(participacionTorneo1);

            ParticipacionTorneo participacionTorneo2 = new()
            {
                IdTorneo = nuevaPartida.IdTorneo,
                IdUsuario = nuevaPartida.IdUsuario2,
                IdRonda = nuevaPartida.NumeroRonda.Value,
                IdBando = 1,
                IdInscripcion = idInscripcion2
            };

            await _unitOfWork.ParticipacionTorneoRepository.Register(participacionTorneo2);
        }
        
    }

    private async Task GenerarOtrasRondasAsync(
        GenerarRondaDTO generarRondaDTO, 
        Torneo torneo, 
        List<EmparejamientoDTO> emparejamientos, 
        List<InscripcionTorneo> inscripciones)
    {
        if (generarRondaDTO.IsTorneoNarsil)
        {

            List<JugadorNarsilDTO> jugadoresNarsil = [];
            List<EmparejamientoDTO> emparejamientosDefinitivos = [];

            // creo la lista jugadores narsil ordenador por clasi
            foreach (var emp in emparejamientos) 
            {
                foreach (var jugador in new[] { emp.Jugador1, emp.Jugador2 })
                {
                    if (jugador.Nick == "BYE")
                    {
                        jugadoresNarsil.Add(new JugadorNarsilDTO
                        {
                            IdUsuario = jugador.IdUsuario,
                            Nick = jugador.Nick
                        });
                    }
                    else
                    {
                        ParticipacionTorneo? participacion = await _unitOfWork.ParticipacionTorneoRepository
                            .GetByIdUsuarioAndRonda(torneo.IdTorneo, jugador.IdUsuario, generarRondaDTO.IdRonda - 1);

                        jugadoresNarsil.Add(new JugadorNarsilDTO
                        {
                            IdUsuario = jugador.IdUsuario,
                            Nick = jugador.Nick,
                            BandoAnterior = participacion?.IdBando
                        });
                    }
                }
            }

            // recorre para emparejar segun bando y clasi
            for (int i = 0; i < jugadoresNarsil.Count; i++)
            {
                JugadorNarsilDTO jugadorActual = jugadoresNarsil[i];

                if (jugadorActual.Emparejado)
                    continue;

                JugadorNarsilDTO? pareja = null;

                // Intentamos emparejar con el siguiente jugador no emparejado
                for (int j = i + 1; j < jugadoresNarsil.Count; j++)
                {
                    JugadorNarsilDTO candidato = jugadoresNarsil[j];

                    if (candidato.Emparejado)
                        continue;

                    // Caso 1: bandos opuestos -> emparejar
                    if ((jugadorActual.BandoAnterior == 0 && candidato.BandoAnterior == 1) ||
                        (jugadorActual.BandoAnterior == 1 && candidato.BandoAnterior == 0))
                    {
                        pareja = candidato;
                        break;
                    }

                    // Caso 2: mismos bandos, buscamos el siguiente candidato
                    if (jugadorActual.BandoAnterior == candidato.BandoAnterior)
                    {
                        continue;
                    }
                }

                if (pareja != null)
                {
                    if(jugadorActual.BandoAnterior == 0)
                    {
                        emparejamientosDefinitivos.Add(new EmparejamientoDTO
                        {
                            Jugador1 = new JugadorEmparejamientoDTO
                            {
                                IdUsuario = pareja.IdUsuario,
                                Nick = pareja.Nick
                            },
                            Jugador2 = new JugadorEmparejamientoDTO
                            {
                                IdUsuario = jugadorActual.IdUsuario,
                                Nick = jugadorActual.Nick
                            }
                        });
                    }
                    else
                    {
                        emparejamientosDefinitivos.Add(new EmparejamientoDTO
                        {
                            Jugador2 = new JugadorEmparejamientoDTO
                            {
                                IdUsuario = pareja.IdUsuario,
                                Nick = pareja.Nick
                            },
                            Jugador1 = new JugadorEmparejamientoDTO
                            {
                                IdUsuario = jugadorActual.IdUsuario,
                                Nick = jugadorActual.Nick
                            }
                        });
                    }
                    jugadorActual.Emparejado = true;
                    pareja.Emparejado = true;
                }
                else
                {
                    // Si no encontró pareja por bando, busca al siguiente no emparejado sin importar bando
                    for (int j = i + 1; j < jugadoresNarsil.Count; j++)
                    {
                        JugadorNarsilDTO candidato = jugadoresNarsil[j];

                        if (!candidato.Emparejado)
                        {
                            pareja = candidato;
                            break;
                        }
                    }

                    if (pareja != null)
                    {
                        if (jugadorActual.BandoAnterior == 0)
                        {
                            emparejamientosDefinitivos.Add(new EmparejamientoDTO
                            {
                                Jugador1 = new JugadorEmparejamientoDTO
                                {
                                    IdUsuario = pareja.IdUsuario,
                                    Nick = pareja.Nick
                                },
                                Jugador2 = new JugadorEmparejamientoDTO
                                {
                                    IdUsuario = jugadorActual.IdUsuario,
                                    Nick = jugadorActual.Nick
                                }
                            });
                        }
                        else
                        {
                            emparejamientosDefinitivos.Add(new EmparejamientoDTO
                            {
                                Jugador2 = new JugadorEmparejamientoDTO
                                {
                                    IdUsuario = pareja.IdUsuario,
                                    Nick = pareja.Nick
                                },
                                Jugador1 = new JugadorEmparejamientoDTO
                                {
                                    IdUsuario = jugadorActual.IdUsuario,
                                    Nick = jugadorActual.Nick
                                }
                            });
                        }
                        jugadorActual.Emparejado = true;
                        pareja.Emparejado = true;
                    }
                    else
                    {

                        if (jugadorActual.BandoAnterior == 0)
                        {
                            emparejamientosDefinitivos.Add(new EmparejamientoDTO
                            {
                                Jugador2 = new JugadorEmparejamientoDTO
                                {
                                    IdUsuario = jugadorActual.IdUsuario,
                                    Nick = jugadorActual.Nick
                                },
                                Jugador1 = new JugadorEmparejamientoDTO
                                {
                                    IdUsuario = 568,
                                    Nick = "BYE"
                                }
                            });
                        }
                        else
                        {
                            emparejamientosDefinitivos.Add(new EmparejamientoDTO
                            {
                                Jugador1 = new JugadorEmparejamientoDTO
                                {
                                    IdUsuario = jugadorActual.IdUsuario,
                                    Nick = jugadorActual.Nick
                                },
                                Jugador2 = new JugadorEmparejamientoDTO
                                {
                                    IdUsuario = 568,
                                    Nick = "BYE"
                                }
                            });
                        }
                        jugadorActual.Emparejado = true;                       
                    }
                }

            }

            // Crear las partidas
            foreach (var item in emparejamientosDefinitivos)
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

                int idInscripcion1 = 0;
                int idInscripcion2 = 0;

                nuevaPartida.EjercitoUsuario1 = "";
                nuevaPartida.EjercitoUsuario2 = "";

                foreach (var inscripcion in inscripciones)
                {

                    if (inscripcion.IdUsuario == item.Jugador1.IdUsuario) 
                    {
                        idInscripcion1 = inscripcion.IdInscripcion;
                        if (inscripcion.Lista.Count != 0)
                            nuevaPartida.EjercitoUsuario1 = inscripcion.Lista.ToList()[0].Ejercito;
                    }

                    if (inscripcion.IdUsuario == item.Jugador2.IdUsuario)
                    {
                        idInscripcion2 = inscripcion.IdInscripcion;
                        if (inscripcion.Lista.Count != 0)
                            nuevaPartida.EjercitoUsuario2 = inscripcion.Lista.ToList()[0].Ejercito;
                    }
                        
                    
                }

                await _unitOfWork.PartidaTorneoRepository.Register(nuevaPartida);

                // Registramos la participacion en el torneo
                await RegisterParticipacionTorneo(nuevaPartida, idInscripcion1, idInscripcion2);
            }
        }
        else
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

        // Cambiar el id del equipo
        int? idEquipo1 = 
            await _unitOfWork.InscripcionRepository.GetIdEquipoByIdUsuarioAndIdTorneoAsync(
                inscripcion1.IdUsuario, 
                existingEntity.IdTorneo);
        int? idEquipo2 =
            await _unitOfWork.InscripcionRepository.GetIdEquipoByIdUsuarioAndIdTorneoAsync(
                inscripcion2.IdUsuario, 
                existingEntity.IdTorneo);
        
        existingEntity.IdEquipo1 = idEquipo1;
        existingEntity.IdEquipo2 = idEquipo2;

        return await _unitOfWork.PartidaTorneoRepository.Edit(existingEntity); ;
    }

    public async Task<List<PartidaTorneoMasDTO>> GetPartidasMasTorneoAsync(int idTorneo)
    {
        List<PartidaTorneo> rawPartidas = await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneo(idTorneo);

        List<PartidaTorneoMasDTO> partidas = _mapper.Map<List<PartidaTorneoMasDTO>>(rawPartidas);

        //Conseguir las listas
        foreach (var item in partidas)
        {
            List<Lista> listasJugador1 = await _unitOfWork.ListaRepository.GetListasByTorneoByUserAsync(idTorneo, item.IdUsuario1);
            List<Lista> listasJugador2 = await _unitOfWork.ListaRepository.GetListasByTorneoByUserAsync(idTorneo, item.IdUsuario2);

            item.ListasJugador1 = _mapper.Map<List<ListaDTO>>(listasJugador1);
            item.ListasJugador2 = _mapper.Map<List<ListaDTO>>(listasJugador2);
        }

        return partidas;
    }

    public async Task<List<PartidaTorneoDTO>> GetPartidasTorneo(int idTorneo)
    {
        List<PartidaTorneo> rawPartidas = await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneo(idTorneo);

        List<PartidaTorneoDTO> partidas = _mapper.Map<List<PartidaTorneoDTO>>(rawPartidas);

        return partidas;
    }

    public async Task<bool> GenerateRoundEquipos(GenerarRondaEquiposDTO request)
    {
        // Si vienen los emparejamientos predefinidos
        if (request.EmparejamientosEquipos?.Count != 0)
        {
            return await GenerarPartidasDesdeEmparejamientos(request.EmparejamientosEquipos!, request);
        }
        else
        {
            // Conseguir todos los equipos inscritos en el torneo.
            List<EquipoDTO> equipos = await _unitOfWork.InscripcionRepository.GetAllEquiposByTorneoAsync(request.IdTorneo);

            //Mezclamos
            equipos = [.. equipos.Shuffle()];

            // Controlamos el bye

            //if (request.NecesitaBye)
            //{
            //    var equipoBye
            //    equipos.Add()
            //}

            // Creamos la lista de emparejamientos
            List<EmparejamientoEquiposDTO> emparejamientos = [];

            for (int i = 0; i < equipos.Count - 1; i += 2)
            {
                EquipoDTO equipo1 = equipos[i];
                EquipoDTO equipo2 = equipos[i + 1];

                List<InscripcionTorneo> inscripciones1 = await _unitOfWork.InscripcionRepository.GetAllInscripcionesByEquipoAsync(equipo1.IdEquipo);
                List<InscripcionTorneo> inscripciones2 = await _unitOfWork.InscripcionRepository.GetAllInscripcionesByEquipoAsync(equipo2.IdEquipo);

                emparejamientos.Add(new EmparejamientoEquiposDTO
                {
                    Equipo1 = new EquipoEmparejamientoDTO
                    {
                        IdEquipo = equipo1.IdEquipo,
                        IdCapitan = equipo1.IdCapitan,
                        NickCapitan = equipo1.NickCapitan,
                        NombreEquipo = equipo1.NombreEquipo,
                        Inscripciones = _mapper.Map<List<InscripcionTorneoEmparejamientoDTO>>(inscripciones1)
                    },
                    Equipo2 = new EquipoEmparejamientoDTO
                    {
                        IdEquipo = equipo2.IdEquipo,
                        IdCapitan = equipo2.IdCapitan,
                        NickCapitan = equipo2.NickCapitan,
                        NombreEquipo = equipo2.NombreEquipo,
                        Inscripciones = _mapper.Map<List<InscripcionTorneoEmparejamientoDTO>>(inscripciones2)
                    }
                });
            }

            if (request.NecesitaBye)
            {
                EquipoDTO equipo1 = equipos[^1];

                List<InscripcionTorneo> inscripciones1 = await _unitOfWork.InscripcionRepository.GetAllInscripcionesByEquipoAsync(equipo1.IdEquipo);

                emparejamientos.Add(new EmparejamientoEquiposDTO
                {
                    Equipo1 = new EquipoEmparejamientoDTO
                    {
                        IdEquipo = equipo1.IdEquipo,
                        IdCapitan = equipo1.IdCapitan,
                        NickCapitan = equipo1.NickCapitan,
                        NombreEquipo = equipo1.NombreEquipo,
                        Inscripciones = _mapper.Map<List<InscripcionTorneoEmparejamientoDTO>>(inscripciones1)
                    },
                    Equipo2 = new EquipoEmparejamientoDTO
                    {
                        IdEquipo = 117,
                        IdCapitan = 568,
                        NickCapitan = "Capitan BYE",
                        NombreEquipo = "Equipo BYE",
                        Inscripciones = []
                    }
                });
            }

            return await GenerarPartidasDesdeEmparejamientos(emparejamientos, request);
        }
    }

    private async Task<bool> GenerarPartidasDesdeEmparejamientos(
        List<EmparejamientoEquiposDTO> emparejamientos, 
        GenerarRondaEquiposDTO request)
    {
        List<PartidaTorneo> partidas = [];

        foreach (EmparejamientoEquiposDTO emparejamiento in emparejamientos)
        {
            List<InscripcionTorneoEmparejamientoDTO> inscripcionesEquipo1 = emparejamiento.Equipo1.Inscripciones;
            List<InscripcionTorneoEmparejamientoDTO> inscripcionesEquipo2 = emparejamiento.Equipo2.Inscripciones;

            int totalPartidas = inscripcionesEquipo1.Count;

            for (int i = 0; i < totalPartidas; i++)
            {
                InscripcionTorneoEmparejamientoDTO jugador2 = new(){};
                InscripcionTorneoEmparejamientoDTO jugador1 = inscripcionesEquipo1[i];
                if (inscripcionesEquipo2.Count < 1)
                {
                    jugador2 = new()
                    {
                        IdUsuario = 568,
                        Ejercito = null,
                    };
                }
                else jugador2 = inscripcionesEquipo2[i];

                partidas.Add(new PartidaTorneo
                {
                    IdTorneo = request.IdTorneo,
                    IdUsuario1 = jugador1.IdUsuario,
                    IdUsuario2 = jugador2.IdUsuario,
                    EjercitoUsuario1 = jugador1.Ejercito,
                    EjercitoUsuario2 = jugador2.Ejercito,
                    NumeroRonda = request.IdRonda,
                    IdEquipo1 = emparejamiento.Equipo1.IdEquipo,
                    IdEquipo2 = emparejamiento.Equipo2.IdEquipo,
                    PartidaValidadaUsuario1 = false,
                    PartidaValidadaUsuario2 = false,
                    
                });
            }
        }

        await _unitOfWork.PartidaTorneoRepository.RegisterMany(partidas);
        return true;
    }

    public async Task<bool> GenerarOtraRondaEquiposAsync(GenerarOtraRondaEquiposRequestDTO request)
    {
        List<EquipoDTO> clasificacion = [.. request.Clasificacion];

        List<(int equipo1, int equipo2)> emparejamientos = [];
        HashSet<int> emparejados = [];

        if (request.NecesitaBye && clasificacion.Count % 2 != 0)
        {
            clasificacion.Add(new EquipoDTO
            {
                NombreEquipo = "Bye",
                IdEquipo = 117,
                IdCapitan = 568
            });
        }

        Dictionary<int, EquipoDTO> equiposDict = clasificacion.ToDictionary(e => e.IdEquipo);

        // 3. Emparejar equipos
        for (int i = 0; i < clasificacion.Count; i++)
        {
            EquipoDTO equipo1 = clasificacion[i];
            if (emparejados.Contains(equipo1.IdEquipo))
                continue;

            int j = i + 1;
            while (j < clasificacion.Count)
            {
                EquipoDTO equipo2 = clasificacion[j];

                if (emparejados.Contains(equipo2.IdEquipo))
                {
                    j++;
                    continue;
                }

                // Si permite repetir rival → no comprobamos nada más
                if (request.PermiteRepetirRival)
                {
                    emparejamientos.Add((equipo1.IdEquipo, equipo2.IdEquipo));
                    emparejados.Add(equipo1.IdEquipo);
                    emparejados.Add(equipo2.IdEquipo);
                    break;
                }

                // Si **NO** permite repetir rival → comprobamos que no se hayan enfrentado ya
                bool yaSeEnfrentaron = request.PairingRondaAnterior.Any(p =>
                    (p.IdEquipo1 == equipo1.IdEquipo && p.IdEquipo2 == equipo2.IdEquipo) ||
                    (p.IdEquipo1 == equipo2.IdEquipo && p.IdEquipo2 == equipo1.IdEquipo)
                );

                if (!yaSeEnfrentaron)
                {
                    emparejamientos.Add((equipo1.IdEquipo, equipo2.IdEquipo));
                    emparejados.Add(equipo1.IdEquipo);
                    emparejados.Add(equipo2.IdEquipo);
                    break;
                }

                j++;
            }
        }

        // Creamos las partidas.
        return await GenerarPartidasDesdeEmparejamientosOtrasRondas(emparejamientos, request, equiposDict);
    }

    private async Task<bool> GenerarPartidasDesdeEmparejamientosOtrasRondas(
        List<(int equipo1, int equipo2)> emparejamientos, 
        GenerarOtraRondaEquiposRequestDTO request,
        Dictionary<int, EquipoDTO> equiposDict)
    {
        List<PartidaTorneo> partidas = [];

        foreach ((int equipo1, int equipo2) in emparejamientos)
        {
            List<InscripcionTorneoEmparejamientoDTO> inscripcionesEquipo1 = 
                _mapper.Map<List<InscripcionTorneoEmparejamientoDTO>>(equiposDict[equipo1].Inscripciones);
            List<InscripcionTorneoEmparejamientoDTO> inscripcionesEquipo2 = 
                _mapper.Map<List<InscripcionTorneoEmparejamientoDTO>>(equiposDict[equipo2].Inscripciones);

            int totalPartidas = inscripcionesEquipo1.Count;

            for (int i = 0; i < totalPartidas; i++)
            {
                InscripcionTorneoEmparejamientoDTO jugador1 = inscripcionesEquipo1[i];
                InscripcionTorneoEmparejamientoDTO jugador2 = (inscripcionesEquipo2.Count > i)
                    ? inscripcionesEquipo2[i]
                    : new InscripcionTorneoEmparejamientoDTO { IdUsuario = 568 };

                partidas.Add(new PartidaTorneo
                {
                    IdTorneo = request.IdTorneo,
                    IdUsuario1 = jugador1.IdUsuario,
                    IdUsuario2 = jugador2.IdUsuario,
                    EjercitoUsuario1 = jugador1.Ejercito,
                    EjercitoUsuario2 = jugador2.Ejercito,
                    NumeroRonda = request.IdRonda,
                    IdEquipo1 = equipo1,
                    IdEquipo2 = equipo2,
                    PartidaValidadaUsuario1 = false,
                    PartidaValidadaUsuario2 = false,

                });
            }
        }

        await _unitOfWork.PartidaTorneoRepository.RegisterMany(partidas);
        return true;
    }

    public async Task<List<PartidaTorneoDTO>> GetPartidasTorneoAsync(int idTorneo)
    {
        List<PartidaTorneoDTO> partidaTorneoDTOs = await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneoAsync(idTorneo);
        return partidaTorneoDTOs;
    }
}


