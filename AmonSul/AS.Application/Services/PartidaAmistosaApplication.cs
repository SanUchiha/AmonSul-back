using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.Interfaces;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services;

public class PartidaAmistosaApplication : IPartidaAmistosaApplication
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PartidaAmistosaApplication(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<List<PartidaAmistosaDTO>> GetPartidasAmistosas()
    {
        var response = await _unitOfWork.PartidaAmistosaRepository.GetPartidasAmistosas();

        return _mapper.Map<List<PartidaAmistosaDTO>>(response);
    }

    public Task<PartidaAmistosaDTO> GetById(int Id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Register(PartidaAmistosaDTO partidaAmistosaDTO)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Edit(PartidaAmistosaDTO partidaAmistosaDTO)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Delete(int id)
    {
        throw new NotImplementedException();
    }
}
