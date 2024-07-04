using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services;

public class PartidaAmistosaApplication(IUnitOfWork unitOfWork, IMapper mapper) : IPartidaAmistosaApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public Task<bool> Delete(int id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Edit(UpdatePartidaAmistosaDTO partidaAmistosa)
    {
        throw new NotImplementedException();
    }

    public Task<ViewPartidaAmistosaDTO> GetById(int Id)
    {
        throw new NotImplementedException();
    }

    public Task<List<ViewPartidaAmistosaDTO>> GetPartidaAmistosasByUsuario(string email)
    {
        throw new NotImplementedException();
    }

    public async Task<List<ViewPartidaAmistosaDTO>> GetPartidasAmistosas()
    {
        var response = await _unitOfWork.PartidaAmistosaRepository.GetPartidasAmistosas();

        return _mapper.Map<List<ViewPartidaAmistosaDTO>>(response);
    }

    public async Task<bool> Register(CreatePartidaAmistosaDTO request)
    {

        PartidaAmistosa partidaAmistosa = _mapper.Map<PartidaAmistosa>(request);

        if (request.ResultadoUsuario1 == request.ResultadoUsuario2) partidaAmistosa.GanadorPartida = 0;
        else
        {
            if(request.ResultadoUsuario1 > request.ResultadoUsuario2) partidaAmistosa.GanadorPartida = request.IdUsuario1;
            else partidaAmistosa.GanadorPartida = request.IdUsuario2;
        }

        return await _unitOfWork.PartidaAmistosaRepository.Register(partidaAmistosa);
    }

    public Task<bool> ValidarPartidaAmistosa(ValidarPartidaDTO validarPartidaDTO)
    {
        throw new NotImplementedException();
    }
}
