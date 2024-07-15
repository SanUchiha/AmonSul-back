using AS.Application.DTOs.Elo;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AS.Application.Services;

public class EloApplication : IEloApplication
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public EloApplication(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<bool> RegisterElo(CreateEloDTO requestElo) => 
        await _unitOfWork.EloRepository.RegisterElo(_mapper.Map<Elo>(requestElo));
    public async Task<Elo> GetEloById(int idElo) => 
        await _unitOfWork.EloRepository.GetEloById(idElo);
    public async Task<List<Elo>> GetElos() => 
        await _unitOfWork.EloRepository.GetElos();
    public async Task<List<Elo>> GetElosByIdUser(int idUsuario) => 
        await _unitOfWork.EloRepository.GetElosByIdUser(idUsuario);
    public async Task<bool> Delete(int idElo) => 
        await _unitOfWork.EloRepository.Delete(idElo);
    public async Task<bool> Edit(Elo elo) => 
        await _unitOfWork.EloRepository.Edit(elo);

    public async Task<ViewEloDTO> GetElo(string email)
    {
        // Conseguir el nick
        var usuario = await _unitOfWork.UsuarioRepository.GetByEmail(email);

        //Conseguimos todos los elos del jugador
        var elos = await GetElosByIdUser(usuario.IdUsuario);
        var elosMapper = _mapper.Map<List<EloDTO>>(elos);

        ViewEloDTO view = new()
        {
            Email = email,
            Nick = usuario.Nick, 
            IdUsuario = usuario.IdUsuario,
            Elos = elosMapper
        };

        return view;
    }

    public async Task<List<ViewEloDTO>> GetAllElos()
    {
        // Me quedo con todos los email en una lista
        var usuarios = await _unitOfWork.UsuarioRepository.GetAll();

        if (usuarios == null) return [];

        List<string> listaEmails = [];

        foreach (var usuario in usuarios)
        {
            listaEmails.Add(usuario.Email);
        }
        
        List<ViewEloDTO> listaViewElos = [];

        foreach (var item in listaEmails)
        {
            var view = await GetElo(item);
            listaViewElos.Add(view);
        }

        return listaViewElos;

    }

    public async Task<int> GetLastElo(int idUsuario)
    {
        Usuario usuario = await _unitOfWork.UsuarioRepository.GetById(idUsuario);

        if(usuario.Email == null) throw new Exception("Usuario no encontrado");

        ViewEloDTO userElo = await GetElo(usuario.Email);

        if (userElo.Elos == null || userElo.Elos.Count == 0)
        {
            return 800;
        }
        EloDTO lastElo = userElo.Elos.OrderByDescending(e => e.FechaElo).FirstOrDefault()!;

        return lastElo?.PuntuacionElo ?? throw new Exception("No se pudo encontrar el Elo más reciente");
    }

    public async Task<List<ClasificacionElo>> GetClasificacion()
    {
        var usuarios = await _unitOfWork.UsuarioRepository.GetAll();

        if (usuarios == null) return [];

        List<string> listaEmails = [];

        foreach (var usuario in usuarios)
        {
            listaEmails.Add(usuario.Email);
        }

        List<ClasificacionElo> clasificacion = [];

        foreach (var item in listaEmails)
        {
            var view = await GetElo(item);
            var obj = _mapper.Map<ClasificacionElo>(view);
            
            //numero partidas
            //victorias, empates, derrotas
            //ultimo elo

            clasificacion.Add(obj);
        }

        return clasificacion;
    }
}
