﻿using AS.Domain.DTOs.Usuario;
using AS.Domain.Exceptions;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AS.Infrastructure.Repositories;

public class GanadorRepository(DbamonsulContext dbamonsulContext) : IGanadorRepository
{
    private readonly DbamonsulContext _dbamonsulContext = dbamonsulContext;

    public async Task<bool> Delete(int id)
    {
        try
        {
            var ganador = await _dbamonsulContext.Ganador.FindAsync(id);
            if (ganador == null) return false;
            
            _dbamonsulContext.Ganador.Remove(ganador);
            await _dbamonsulContext.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al eliminar el ganador.", ex);
        }
    }

    public async Task<List<Ganador>> GetAll()
    {
        try
        {
            return await _dbamonsulContext.Ganador
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al obtener los ganadores.", ex);
        }
    }

    public async Task<Ganador> GetById(int id)
    {
        try
        {
            Ganador? ganador = await _dbamonsulContext.Ganador
                .FirstOrDefaultAsync(g => g.IdGanador == id);

            if (ganador == null)
            {
                throw new KeyNotFoundException($"No se encontró un ganador con ID {id}.");
            }

            return ganador;
        }
        catch (Exception ex)
        {
            throw new Exception($"Ocurrió un problema al obtener el ganador con ID {id}.", ex);
        }
    }

    public async Task<bool> Register(Ganador ganador)
    {
        try
        {
            await _dbamonsulContext.Ganador.AddAsync(ganador);
            await _dbamonsulContext.SaveChangesAsync();
            return true; 
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un problema al registrar el ganador.", ex);
        }
    }
}