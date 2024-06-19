using AS.Domain.Exceptions;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories
{
    public class FaccionRepository : IFaccionRepository
    {
        private readonly DbamonsulContext _dbamonsulContext;

        public FaccionRepository(DbamonsulContext dbamonsulContext)
        {
            _dbamonsulContext = dbamonsulContext;
        }
        
        public async Task<List<Faccion>> GetFacciones()
        {
            try
            {
                var response = await _dbamonsulContext.Facciones.ToListAsync();
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrio un problema en el servidor.", ex);
            }
        }
       
        public Task<Faccion> GetById(int Id)
        {
            throw new NotImplementedException();
        }
      
        public Task<bool> Register(Faccion faccion)
        {
            throw new NotImplementedException();
        }
       
        public Task<bool> Edit(Faccion faccion)
        {
            throw new NotImplementedException();
        }
     
        public Task<bool> Delete(Faccion faccion)
        {
            throw new NotImplementedException();
        }
    }
}
