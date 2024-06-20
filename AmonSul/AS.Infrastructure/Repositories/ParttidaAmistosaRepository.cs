using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories
{
    public class PartidaAmistosaRepository : IPartidaAmistosaRepository
    {
        private readonly DbamonsulContext _dbamonsulContext;

        public PartidaAmistosaRepository(DbamonsulContext dbamonsulContext)
        {
            _dbamonsulContext = dbamonsulContext;
        }

        public async Task<List<PartidaAmistosa>> GetPartidasAmistosas()
        {
            try
            {
                var response = await _dbamonsulContext.PartidaAmistosas.ToListAsync();
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrio un problema en el servidor.", ex);
            }
        }
       
        public Task<PartidaAmistosa> GetById(int Id)
        {
            throw new NotImplementedException();
        }
      
        public Task<bool> Register(PartidaAmistosa partidaAmistosa)
        {
            throw new NotImplementedException();
        }
       
        public Task<bool> Edit(PartidaAmistosa partidaAmistosa)
        {
            throw new NotImplementedException();
        }
     
        public Task<bool> Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
