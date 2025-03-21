using AS.Application.DTOs.Email;
using AS.Application.DTOs.Inscripcion;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Equipo;
using AS.Domain.DTOs.Inscripcion;
using AS.Domain.DTOs.Torneo;
using AS.Domain.DTOs.Usuario;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;
using System.Net.Mail;

namespace AS.Application.Services;

public class InscripcionApplication(
    IUnitOfWork unitOfWork, 
    IMapper mapper, 
    IEmailApplicacion emailApplicacion) : IInscripcionApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IEmailApplicacion _emailApplicacion = emailApplicacion;

    public async Task<bool> CambiarEstadoLista(ActualizarEstadoLista actualizarEstado)
    {
        InscripcionTorneo inscripcion = await _unitOfWork.InscripcionRepository.GetInscripcionById(actualizarEstado.IdInscripcion);
        if (inscripcion == null) return false;

        TorneoUsuarioDto torneo = await _unitOfWork.TorneoRepository.GetNombreById(inscripcion.IdTorneo);
        if (torneo == null) return false;

        UsuarioEmailDto usuario = await _unitOfWork.UsuarioRepository.GetEmailNickById(inscripcion.IdUsuario);
        if (usuario == null) return false;

        inscripcion.EstadoLista = actualizarEstado.EstadoLista;

        var result = await _unitOfWork.InscripcionRepository.Update(inscripcion);
        if (result == false) return false;

        EmailListaDTO emailListaDTO = new()
        {
            EmailTo = usuario.Email,
            NombreTorneo = torneo.NombreTorneo,
            EstadoLista = actualizarEstado.EstadoLista!
        };
        try
        {
            await _emailApplicacion.SendEmailModificacionLista(emailListaDTO);
        }
        catch (SmtpException smtpEx)
        {
            Console.WriteLine(smtpEx.Message);
        }

        return result;
    }

    public async Task<bool> CambiarEstadoPago(ActualizarEstadoPago actualizarEstado)
    {
        InscripcionTorneo inscripcion = await _unitOfWork.InscripcionRepository.GetInscripcionById(actualizarEstado.IdInscripcion);
        if (inscripcion == null) return false;

        TorneoUsuarioDto torneo = await _unitOfWork.TorneoRepository.GetNombreById(inscripcion.IdTorneo);
        if (torneo == null) return false;

        UsuarioEmailDto usuario = await _unitOfWork.UsuarioRepository.GetEmailNickById(inscripcion.IdUsuario);
        if (usuario == null) return false;

        inscripcion.EsPago = actualizarEstado.EsPago;

        var result = await _unitOfWork.InscripcionRepository.Update(inscripcion);
        if (result == false) return false;

        EmailPagoDTO emailPagoDTO = new()
        {
            EmailTo = usuario.Email,
            NombreTorneo = torneo.NombreTorneo,
            EstadoPago = actualizarEstado.EsPago!
        };

        try
        {
            await _emailApplicacion.SendEmailModificacionPago(emailPagoDTO);
        }
        catch (SmtpException smtpEx)
        {
            Console.WriteLine(smtpEx.Message);
        }

        return result;
    }

    public async Task<bool> CreaInsciprcionEquipo(CreateEquipoDTO createEquipoDTO)
    {
        Equipo equipo = new()
        {
            IdCapitan = createEquipoDTO.IdCapitan,
            NombreEquipo = createEquipoDTO.NombreEquipo
        };
        Equipo equipoCreated = await _unitOfWork.InscripcionRepository.CreateEquipoAsync(equipo);

        if(equipoCreated == null) return false;

        if(createEquipoDTO.Miembros!.Count == 0) return false;   

        foreach (var item in createEquipoDTO.Miembros)
        {
            EquipoUsuario equipoUsuario = new()
            {
                IdEquipo = equipoCreated.IdEquipo,
                IdUsuario = item
            };
            await _unitOfWork.InscripcionRepository.AddUsuarioToEquipoAsync(equipoUsuario);

            CrearInscripcionEquipoDTO crearInscripcionEquipoDTO = new()
            {
                IdTorneo = createEquipoDTO.IdTorneo,
                IdUsuario = item,
                IdEquipo = equipoCreated.IdEquipo
            };

            InscripcionTorneo inscripcion = _mapper.Map<InscripcionTorneo>(crearInscripcionEquipoDTO);
            await _unitOfWork.InscripcionRepository.Register(inscripcion);
        }

        EquipoUsuario equipoUsuarioCapitan = new()
        {
            IdEquipo = equipoCreated.IdEquipo,
            IdUsuario = createEquipoDTO.IdCapitan
        };
        await _unitOfWork.InscripcionRepository.AddUsuarioToEquipoAsync(equipoUsuarioCapitan);

        CrearInscripcionEquipoDTO crearInscripcionEquipoCapitanDTO = new()
        {
            IdTorneo = createEquipoDTO.IdTorneo,
            IdUsuario = createEquipoDTO.IdCapitan,
            IdEquipo = equipoCreated.IdEquipo
        };

        InscripcionTorneo inscripcionCapitan = _mapper.Map<InscripcionTorneo>(crearInscripcionEquipoCapitanDTO);
        bool result = await _unitOfWork.InscripcionRepository.Register(inscripcionCapitan);

        return result;
    }

    public async Task<InscripcionTorneo> Delete(int id)
    {
        return await _unitOfWork.InscripcionRepository.Delete(id);
    }

    public async Task<InscripcionTorneoDTO> GetInscripcionById(int Id)
    {
        InscripcionTorneo inscripcion = await _unitOfWork.InscripcionRepository.GetInscripcionById(Id);

        InscripcionTorneoDTO inscripcionDTO = _mapper.Map<InscripcionTorneoDTO>(inscripcion);

            if (inscripcion.Lista.Count > 0)
            {
                inscripcionDTO.ListaData = inscripcion.Lista.ToList()[0].ListaData;
                inscripcionDTO.IdLista = inscripcion.Lista.ToList()[0].IdLista;
                inscripcionDTO.Ejercito = inscripcion.Lista.ToList()[0].Ejercito;
                inscripcionDTO.FechaEntregaLista = inscripcion.Lista.ToList()[0].FechaEntrega;
                    if (inscripcion.EstadoLista == "NO ENTREGADA") inscripcionDTO.EstadoLista = "ENTREGADA";
            }
            
        return inscripcionDTO;
    }

    public async Task<List<InscripcionTorneo>> GetInscripciones()
    {
        return await _unitOfWork.InscripcionRepository.GetInscripciones();
    }

    public async Task<List<InscripcionUsuarioIndividualDTO>> GetInscripcionesByTorneo(int idTorneo)
    {
        List<InscripcionTorneo> insc = await _unitOfWork.InscripcionRepository.GetInscripcionesByTorneo(idTorneo);

        return _mapper.Map<List<InscripcionUsuarioIndividualDTO>>(insc);
    }

    public async Task<List<InscripcionUsuarioIndividualDTO>> GetInscripcionesIndividualByUser(int idUsuario)
    {
        List<InscripcionTorneo> inscripcionTorneos = 
            await _unitOfWork.InscripcionRepository.GetInscripcionesIndividualByUser(idUsuario);

        List<InscripcionUsuarioIndividualDTO> inscripcionUsuarioDTOs =
            _mapper.Map<List<InscripcionUsuarioIndividualDTO>>(inscripcionTorneos);
        
        return inscripcionUsuarioDTOs;
    }

    public async Task<bool> Register(CrearInscripcionDTO inscripcionTorneo)
    {
        UsuarioEmailDto usuario = await _unitOfWork.UsuarioRepository.GetEmailNickById(inscripcionTorneo.IdUsuario);
        if (usuario == null) return false;

        TorneoUsuarioDto torneo = await _unitOfWork.TorneoRepository.GetNombreById(inscripcionTorneo.IdTorneo);
        if (torneo == null) return false;

        UsuarioEmailDto organizador = await _unitOfWork.UsuarioRepository.GetEmailNickById(torneo.IdUsuario);
        if (organizador == null) return false;

        bool registro = await _unitOfWork.InscripcionRepository.Register(
            _mapper.Map<InscripcionTorneo>(inscripcionTorneo));

        EmailContactoDTO emailContactoDTO = new()
        {
            Email = usuario.Email,
            Message = torneo.NombreTorneo,
        };

        try 
        {
            await _emailApplicacion.SendEmailRegistroTorneo(emailContactoDTO);

            emailContactoDTO.Email = organizador.Email;
            emailContactoDTO.Message = usuario.Nick;
            await _emailApplicacion.SendEmailOrganizadorNuevoRegistro(emailContactoDTO);
        }
        catch (SmtpException smtpEx)
        {
            Console.WriteLine(smtpEx.Message);
        }

        return registro;
    }

    public async Task<List<InscripcionUsuarioEquipoDTO>> GetInscripcionEquipoByIdAsync(int idUser)
    {
        List<InscripcionTorneo> inscripcionTorneos =
            await _unitOfWork.InscripcionRepository.GetInscripcionesEquipoByUser(idUser);

        List<InscripcionUsuarioEquipoDTO> inscripcionUsuarioDTOs =
            _mapper.Map<List<InscripcionUsuarioEquipoDTO>>(inscripcionTorneos);

        return inscripcionUsuarioDTOs;
    }

    public async Task<InscripcionEquipoDTO> GetInscripcionEquipo(int idInscripcion)
    {
        InscripcionTorneo inscripcion = await _unitOfWork.InscripcionRepository.GetInscripcionById(idInscripcion);
        Equipo? equipo = await _unitOfWork.InscripcionRepository.GetEquipoByIdAsync(inscripcion.IdEquipo!.Value);
        List<InscripcionTorneo> inscripciones = await _unitOfWork.InscripcionRepository.GetAllInscripcionesByEquipoAsync(inscripcion.IdEquipo!.Value);

        List<ComponentesEquipoDTO> componentesEquipoDTOs = _mapper.Map<List<ComponentesEquipoDTO>>(inscripciones);
        InscripcionEquipoDTO inscripcionEquipoDTO = _mapper.Map<InscripcionEquipoDTO>(inscripcion);

        inscripcionEquipoDTO.IdCapitan = equipo!.IdCapitan;
        inscripcionEquipoDTO.NombreEquipo = equipo.NombreEquipo;

        foreach (var item in componentesEquipoDTOs)
        {
            if (inscripcionEquipoDTO.IdCapitan == item.IdUsuario) item.EsCapitan = true;
            var nick = await _unitOfWork.UsuarioRepository.GetEmailNickById(item.IdUsuario);
            item.Nick = nick.Nick;
        }
        inscripcionEquipoDTO.ComponentesEquipoDTO = componentesEquipoDTOs;

        inscripcionEquipoDTO.IdOrganizador= await _unitOfWork.TorneoRepository.GetIdOrganizadorByIdTorneo(inscripcionEquipoDTO.IdTorneo);

        UsuarioEmailDto organizador = await _unitOfWork.UsuarioRepository.GetEmailNickById(inscripcionEquipoDTO.IdOrganizador);

        inscripcionEquipoDTO.EmailOrganizador = organizador.Email;

        return inscripcionEquipoDTO;
    }

    public async Task<List<EquipoDTO>> GetInscripcionesEquipoByTorneoAsync(int idTorneo)
    {
        List<EquipoDTO> equipos = await _unitOfWork.InscripcionRepository.GetAllEquiposByTorneoAsync(idTorneo);
        return equipos;
    }

    public async Task<bool> EstaApuntadoAsync(int idUsuario, int idTorneo)
    {
        return await _unitOfWork.InscripcionRepository.EstaApuntadoAsync(idUsuario, idTorneo);
    }

    public async Task<bool> DeleteEquipo(int idEquipo) => 
        await _unitOfWork.InscripcionRepository.DeleteEquipoAsync(idEquipo);

    public async Task<bool> RegisterMiembroAsync(CreateMiembroEquipoDTO createMiembroEquipoDTO)
    {
        EquipoUsuario equipoUsuario = new()
        {
            IdEquipo = createMiembroEquipoDTO.IdEquipo,
            IdUsuario = createMiembroEquipoDTO.IdUsuario,
        };
        bool isAddEquipo =  await _unitOfWork.InscripcionRepository.AddUsuarioToEquipoAsync(equipoUsuario);
        if (!isAddEquipo) return false;

        CrearInscripcionEquipoDTO crearInscripcionEquipoDTO = new()
        {
            IdTorneo = createMiembroEquipoDTO.IdTorneo,
            IdUsuario = createMiembroEquipoDTO.IdUsuario,
            IdEquipo = createMiembroEquipoDTO.IdEquipo
        };

        InscripcionTorneo inscripcion = _mapper.Map<InscripcionTorneo>(crearInscripcionEquipoDTO);
        bool isCreated = await _unitOfWork.InscripcionRepository.Register(inscripcion);
        if (!isCreated) return false;

        return true;
    }

    public async Task<bool> DeleteMiembroAsync(int idInscripcion) => 
        await _unitOfWork.InscripcionRepository.DeleteMiembroAsync(idInscripcion);

    public async Task<bool> CambiarEstadoPagoEquipo(ActualizarEstadoPagoEquipo actualizarEstadoPagoEquipo)
    {
        List<InscripcionTorneo> inscripciones = await _unitOfWork.InscripcionRepository.GetAllInscripcionesByEquipoAsync(actualizarEstadoPagoEquipo.IdEquipo);

        foreach (var item in inscripciones)
        {
            item.EsPago = actualizarEstadoPagoEquipo.EsPago;
            await _unitOfWork.InscripcionRepository.Update(item);
        }

        TorneoUsuarioDto torneo = await _unitOfWork.TorneoRepository.GetNombreById(inscripciones[0].IdTorneo);
        if (torneo == null) return false;

        Equipo? equipo = await _unitOfWork.InscripcionRepository.GetEquipoByIdAsync(inscripciones[0].IdEquipo!.Value);
        if (equipo == null) return false;

        UsuarioEmailDto usuario = await _unitOfWork.UsuarioRepository.GetEmailNickById(equipo.IdCapitan);
        if (usuario == null) return false;

        EmailPagoDTO emailPagoDTO = new()
        {
            EmailTo = usuario.Email,
            NombreTorneo = torneo.NombreTorneo,
            EstadoPago = actualizarEstadoPagoEquipo.EsPago!
        };

        try
        {
            await _emailApplicacion.SendEmailModificacionPago(emailPagoDTO);
        }
        catch (SmtpException smtpEx)
        {
            Console.WriteLine(smtpEx.Message);
        }

        return true;
    }
}
