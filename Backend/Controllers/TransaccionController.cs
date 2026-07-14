using Microsoft.AspNetCore.Mvc;
using Backend.Services.Interfaces;
using Backend.DTOs;
using System.Threading.Tasks;

namespace Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransaccionController : ControllerBase
{
    private readonly ITransaccionService _transaccionService;

    public TransaccionController(ITransaccionService transaccionService)
    {
        _transaccionService = transaccionService;
    }

    [HttpPost("proponer")]
    public async Task<IActionResult> ProponerTrato([FromBody] ProponerTratoDto dto)
    {
        var (exito, mensaje) = await _transaccionService.ProponerTratoAsync(dto);
        if (!exito)
        {
            return NotFound(new { message = mensaje });
        }

        return Ok(new { message = mensaje });
    }

    [HttpPost("confirmar")]
    public async Task<IActionResult> ConfirmarTrato([FromBody] ProponerTratoDto dto)
    {
        var (exito, mensaje) = await _transaccionService.ConfirmarTratoAsync(dto);
        if (!exito)
        {
            return BadRequest(new { message = mensaje });
        }

        return Ok(new { message = mensaje });
    }

    [HttpGet("verificar")]
    public async Task<IActionResult> VerificarTransaccionCompletada(int idVendedor, int idComprador)
    {
        var existe = await _transaccionService.VerificarTransaccionCompletadaAsync(idVendedor, idComprador);
        return Ok(new { permitida = existe });
    }
}
