﻿using AS.Application.DTOs.Elo;
using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Usuario;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AS.Utils.Statics;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AS.Application.Services;

public class PartidaAmistosaApplication(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IEmailApplicacion emailApplication) 
        : IPartidaAmistosaApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IEmailApplicacion _emailApplication = emailApplication;

    public async Task<bool> Delete(int id)
    {
       return await _unitOfWork.PartidaAmistosaRepository.Delete(id);
    }

    public async Task<bool> Edit(UpdatePartidaAmistosaDTO partidaAmistosa)
    {
        var result = await _unitOfWork.PartidaAmistosaRepository.Edit(
            _mapper.Map<PartidaAmistosa>(partidaAmistosa));
        return result;
    }

    public async Task<ViewPartidaAmistosaDTO> GetById(int Id)
    {
        PartidaAmistosa partida = await _unitOfWork.PartidaAmistosaRepository.GetById(Id);

        if (partida == null) return null!;

        ViewPartidaAmistosaDTO detallePartida = _mapper.Map<ViewPartidaAmistosaDTO>(partida);
        UsuarioEmailDto usuario1 = await _unitOfWork.UsuarioRepository.GetEmailNickById(detallePartida.IdUsuario1);
        detallePartida.NickUsuario1 = usuario1.Nick;
        UsuarioEmailDto usuario2 = await _unitOfWork.UsuarioRepository.GetEmailNickById(detallePartida.IdUsuario2);
        detallePartida.NickUsuario2 = usuario2.Nick;
        
        if (detallePartida.GanadorPartida != 0)
        {
            detallePartida.NickUsuario2 = usuario2.Nick;
            if (detallePartida.GanadorPartida == detallePartida.IdUsuario2) detallePartida.GanadorPartidaNick = detallePartida.NickUsuario2;
            else detallePartida.GanadorPartidaNick = detallePartida.NickUsuario1;
        }

        return detallePartida;
    }

    public async Task<List<ViewPartidaAmistosaDTO>> GetPartidaAmistosasByUsuario(string email)
    {
        var rawPartidas = await GetPartidasAmistosas();
        if (rawPartidas == null) return [];

        var usuario = await _unitOfWork.UsuarioRepository.GetByEmail(email);
        if (usuario == null) return [];

        return rawPartidas.FindAll(p => p.IdUsuario1 == usuario.IdUsuario || p.IdUsuario2 == usuario.IdUsuario);
    }

    public async Task<List<ViewPartidaAmistosaDTO>> GetPartidaAmistosasByUsuarioPendientes(int idUsuario)
    {
        var listaPartidas = await _unitOfWork.PartidaAmistosaRepository.GetPartidaAmistosasUsuarioById(idUsuario);
        
        var pendientes2 = listaPartidas.FindAll(p =>
            (p.PartidaValidadaUsuario1 == false || p.PartidaValidadaUsuario1 == null) ||
            (p.PartidaValidadaUsuario2 == false || p.PartidaValidadaUsuario2 == null));

        return _mapper.Map<List<ViewPartidaAmistosaDTO>>(pendientes2);
    }

    public async Task<List<ViewPartidaAmistosaDTO>> GetPartidaAmistosasByUsuarioValidadas(int idUsuario)
    {
        List<PartidaAmistosa> listaPartidas = await _unitOfWork.PartidaAmistosaRepository.GetPartidaAmistosasUsuarioById(idUsuario);
        if (listaPartidas.Count == 0) return [];

        listaPartidas = listaPartidas
            .Where(p => (p.PartidaValidadaUsuario1 ?? false) && (p.PartidaValidadaUsuario2 ?? false))
            .ToList();

        List<ViewPartidaAmistosaDTO> partidas = _mapper.Map<List<ViewPartidaAmistosaDTO>>(listaPartidas);

        foreach (var partida in partidas)
        {
            //Usuario usuario1 = await _unitOfWork.UsuarioRepository.GetById(partida.IdUsuario1);
            partida.NickUsuario1 = partida.IdUsuario1Navigation!.Nick;
            //Usuario usuario2 = await _unitOfWork.UsuarioRepository.GetById(partida.IdUsuario2);
            partida.NickUsuario2 = partida.IdUsuario2Navigation!.Nick;

            if (partida.GanadorPartida != 0)
            {
                if (partida.GanadorPartida == partida.IdUsuario2) 
                    partida.GanadorPartidaNick = partida.NickUsuario2;
                else partida.GanadorPartidaNick = partida.NickUsuario1;
            }
        }

        partidas = partidas
            .FindAll(p => (p.IdUsuario1 == idUsuario || p.IdUsuario2 == idUsuario)
                      && (p.PartidaValidadaUsuario1 ?? false)
                      && (p.PartidaValidadaUsuario2 ?? false));

        return partidas;
    }

    public async Task<List<ViewPartidaAmistosaDTO>> GetPartidasAmistosas()
    {
        List<PartidaAmistosa> response = await _unitOfWork.PartidaAmistosaRepository.GetPartidasAmistosas();

        List<ViewPartidaAmistosaDTO> partidas = _mapper.Map<List<ViewPartidaAmistosaDTO>>(response);

        foreach (var partida in partidas)
        {
            var usuario1 = await _unitOfWork.UsuarioRepository.GetById(partida.IdUsuario1);
            partida.NickUsuario1 = usuario1.Nick;
            var usuario2 = await _unitOfWork.UsuarioRepository.GetById(partida.IdUsuario2);
            partida.NickUsuario2 = usuario2.Nick;
            if (partida.GanadorPartida != 0)
            {
                if (partida.GanadorPartida == partida.IdUsuario2) partida.GanadorPartidaNick = partida.NickUsuario2;
                else partida.GanadorPartidaNick = partida.NickUsuario1;
            }
        }

        return partidas;
    }

    public async Task<List<ViewPartidaAmistosaDTO>> GetPartidasAmistosasByUser(int idUser)
    {
        List<PartidaAmistosa> response = 
            await _unitOfWork.PartidaAmistosaRepository.GetPartidaAmistosasUsuarioById(idUser);

        List<ViewPartidaAmistosaDTO> partidasAmistosas = 
            _mapper.Map<List<ViewPartidaAmistosaDTO>>(response);

        foreach (var item in partidasAmistosas)
        {
            if (item.GanadorPartida == item.IdUsuario1) item.GanadorPartidaNick = item.NickUsuario1;            
            else if (item.GanadorPartida == item.IdUsuario2) item.GanadorPartidaNick = item.NickUsuario2;
        }

        return partidasAmistosas;
    }

    public async Task<List<ViewPartidaAmistosaDTO>> GetPartidasAmistosasValidadas()
    {
        List<ViewPartidaAmistosaDTO> partidaAmistosas = await this.GetPartidasAmistosas();

        return partidaAmistosas.Where(p => p.PartidaValidadaUsuario1 == true && p.PartidaValidadaUsuario2 == true).ToList();
    }

    public async Task<bool> Register(CreatePartidaAmistosaDTO request)
    {

        PartidaAmistosa partidaAmistosa = _mapper.Map<PartidaAmistosa>(request);
        partidaAmistosa.EjercitoUsuario1 = request.EjercitoUsuario1;
        partidaAmistosa.EjercitoUsuario2 = request.EjercitoUsuario2;

        if (request.ResultadoUsuario1 == request.ResultadoUsuario2) 
            partidaAmistosa.GanadorPartida = 0;
        else
        {
            if (request.ResultadoUsuario1 > request.ResultadoUsuario2) 
                partidaAmistosa.GanadorPartida = request.IdUsuario1;
            else partidaAmistosa.GanadorPartida = request.IdUsuario2;
        }

        bool registro =
            await _unitOfWork.PartidaAmistosaRepository.Register(partidaAmistosa);

        UsuarioEmailDto email1 = 
            await _unitOfWork.UsuarioRepository.GetEmailNickById(request.IdUsuario1);
        UsuarioEmailDto email2 =
            await _unitOfWork.UsuarioRepository.GetEmailNickById(request.IdUsuario2);

        List<string> listaDestinatarios = [email1.Email, email1.Email];

        if (listaDestinatarios.Count > 0)
            _ = Task.Run(() => 
            _emailApplication.SendEmailNuevaPartida(listaDestinatarios));

        return registro;
    }

    public async Task<bool> ValidarPartidaAmistosa(ValidarPartidaDTO validarPartidaDTO)
    {
        ViewPartidaAmistosaDTO partida = await GetById(validarPartidaDTO.IdPartida);
        if (partida == null) return false;

        Usuario usuario = await _unitOfWork.UsuarioRepository.GetByEmail(validarPartidaDTO.EmailJugador);
        if (usuario == null) return false;

        bool usuario1 = false;
        bool usuario2 = false;
        if (usuario.IdUsuario == partida.IdUsuario1) usuario1 = true;
        else if (usuario.IdUsuario == partida.IdUsuario2) usuario2 = true;
        else return false;

        if (usuario1) partida.PartidaValidadaUsuario1 = true;
        else partida.PartidaValidadaUsuario2 = true;

        UpdatePartidaAmistosaDTO updatePartida = _mapper.Map<UpdatePartidaAmistosaDTO>(partida);

        bool result = await Edit(updatePartida);

        return result;
    }
}
