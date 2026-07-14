using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Controllers;
using Backend.DTOs;
using Backend.Services.Implementations;
using Backend.Models;
using Xunit;
using System;
using System.Threading.Tasks;

namespace Backend.Tests.Controllers;

public class TransaccionControllerTests
{
    private PokemonMarketContext CrearContextoEnMemoria(string dbName)
    {
        var options = new DbContextOptionsBuilder<PokemonMarketContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new PokemonMarketContext(options);
    }

    private void SembrarDatos(PokemonMarketContext context)
    {
        context.Usuarios.Add(new Usuario { IdUsuarios = 1, Nombre = "Vendedor", Correo = "ven@mail.com", Contraseña = "123" });
        context.Usuarios.Add(new Usuario { IdUsuarios = 2, Nombre = "Comprador", Correo = "com@mail.com", Contraseña = "123" });

        var item = new Inventario { IdItem = 1, Nombre = "Mewtwo", precio = 500 };
        context.Inventarios.Add(item);

        context.InventarioUsuarios.Add(new InventarioUsuario
        {
            IdInventarioUser = 1,
            IdUsuario = 1, 
            IdItem = 1,
            Cantidad = 2,
            EstadoFisico = "Buen Estado",
            IdItemNavigation = item
        });

        context.SaveChanges();
    }

    [Fact]
    public async Task ConfirmarTrato_DebeTransferirCartaYCrearTransaccion()
    {
        // Arrange
        var context = CrearContextoEnMemoria("ConfirmarTrato_Exito");
        SembrarDatos(context);
        var controller = new TransaccionController(new TransaccionService(context));

        var dto = new ProponerTratoDto
        {
            IdVendedor = 1,
            IdComprador = 2,
            IdInventarioUser = 1,
            Precio = 500,
            IdProponente = 2
        };

        // Act
        var result = await controller.ConfirmarTrato(dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);

        
        var cartaVendedor = await context.InventarioUsuarios.FirstOrDefaultAsync(i => i.IdUsuario == 1);
        Assert.NotNull(cartaVendedor);
        Assert.Equal(1, cartaVendedor.Cantidad);

        var cartaComprador = await context.InventarioUsuarios.FirstOrDefaultAsync(i => i.IdUsuario == 2);
        Assert.NotNull(cartaComprador); 
        Assert.Equal(1, cartaComprador.Cantidad);

        
        var transaccion = await context.Transacciones.FirstOrDefaultAsync();
        Assert.NotNull(transaccion);
        Assert.Equal(1, transaccion.IdVendedor);
        Assert.Equal(2, transaccion.IdComprador);

        
        var mensaje = await context.Mensajes.FirstOrDefaultAsync();
        Assert.NotNull(mensaje);
        Assert.Contains("[SISTEMA_TRATO_CONFIRMADO]", mensaje.Texto);
    }
}
