using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransaccionController : ControllerBase
{
    private readonly PokemonMarketContext _context;

    public TransaccionController(PokemonMarketContext context)
    {
        _context = context;
    }

    public class ProponerTratoDto
    {
        public int IdVendedor { get; set; }
        public int IdComprador { get; set; }
        public int IdInventarioUser { get; set; }
        public int? Precio { get; set; }
    }

    [HttpPost("proponer")]
    public async Task<IActionResult> ProponerTrato([FromBody] ProponerTratoDto dto)
    {
        // 1. Validate InventarioUser exists
        var invUser = await _context.InventarioUsuarios
            .Include(i => i.IdItemNavigation)
            .FirstOrDefaultAsync(i => i.IdInventarioUser == dto.IdInventarioUser && i.IdUsuario == dto.IdVendedor);
        
        if (invUser == null)
            return NotFound(new { message = "Carta no encontrada en el inventario del vendedor." });

        // 2. Format interactive message payload
        var payload = new
        {
            idInventarioUser = dto.IdInventarioUser,
            precio = dto.Precio,
            nombreCarta = invUser.IdItemNavigation?.Nombre ?? "Carta Desconocida"
        };
        string payloadJson = JsonSerializer.Serialize(payload);
        string mensajeTexto = $"[SISTEMA_PROPUESTA_TRATO]{payloadJson}";

        // 3. Save as a message from Vendedor to Comprador
        var mensaje = new Mensaje
        {
            IdRemitente = dto.IdVendedor,
            IdDestinatario = dto.IdComprador,
            IdItem = invUser.IdItem,
            Texto = mensajeTexto,
            Estado = false, // Not read yet
            Fecha = DateTime.UtcNow
        };

        _context.Mensajes.Add(mensaje);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Propuesta enviada al chat." });
    }

    [HttpPost("confirmar")]
    public async Task<IActionResult> ConfirmarTrato([FromBody] ProponerTratoDto dto)
    {
        // 1. Verify card is still available
        var invUser = await _context.InventarioUsuarios
            .FirstOrDefaultAsync(i => i.IdInventarioUser == dto.IdInventarioUser && i.IdUsuario == dto.IdVendedor);

        if (invUser == null || invUser.Cantidad <= 0)
            return BadRequest(new { message = "La carta ya no está disponible." });

        // 2. Create the Transaccion record
        var transaccion = new Transaccion
        {
            IdVendedor = dto.IdVendedor,
            IdComprador = dto.IdComprador,
            IdInventarioUser = dto.IdInventarioUser,
            Precio = dto.Precio,
            Fecha = DateTime.UtcNow,
            Estado = "Completado"
        };

        _context.Transacciones.Add(transaccion);

        // 3. Deduct stock from seller
        invUser.Cantidad -= 1;
        if (invUser.Cantidad <= 0)
        {
            _context.InventarioUsuarios.Remove(invUser);
        }
        else
        {
            _context.InventarioUsuarios.Update(invUser);
        }

        // 4. Add stock to buyer's inventory
        var invBuyer = await _context.InventarioUsuarios
            .FirstOrDefaultAsync(i => i.IdUsuario == dto.IdComprador && i.IdItem == invUser.IdItem && i.EstadoFisico == invUser.EstadoFisico);

        if (invBuyer != null)
        {
            invBuyer.Cantidad += 1;
            _context.InventarioUsuarios.Update(invBuyer);
        }
        else
        {
            var newInvBuyer = new InventarioUsuario
            {
                IdUsuario = dto.IdComprador,
                IdItem = invUser.IdItem,
                EstadoFisico = invUser.EstadoFisico,
                Cantidad = 1
            };
            _context.InventarioUsuarios.Add(newInvBuyer);
        }

        // Add a system message confirming the trade
        var mensajeConfirmacion = new Mensaje
        {
            IdRemitente = dto.IdComprador,
            IdDestinatario = dto.IdVendedor,
            Texto = "[SISTEMA_TRATO_CONFIRMADO]El trato ha sido confirmado por el comprador.",
            Estado = false,
            Fecha = DateTime.UtcNow
        };
        _context.Mensajes.Add(mensajeConfirmacion);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Trato confirmado exitosamente." });
    }

    [HttpGet("verificar")]
    public async Task<IActionResult> VerificarTransaccionCompletada(int idVendedor, int idComprador)
    {
        var existe = await _context.Transacciones.AnyAsync(t => 
            t.IdVendedor == idVendedor && 
            t.IdComprador == idComprador && 
            t.Estado == "Completado");
            
        return Ok(new { permitida = existe });
    }
}
