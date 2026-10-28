using System.IO.Compression;
using System.Net.Mail;
using System.Text.Json;
using AS.Application.DTOs.Email;
using AS.Application.DTOs.Lista;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Lista;
using AS.Domain.DTOs.Usuario;
using AS.Domain.Models;
using AS.Domain.Types.Torneo;
using AS.Infrastructure.DTOs.Lista;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services;

public class ListaApplication(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IEmailApplicacion emailApplicacion,
    IHttpClientFactory httpClientFactory
) : IListaApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IEmailApplicacion _emailApplicacion = emailApplicacion;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<Lista> Delete(int idLista) =>
        await _unitOfWork.ListaRepository.Delete(idLista);

    public async Task<ListaDTO> GetListaById(int idLista)
    {
        Lista lista = await _unitOfWork.ListaRepository.GetListaById(idLista);

        return _mapper.Map<ListaDTO>(lista);
    }

    public async Task<ListaViewDTO> GetListaInscripcionById(int idInscripcion)
    {
        Lista lista = await _unitOfWork.ListaRepository.GetListaInscripcionById(idInscripcion);

        return _mapper.Map<ListaViewDTO>(lista);
    }

    // Solo las listas que puede ver el solicitante (ver PuedeVerListaTorneoAsync)
    public async Task<List<Lista>> GetListas(int idUsuarioSolicitante) =>
        await _unitOfWork.ListaRepository.GetListasVisibles(idUsuarioSolicitante);

    public async Task<List<ListaViewDTO>> GetListasByInscripcionAsync(int idInscripcion)
    {
        List<Lista> listas = await _unitOfWork.ListaRepository.GetListasByInscripcion(
            idInscripcion
        );

        return _mapper.Map<List<ListaViewDTO>>(listas);
    }

    public async Task<List<Lista>> GetListasByTorneo(int idTorneo) =>
        await _unitOfWork.ListaRepository.GetListasByTorneo(idTorneo);

    public async Task<List<Lista>> GetListasByUser(int idUsuario, int idUsuarioSolicitante) =>
        await _unitOfWork.ListaRepository.GetListasVisibles(idUsuarioSolicitante, idUsuario);

    public async Task<ListaDTO> GetListaTorneo(ListaTorneoRequestDTO listaTorneoRequestDTO)
    {
        Lista lista = await _unitOfWork.ListaRepository.GetListaTorneo(
            listaTorneoRequestDTO.IdTorneo,
            listaTorneoRequestDTO.IdUsuario
        );
        // Con lista conjunta el compañero no tiene lista propia: la de la pareja está en la inscripción del capitán
        if (lista is null)
        {
            List<InscripcionTorneo> inscripcionesPareja = await GetInscripcionesListaConjuntaAsync(
                listaTorneoRequestDTO.IdTorneo,
                listaTorneoRequestDTO.IdUsuario
            );
            lista = inscripcionesPareja.SelectMany(i => i.Lista).FirstOrDefault()!;
        }
        if (lista is null)
            return null!;

        ListaDTO listaDTO = _mapper.Map<ListaDTO>(lista);

        return listaDTO;
    }

    /// <summary>
    /// Con las listas ocultas solo pueden verla su dueño y el organizador del torneo
    /// </summary>
    public async Task<bool> PuedeVerListaTorneoAsync(
        int idTorneo,
        int idUsuarioLista,
        int idUsuarioSolicitante
    )
    {
        Torneo? torneo = await _unitOfWork.TorneoRepository.GetVisibilidadListas(idTorneo);
        if (torneo is null)
            return false;

        return torneo.MostrarListas
            || idUsuarioSolicitante == idUsuarioLista
            || idUsuarioSolicitante == torneo.IdUsuario;
    }

    /// <summary>
    /// Misma regla que PuedeVerListaTorneoAsync, sacando torneo y dueño de la inscripción de la lista
    /// </summary>
    public async Task<bool> PuedeVerListaAsync(int idLista, int idUsuarioSolicitante)
    {
        InscripcionTorneo? inscripcion =
            await _unitOfWork.ListaRepository.GetInscripcionByIdLista(idLista);
        if (inscripcion is null)
            return false;

        return await PuedeVerListaTorneoAsync(
            inscripcion.IdTorneo,
            inscripcion.IdUsuario,
            idUsuarioSolicitante
        );
    }

    /// <summary>
    /// Listas de una inscripción: misma regla, con torneo y dueño de la inscripción
    /// </summary>
    public async Task<bool> PuedeVerListasInscripcionAsync(int idInscripcion, int idUsuarioSolicitante)
    {
        InscripcionTorneo? inscripcion =
            await _unitOfWork.InscripcionRepository.GetTorneoYUsuario(idInscripcion);
        if (inscripcion is null)
            return false;

        return await PuedeVerListaTorneoAsync(
            inscripcion.IdTorneo,
            inscripcion.IdUsuario,
            idUsuarioSolicitante
        );
    }

    /// <summary>
    /// Todas las listas de un torneo: solo si son visibles o lo pide el organizador
    /// </summary>
    public async Task<bool> PuedeVerListasTorneoAsync(int idTorneo, int idUsuarioSolicitante)
    {
        Torneo? torneo = await _unitOfWork.TorneoRepository.GetVisibilidadListas(idTorneo);
        if (torneo is null)
            return false;

        return torneo.MostrarListas || idUsuarioSolicitante == torneo.IdUsuario;
    }

    public async Task<ResultRegisterListarDTO> RegisterLista(
        CreateListaTorneoDTO createListaTorneoDTO
    )
    {
        Lista lista = _mapper.Map<Lista>(createListaTorneoDTO);
        lista.Bando = createListaTorneoDTO.Ejercito.Band;
        lista.Ejercito = createListaTorneoDTO.Ejercito.Name;

        // Parejas con lista conjunta: una sola lista por pareja, en la inscripción del capitán
        InscripcionTorneo? inscripcionLista =
            await _unitOfWork.InscripcionRepository.GetTorneoYUsuario(createListaTorneoDTO.IdInscripcion);
        List<InscripcionTorneo> inscripcionesPareja = inscripcionLista is null
            ? []
            : await GetInscripcionesListaConjuntaAsync(
                inscripcionLista.IdTorneo,
                inscripcionLista.IdUsuario
            );
        if (inscripcionesPareja.Count > 0)
        {
            if (inscripcionesPareja.Any(i => i.Lista.Count > 0))
                return new ResultRegisterListarDTO
                {
                    Result = false,
                    Mensaje = "Tu pareja ya ha entregado la lista conjunta. Modifícala en lugar de subir otra.",
                };
            lista.IdInscripcion = inscripcionesPareja[0].IdInscripcion;
        }

        ResultRegisterListarDTO result = await _unitOfWork.ListaRepository.RegisterLista(lista);
        if (!result.Result)
            return result;

        InscripcionTorneo inscripcion = await _unitOfWork.InscripcionRepository.GetInscripcionById(
            createListaTorneoDTO.IdInscripcion
        );
        if (inscripcion == null)
            return result;

        inscripcion.EstadoLista = "ENTREGADA";
        inscripcion.FechaEntregaLista = createListaTorneoDTO.FechaEntrega;

        await _unitOfWork.InscripcionRepository.Update(inscripcion);
        await MarcarListaConjuntaEntregadaAsync(
            inscripcionesPareja,
            inscripcion.IdInscripcion,
            createListaTorneoDTO.FechaEntrega
        );

        string? emailOrganizador = createListaTorneoDTO.EmailOrganizador;
        if (emailOrganizador is null)
        {
            int idOrganizador = await _unitOfWork.TorneoRepository.GetIdOrganizadorByIdTorneo(
                createListaTorneoDTO.IdTorneo
            );
            emailOrganizador = (
                await _unitOfWork.UsuarioRepository.GetEmailNickById(idOrganizador)
            ).Email;
        }

        EmailContactoDTO emailContactoDTO = new()
        {
            Email = emailOrganizador,
            Message = createListaTorneoDTO.Nick,
        };

        await _emailApplicacion.SendEmailOrganizadorEnvioListaTorneo(emailContactoDTO);

        return result;
    }

    public async Task<bool> UpdateEstadoLista(UpdateEstadoListaDTO request)
    {
        bool result = await _unitOfWork.ListaRepository.UpdateEstadoLista(request);
        if (!result)
            return false;

        Torneo torneo = await _unitOfWork.TorneoRepository.GetById(request.IdTorneo);
        UsuarioEmailDto usuario = await _unitOfWork.UsuarioRepository.GetEmailNickById(
            request.IdUsuario
        );

        EmailListaDTO emailListaDTO = new()
        {
            EmailTo = usuario.Email,
            NombreTorneo = torneo.NombreTorneo,
            EstadoLista = request.Estado.ToUpper().Trim(),
        };
        try
        {
            await _emailApplicacion.SendEmailModificacionLista(emailListaDTO);
        }
        catch (SmtpException smtpEx)
        {
            Console.WriteLine(smtpEx.Message);
        }

        return true;
    }

    public async Task<bool> UpdateLista(UpdateListaDTO updateListaTorneoDTO)
    {
        Lista lista = await _unitOfWork.ListaRepository.UpdateLista(updateListaTorneoDTO);
        if (lista == null)
            return false;

        InscripcionTorneo inscripcion = await _unitOfWork.InscripcionRepository.GetInscripcionById(
            lista.IdInscripcion
        );
        if (inscripcion == null)
            return false;

        inscripcion.EstadoLista = "ENTREGADA";
        inscripcion.FechaEntregaLista = updateListaTorneoDTO.FechaEntrega;

        await _unitOfWork.InscripcionRepository.Update(inscripcion);
        await MarcarListaConjuntaEntregadaAsync(
            await GetInscripcionesListaConjuntaAsync(inscripcion.IdTorneo, inscripcion.IdUsuario),
            inscripcion.IdInscripcion,
            updateListaTorneoDTO.FechaEntrega
        );

        int idOrganizador = await _unitOfWork.TorneoRepository.GetIdOrganizadorByIdTorneo(
            inscripcion.IdTorneo
        );
        UsuarioEmailDto organizador = await _unitOfWork.UsuarioRepository.GetEmailNickById(
            idOrganizador
        );
        UsuarioEmailDto usuario = await _unitOfWork.UsuarioRepository.GetEmailNickById(
            inscripcion.IdUsuario
        );

        EmailContactoDTO emailContactoDTO = new()
        {
            Email = organizador.Email,
            Message = usuario.Nick,
        };

        await _emailApplicacion.SendEmailOrganizadorEnvioListaTorneo(emailContactoDTO);

        return true;
    }

    /// <summary>
    /// Parejas con lista conjunta: inscripciones de la pareja en el torneo, con la del capitán primero.
    /// Vacía en cualquier otro torneo
    /// </summary>
    private async Task<List<InscripcionTorneo>> GetInscripcionesListaConjuntaAsync(
        int idTorneo,
        int idUsuario
    )
    {
        Torneo? torneo = await _unitOfWork.TorneoRepository.GetTipoYModoListas(idTorneo);
        if (torneo?.TipoTorneo != TorneoType.PAREJAS || !torneo.ListaPorPareja)
            return [];

        Equipo? pareja = await _unitOfWork.InscripcionRepository.GetEquipoByUsuarioAndTorneoAsync(
            idUsuario,
            idTorneo
        );
        if (pareja is null)
            return [];

        return
        [
            .. pareja
                .InscripcionTorneos.Where(i => i.IdTorneo == idTorneo)
                .OrderByDescending(i => i.IdUsuario == pareja.IdCapitan),
        ];
    }

    /// <summary>
    /// Con lista conjunta el compañero también queda con la lista entregada
    /// </summary>
    private async Task MarcarListaConjuntaEntregadaAsync(
        List<InscripcionTorneo> inscripcionesPareja,
        int idInscripcionYaMarcada,
        DateOnly? fechaEntrega
    )
    {
        foreach (
            InscripcionTorneo inscripcion in inscripcionesPareja.Where(i =>
                i.IdInscripcion != idInscripcionYaMarcada
            )
        )
        {
            inscripcion.EstadoLista = "ENTREGADA";
            inscripcion.FechaEntregaLista = fechaEntrega;
            await _unitOfWork.InscripcionRepository.Update(inscripcion);
        }
    }

    public async Task<(byte[] FileBytes, string FileName)> GetListasPdfByTorneo(int idTorneo)
    {
        List<Lista> listas = await GetListasByTorneo(idTorneo);

        Torneo torneo = 
            await _unitOfWork.TorneoRepository.GetById(idTorneo) 
            ?? throw new Exception("Torneo no encontrado");

        string torneoNombre = torneo.NombreTorneo;

        const int listasPorPdf = 10;
        int totalPdfs = (int)Math.Ceiling(listas.Count / (double)listasPorPdf);

        // Si hay 10 o menos listas, devolver un solo PDF
        if (totalPdfs == 1)
        {
            byte[] pdfBytes = Utils.PdfListaGenerator.GenerateListasPdf(listas);
            string fileName = $"Listas_{torneoNombre}.pdf";
            return (pdfBytes, fileName);
        }

        // Generar múltiples PDFs en un ZIP
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            for (int i = 0; i < totalPdfs; i++)
            {
                var listasSubset = listas.Skip(i * listasPorPdf).Take(listasPorPdf).ToList();
                byte[] pdfBytes = Utils.PdfListaGenerator.GenerateListasPdf(listasSubset);
                
                var entry = archive.CreateEntry($"Listas_{torneoNombre}_Parte{i + 1}de{totalPdfs}.pdf");
                using var entryStream = entry.Open();
                await entryStream.WriteAsync(pdfBytes);
            }
        }

        string zipFileName = $"Listas_{torneoNombre}.zip";
        return (memoryStream.ToArray(), zipFileName);
    }

    public async Task<(Stream Stream, string FileName)> GetListasPdfStreamByTorneo(int idTorneo)
    {
        List<Lista> listas = await GetListasByTorneo(idTorneo);

        Torneo torneo = 
            await _unitOfWork.TorneoRepository.GetById(idTorneo) 
            ?? throw new Exception("Torneo no encontrado");

        string torneoNombre = torneo.NombreTorneo;

        const int listasPorPdf = 40;
        int totalPdfs = (int)Math.Ceiling(listas.Count / (double)listasPorPdf);

        // Si hay 40 o menos listas, devolver un solo PDF
        if (totalPdfs == 1)
        {
            byte[] pdfBytes = Utils.PdfListaGenerator.GenerateListasPdf(listas);
            var stream = new MemoryStream(pdfBytes);
            string fileName = $"Listas_{torneoNombre}.pdf";
            return (stream, fileName);
        }

        // Generar múltiples PDFs en un ZIP con streaming
        var memoryStream = new MemoryStream();
        
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            for (int i = 0; i < totalPdfs; i++)
            {
                var listasSubset = listas.Skip(i * listasPorPdf).Take(listasPorPdf).ToList();
                byte[] pdfBytes = Utils.PdfListaGenerator.GenerateListasPdf(listasSubset);
                
                var entry = archive.CreateEntry($"Listas_{torneoNombre}_Parte{i + 1}de{totalPdfs}.pdf");
                using var entryStream = entry.Open();
                await entryStream.WriteAsync(pdfBytes);
            }
        }

        memoryStream.Position = 0;
        string zipFileName = $"Listas_{torneoNombre}.zip";
        return (memoryStream, zipFileName);
    }

    public async Task<List<ListaCompletaDTO>> GetListasCompletasByTorneo(int idTorneo)
    {
        List<Lista> listas = await _unitOfWork.ListaRepository.GetListasByTorneo(idTorneo);

        bool isTeams = false;
        if (listas.Count != 0)
        {
            if (listas[0].IdInscripcionNavigation?.IdEquipo is not null)
                isTeams = true;

            if (isTeams)
                listas = [.. listas
                    .OrderBy(l => l.IdInscripcionNavigation?.IdEquipo)
                    .ThenBy(l => l.IdInscripcionNavigation?.IdUsuarioNavigation?.Nick)];
            else
                listas = [.. listas
                    .OrderBy(l => l.IdInscripcionNavigation?.IdUsuarioNavigation?.Nick)];
        }
        
        return _mapper.Map<List<ListaCompletaDTO>>(listas);
    }

    public async Task<MigracionCloudinaryResultDTO> MigrarListasACloudinaryAsync()
    {
        const string cloudinaryUrl = "https://api.cloudinary.com/v1_1/agyewc29/image/upload";
        const string uploadPreset = "amonsul_listas";
        const int batchSize = 50;

        var resultado = new MigracionCloudinaryResultDTO();
        int lastIdLista = 0;

        while (true)
        {
            List<Lista> listas = await _unitOfWork.ListaRepository.GetListasBase64BatchAsync(lastIdLista, batchSize);
            if (listas.Count == 0) break;

            foreach (Lista lista in listas)
            {
                resultado.TotalProcesados++;
                try
                {
                    using HttpClient httpClient = _httpClientFactory.CreateClient();
                    var formFields = new List<KeyValuePair<string, string>>
                    {
                        new("file", lista.ListaData!),
                        new("upload_preset", uploadPreset),
                    };
                    using var formData = new FormUrlEncodedContent(formFields);

                    HttpResponseMessage response = await httpClient.PostAsync(cloudinaryUrl, formData);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"[Cloudinary] idLista={lista.IdLista} → {(int)response.StatusCode}: {responseBody}");
                        resultado.Fallidos++;
                        resultado.Errores.Add(new MigracionCloudinaryErrorDTO
                        {
                            IdLista = lista.IdLista,
                            Mensaje = $"HTTP {(int)response.StatusCode}: {responseBody}"
                        });
                        continue;
                    }

                    using JsonDocument doc = JsonDocument.Parse(responseBody);
                    if (!doc.RootElement.TryGetProperty("secure_url", out JsonElement secureUrlElement))
                    {
                        resultado.Fallidos++;
                        resultado.Errores.Add(new MigracionCloudinaryErrorDTO
                        {
                            IdLista = lista.IdLista,
                            Mensaje = "No se encontró 'secure_url' en la respuesta de Cloudinary"
                        });
                        continue;
                    }

                    string secureUrl = secureUrlElement.GetString()!;
                    await _unitOfWork.ListaRepository.UpdateListaDataAsync(lista.IdLista, secureUrl);
                    resultado.Exitosos++;
                }
                catch (Exception ex)
                {
                    resultado.Fallidos++;
                    resultado.Errores.Add(new MigracionCloudinaryErrorDTO
                    {
                        IdLista = lista.IdLista,
                        Mensaje = ex.Message
                    });
                }

                lastIdLista = lista.IdLista;
            }

            if (listas.Count < batchSize) break;
        }

        return resultado;
    }
}
