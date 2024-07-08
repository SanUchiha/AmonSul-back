using AS.Application.DTOs.Email;
using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.DTOs.Usuario;
using AS.Application.Exceptions;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AS.Utils.Constantes;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AS.Application.Services;

public class PartidaAmistosaApplication(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PartidaAmistosaApplication> logger, IEmailSender emailSender) : IPartidaAmistosaApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<PartidaAmistosaApplication> _logger = logger;
    private readonly IEmailSender _emailSender = emailSender;

    public Task<bool> Delete(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> Edit(UpdatePartidaAmistosaDTO partidaAmistosa)
    {
        return await _unitOfWork.PartidaAmistosaRepository.Edit(
            _mapper.Map<PartidaAmistosa>(partidaAmistosa));
    }

    public async Task<ViewPartidaAmistosaDTO> GetById(int Id)
    {
        var partida = await _unitOfWork.PartidaAmistosaRepository.GetById(Id);

        return _mapper.Map<ViewPartidaAmistosaDTO>(partida);
    }

    public async Task<List<ViewPartidaAmistosaDTO>> GetPartidaAmistosasByUsuario(string email)
    {
        var rawPartidas = await GetPartidasAmistosas();
        if (rawPartidas == null) return [];

        var usuario = await _unitOfWork.UsuarioRepository.GetByEmail(email);
        if (usuario == null) return [];

        return rawPartidas.FindAll(p => p.IdUsuario1 == usuario.IdUsuario || p.IdUsuario2 == usuario.IdUsuario);
    }

    public async Task<List<ViewPartidaAmistosaDTO>> GetPartidaAmistosasByUsuarioPendientes(string email)
    {
        List<ViewPartidaAmistosaDTO> rawPartidasUsuario = await GetPartidaAmistosasByUsuario(email);
        if (rawPartidasUsuario == null) return [];

        var usuario = await _unitOfWork.UsuarioRepository.GetByEmail(email);
        if (usuario == null) return [];

        return rawPartidasUsuario.FindAll(p =>
            (p.IdUsuario1 == usuario.IdUsuario && !(p.PartidaValidadaUsuario1 ?? false))
            || (p.IdUsuario2 == usuario.IdUsuario && !(p.PartidaValidadaUsuario2 ?? false)));
    }

    public async Task<List<ViewPartidaAmistosaDTO>> GetPartidaAmistosasByUsuarioValidadas(string email)
    {
        var rawPartidas = await GetPartidasAmistosas();
        if (rawPartidas == null) return [];

        var usuario = await _unitOfWork.UsuarioRepository.GetByEmail(email);
        if (usuario == null) return [];

        rawPartidas = rawPartidas
            .FindAll(p => (p.IdUsuario1 == usuario.IdUsuario || p.IdUsuario2 == usuario.IdUsuario)
                      && (p.PartidaValidadaUsuario1 ?? false)
                      && (p.PartidaValidadaUsuario2 ?? false));

        foreach (var partida in rawPartidas)
        {
            var usuario1 = await _unitOfWork.UsuarioRepository.GetById(partida.IdUsuario1);
            partida.NickUsuario1 = usuario1.Nick;
            var usuario2 = await _unitOfWork.UsuarioRepository.GetById(partida.IdUsuario2);
            partida.NickUsuario2 = usuario2.Nick;
            if(partida.GanadorPartida != 0) 
            {
                var usuarioGanador = await _unitOfWork.UsuarioRepository.GetById(partida.GanadorPartida);
                partida.GanadorPartidaNick = usuarioGanador.Nick;
            }
            
        }

        return rawPartidas;
    }

    public async Task<List<ViewPartidaAmistosaDTO>> GetPartidasAmistosas()
    {
        List<PartidaAmistosa> response = await _unitOfWork.PartidaAmistosaRepository.GetPartidasAmistosas();

        return _mapper.Map<List<ViewPartidaAmistosaDTO>>(response);
    }

    public async Task<List<ViewPartidaAmistosaDTO>> GetPartidasAmistosasValidadas()
    {
        List<ViewPartidaAmistosaDTO> partidaAmistosas = await this.GetPartidasAmistosas();

        return partidaAmistosas.Where(p => p.PartidaValidadaUsuario1 == true && p.PartidaValidadaUsuario2 == true).ToList();
    }

    public async Task<bool> Register(CreatePartidaAmistosaDTO request)
    {

        PartidaAmistosa partidaAmistosa = _mapper.Map<PartidaAmistosa>(request);

        if (request.ResultadoUsuario1 == request.ResultadoUsuario2) partidaAmistosa.GanadorPartida = 0;
        else
        {
            if (request.ResultadoUsuario1 > request.ResultadoUsuario2) partidaAmistosa.GanadorPartida = request.IdUsuario1;
            else partidaAmistosa.GanadorPartida = request.IdUsuario2;
        }

        var registro = await _unitOfWork.PartidaAmistosaRepository.Register(partidaAmistosa);

        //Conseguir los mails
        var usuario1 = await _unitOfWork.UsuarioRepository.GetById(request.IdUsuario1);
        var usuario2 = await _unitOfWork.UsuarioRepository.GetById(request.IdUsuario1);

        //Envio de mensajes de partida creada
        EmailRegisterDTO emailRegister1DTO = new()
        {
            EmailTo = usuario1.Email,
            Subject = ConstEmailMessage.MessageCreacionPartidaAsunto,
            Body = ConstEmailMessage.MessageCreacionPartidaBody,
        };
        EmailRegisterDTO emailRegister2DTO = new()
        {
            EmailTo = usuario2.Email,
            Subject = ConstEmailMessage.MessageCreacionPartidaAsunto,
            Body = ConstEmailMessage.MessageCreacionPartidaBody,
        };

        try
        {
            await _emailSender.SendEmailRegister(emailRegister1DTO);
            await _emailSender.SendEmailRegister(emailRegister2DTO);
        }
        catch (EmailSendException emailEx)
        {
            _logger.LogError(emailEx, "Error al enviar el correo de creación de partdia. ${emailEx.Message}", emailEx.Message);
        }

        return registro;
    }

    public async Task<bool> ValidarPartidaAmistosa(ValidarPartidaDTO validarPartidaDTO)
    {
        //1. Buscar la partida
        var partida = await GetById(validarPartidaDTO.IdPartida);
        if (partida == null)
        {
            _logger.LogInformation("Partida amistosa no encontrada");
            return false;
        }

        //2. quedarnos con el Id del jugador
        var usuario = await _unitOfWork.UsuarioRepository.GetByEmail(validarPartidaDTO.EmailJugador);
        if (usuario == null)
        {
            _logger.LogInformation("Usuario no encontrado");
            return false;
        }

        bool usuario1 = false; bool usuario2 = false;
        //3. Validar si es jugador 1 o jugador 2
        if (usuario.IdUsuario == partida.IdUsuario1) usuario1 = true;
        else if (usuario.IdUsuario == partida.IdUsuario2) usuario2 = true;
        else return false;

        //4. Validar la partida
        if (usuario1) partida.PartidaValidadaUsuario1 = true;
        else partida.PartidaValidadaUsuario2 = true;

        //5. Guardamos los cambios en la partida

        UpdatePartidaAmistosaDTO updatePartida = _mapper.Map<UpdatePartidaAmistosaDTO>(partida);
        var result = await Edit(updatePartida);

        return true;
    }
}
