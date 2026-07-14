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
        public int? IdInventarioUserIntercambio { get; set; }
        public int IdProponente { get; set; }
    }

    [HttpPost("proponer")]
    public async Task<IActionResult> ProponerTrato([FromBody] ProponerTratoDto dto)
    {
        var invUser = await _context.InventarioUsuarios
            .Include(i => i.IdItemNavigation)
            .FirstOrDefaultAsync(i => i.IdInventarioUser == dto.IdInventarioUser && i.IdUsuario == dto.IdVendedor);
        
        if (invUser == null)
            return NotFound(new { message = "Carta no encontrada en el inventario del vendedor." });

        string? nombreCartaIntercambio = null;
        if (dto.IdInventarioUserIntercambio.HasValue && dto.IdInventarioUserIntercambio.Value > 0)
        {
            var swapCard = await _context.InventarioUsuarios
                .Include(i => i.IdItemNavigation)
                .FirstOrDefaultAsync(i => i.IdInventarioUser == dto.IdInventarioUserIntercambio.Value && i.IdUsuario == dto.IdComprador);
            nombreCartaIntercambio = swapCard?.IdItemNavigation?.Nombre;
        }

        var payload = new
        {
            idInventarioUser = dto.IdInventarioUser,
            precio = dto.Precio,
            nombreCarta = invUser.IdItemNavigation?.Nombre ?? "Carta Desconocida",
            idInventarioUserIntercambio = dto.IdInventarioUserIntercambio,
            nombreCartaIntercambio = nombreCartaIntercambio,
            idVendedor = dto.IdVendedor,
            idComprador = dto.IdComprador
        };
        string payloadJson = JsonSerializer.Serialize(payload);
        string mensajeTexto = $"[SISTEMA_PROPUESTA_TRATO]{payloadJson}";

        var mensaje = new Mensaje
        {
            IdRemitente = dto.IdProponente,
            IdDestinatario = dto.IdProponente == dto.IdVendedor ? dto.IdComprador : dto.IdVendedor,
            IdItem = invUser.IdItem,
            Texto = mensajeTexto,
            Estado = false,
            Fecha = DateTime.UtcNow
        };

        _context.Mensajes.Add(mensaje);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Propuesta enviada al chat." });
    }

    [HttpPost("confirmar")]
    public async Task<IActionResult> ConfirmarTrato([FromBody] ProponerTratoDto dto)
    {
        var invUser = await _context.InventarioUsuarios
            .FirstOrDefaultAsync(i => i.IdInventarioUser == dto.IdInventarioUser && i.IdUsuario == dto.IdVendedor);

        if (invUser == null || invUser.Cantidad <= 0)
            return BadRequest(new { message = "La carta ya no está disponible." });

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

        invUser.Cantidad -= 1;
        if (invUser.Cantidad <= 0)
        {
            _context.InventarioUsuarios.Remove(invUser);
        }
        else
        {
            _context.InventarioUsuarios.Update(invUser);
        }

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

        if (dto.IdInventarioUserIntercambio.HasValue && dto.IdInventarioUserIntercambio.Value > 0)
        {
            var invBuyerCard = await _context.InventarioUsuarios
                .FirstOrDefaultAsync(i => i.IdInventarioUser == dto.IdInventarioUserIntercambio.Value && i.IdUsuario == dto.IdComprador);
            if (invBuyerCard != null && invBuyerCard.Cantidad > 0)
            {
                invBuyerCard.Cantidad -= 1;
                if (invBuyerCard.Cantidad <= 0)
                {
                    _context.InventarioUsuarios.Remove(invBuyerCard);
                }
                else
                {
                    _context.InventarioUsuarios.Update(invBuyerCard);
                }

                var invSellerCard = await _context.InventarioUsuarios
                    .FirstOrDefaultAsync(i => i.IdUsuario == dto.IdVendedor && i.IdItem == invBuyerCard.IdItem && i.EstadoFisico == invBuyerCard.EstadoFisico);
                if (invSellerCard != null)
                {
                    invSellerCard.Cantidad += 1;
                    _context.InventarioUsuarios.Update(invSellerCard);
                }
                else
                {
                    var newInvSellerCard = new InventarioUsuario
                    {
                        IdUsuario = dto.IdVendedor,
                        IdItem = invBuyerCard.IdItem,
                        EstadoFisico = invBuyerCard.EstadoFisico,
                        Cantidad = 1
                    };
                    _context.InventarioUsuarios.Add(newInvSellerCard);
                }
            }
        }

        var payloadConfirmacion = new
        {
            idVendedor = dto.IdVendedor,
            idComprador = dto.IdComprador
        };
        string payloadConfirmacionJson = JsonSerializer.Serialize(payloadConfirmacion);

        var mensajeConfirmacion = new Mensaje
        {
            IdRemitente = dto.IdComprador,
            IdDestinatario = dto.IdVendedor,
            Texto = $"[SISTEMA_TRATO_CONFIRMADO]{payloadConfirmacionJson}",
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
