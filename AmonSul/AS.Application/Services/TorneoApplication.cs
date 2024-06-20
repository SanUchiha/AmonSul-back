using AS.Application.DTOs.Torneo;
using AS.Application.Interfaces;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services
{
    public class TorneoApplication : ITorneoApplication
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TorneoApplication(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<List<TorneoDTO>> GetTorneos()
        {
            var response = await _unitOfWork.TorneoRepository.GetTorneos();

            return _mapper.Map<List<TorneoDTO>>(response);
        }

        public Task<TorneoDTO> GetById(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Register(TorneoDTO TorneoDTO)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Edit(TorneoDTO TorneoDTO)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
