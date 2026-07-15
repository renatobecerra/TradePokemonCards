using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Backend.Controllers;
using Backend.DTOs;
using Backend.Services.Implementations;
using Backend.Models;
using System.Linq;

namespace Backend.Tests.Controllers;

public class ResenaControllerTests
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
        context.Usuarios.Add(new Usuario { IdUsuarios = 1, Nombre = "Comprador", Correo = "a@a.com", Contraseña = "1" });
        context.Usuarios.Add(new Usuario { IdUsuarios = 2, Nombre = "Vendedor", Correo = "b@b.com", Contraseña = "2" });
        
        
        context.Transacciones.Add(new Transaccion { 
            IdTransaccion = 1,
            IdVendedor = 2,
            IdComprador = 1,
            Estado = "Completado",
            Fecha = System.DateTime.Now
        });

        context.SaveChanges();
    }

    [Fact]
    public async Task PostResena_DebeCrearRegistro_CuandoDatosSonValidos()
    {
        // Arrange
        var context = CrearContextoEnMemoria("CrearResena_Valida");
        SembrarDatos(context);
        var controller = new ResenaController(new ResenaService(context));
        var dto = new ResenaDto
        {
            IdUsuarioResenador = 1,
            IdUsuarioResenado = 2,
            Calificacion = 5,
            Texto = "Excelente vendedor"
        };

        // Act
        var result = await controller.PostResena(dto);

        // Assert
        var okResult = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var resenaDb = await context.Reseñas.FirstOrDefaultAsync();
        Assert.NotNull(resenaDb);
        Assert.Equal(5, resenaDb.Calificacion);
        Assert.Equal("Excelente vendedor", resenaDb.Texto);
    }

    [Fact]
    public async Task GetResenasPorUsuario_DebeRetornarLista()
    {
        // Arrange
        var context = CrearContextoEnMemoria("GetResenas_Valida");
        SembrarDatos(context);
        
        context.Reseñas.Add(new Reseña
        {
            IdUsuarioReseñador = 1,
            IdUsuarioReseñado = 2,
            Calificacion = 4,
            Texto = "Buen vendedor"
        });
        context.SaveChanges();

        var controller = new ResenaController(new ResenaService(context));

        // Act
        var result = await controller.GetResenasPorUsuario(2);

        // Assert
        var okResult = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }
}
