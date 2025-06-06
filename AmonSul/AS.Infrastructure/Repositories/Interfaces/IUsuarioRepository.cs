﻿using AS.Domain.DTOs.Elos;
using AS.Domain.DTOs.Ganador;
using AS.Domain.DTOs.Usuario;
using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario> GetById(int id);
    Task<Usuario> GetByIdFast(int id);
    Task<Usuario> GetByEmail(string email);
    Task<Usuario> GetByNick(string nick);
    Task<List<Usuario>> GetAll();
    Task<bool> EditAsync(Usuario usuario);
    Task<bool> Register(Usuario usuario);
    Task<bool> Delete(string email);
    Task<Usuario> GetUsuario(string id);

    Task<UsuarioEmailDto> GetEmailNickById(int id);
    Task<List<string>> GetAllEmail();
    Task<List<Usuario>> GetUsuariosByIds(List<int> usuarioIds);
    Task<string> GetComunidadNameByIdUser(int idUser);
    Task<List<UsersElosDTO>> GetAllUserWithElo();
    Task<List<GanadorNickDTO>> GetAllSoloNicks();
    Task<List<UsuarioSinEquipoDTO>> GetUsuariosNoInscritosTorneoAsync(int idTorneo);
    Task<List<Usuario>> GetUsuariosByTorneo(int idTorneo);
    Task<Usuario> GetUsuarioSoloById(int idUsuario);
    Task<bool> GetProteccionDatos(int idUsuario);
}
