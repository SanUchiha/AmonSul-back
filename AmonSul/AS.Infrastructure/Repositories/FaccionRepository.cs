using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

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

    public async Task<Faccion> GetById(int id)
    {
        try
        {
            var response = await _dbamonsulContext.Facciones.FirstOrDefaultAsync(f =>f.IdFaccion == id);
            return response!;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor.", ex);
        }
    }

    public async Task<bool> Register(Faccion faccion)
    {
        try
        {
            var response = await _dbamonsulContext.Facciones.AddAsync(faccion);
            await _dbamonsulContext.SaveChangesAsync();

            if (response == null) return false;
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor.", ex);
        }
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
