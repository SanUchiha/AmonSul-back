using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

public class TorneoRepository : ITorneoRepository
{
    private readonly DbamonsulContext _dbamonsulContext;

    public TorneoRepository(DbamonsulContext dbamonsulContext)
    {
        _dbamonsulContext = dbamonsulContext;
    }

    public async Task<List<Torneo>> GetTorneos()
    {
        try
        {
            var response = await _dbamonsulContext.Torneos.ToListAsync();
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrio un problema en el servidor.", ex);
        }
    }

    public Task<Torneo> GetById(int Id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Register(Torneo torneo)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Edit(Torneo torneo)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Delete(int id)
    {
        throw new NotImplementedException();
    }
}
    
