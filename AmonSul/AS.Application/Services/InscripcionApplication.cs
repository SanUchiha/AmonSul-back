﻿using AS.Application.DTOs.Email;
using AS.Application.DTOs.Inscripcion;
using AS.Application.Interfaces;
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

    public async Task<List<InscripcionUsuarioDTO>> GetInscripcionesByTorneo(int idTorneo)
    {
        List<InscripcionTorneo> insc = await _unitOfWork.InscripcionRepository.GetInscripcionesByTorneo(idTorneo);

        return _mapper.Map<List<InscripcionUsuarioDTO>>(insc);
    }

    public async Task<List<InscripcionUsuarioDTO>> GetInscripcionesByUser(int idUsuario)
    {
        List<InscripcionTorneo> insc = 
            await _unitOfWork.InscripcionRepository.GetInscripcionesByUser(idUsuario);

        List<InscripcionUsuarioDTO> result =
            _mapper.Map<List<InscripcionUsuarioDTO>>(insc);
        
        return result;
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
}
