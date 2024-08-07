﻿using AS.Application.DTOs.Inscripcion;
using AS.Application.Interfaces;
using AS.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.Api.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class InscripcionController : ControllerBase
{
    private readonly IInscripcionApplication _inscripcionApplication;

    public InscripcionController(IInscripcionApplication inscripcionApplication)
    {
        _inscripcionApplication = inscripcionApplication;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InscripcionTorneo>>> GetInscripciones()
    {
        var inscripciones = await _inscripcionApplication.GetInscripciones();
        return Ok(inscripciones);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InscripcionTorneo>> GetInscripcionById(int id)
    {
        var inscripcion = await _inscripcionApplication.GetInscripcionById(id);
        if (inscripcion == null)
        {
            return NotFound();
        }
        return Ok(inscripcion);
    }

    [HttpGet("byUser/{idUsuario}")]
    public async Task<ActionResult<IList<InscripcionTorneo>>> GetInscripcionesByUser(int idUsuario)
    {
        var inscripciones = await _inscripcionApplication.GetInscripcionesByUser(idUsuario);
        return Ok(inscripciones);
    }

    [HttpGet("byTorneo/{idTorneo}")]
    public async Task<ActionResult<IEnumerable<InscripcionTorneo>>> GetInscripcionesByTorneo(int idTorneo)
    {
        var inscripciones = await _inscripcionApplication.GetInscripcionesByTorneo(idTorneo);
        return Ok(inscripciones);
    }

    [HttpPost]
    public async Task<ActionResult> Register([FromBody] CrearInscripcionDTO inscripcionTorneo)
    {
        var result = await _inscripcionApplication.Register(inscripcionTorneo);
        if (!result)
        {
            return BadRequest();
        }
        return Created();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<InscripcionTorneo>> Delete(int id)
    {
        var inscripcion = await _inscripcionApplication.Delete(id);
        if (inscripcion == null)
        {
            return NotFound();
        }
        return Ok(inscripcion);
    }
}
