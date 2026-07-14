using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Controllers;
using Backend.DTOs;
using Backend.Services.Implementations;
using Backend.Models;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Tests.Controllers;

/// <summary>
/// Tests derivados de los Criterios de Aceptación: Inventario de Cartas
/// </summary>
public class ItemControllerTests
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
        context.Usuarios.Add(new Usuario
        {
            IdUsuarios = 1,
            Nombre = "Ash",
            Apellido = "Ketchum",
            Correo = "ash@pokemon.com",
            Contraseña = "hash123",
            Rol = "Usuario"
        });
        
        var item = new Inventario
        {
            IdItem = 1,
            Nombre = "Pikachu",
            precio = 1000
        };
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

    // ─────────────────────────────────────────────────────────────
    // CA: Añadir Carta al Inventario
    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task AgregarAlInventario_DebeCrearRegistro_CuandoDatosSonValidos()
    {
        // Arrange
        var context = CrearContextoEnMemoria("AgregarAlInventario_Valido");
        SembrarDatos(context);
        var controller = new InventarioController(new InventarioService(context));
        var dto = new CrearItemDto
        {
            IdUsuario = 1,
            Nombre = "Charizard",
            Estado = "Excelente",
            Edicion = "Base Set",
            Precio = 50000,
            Cantidad = 1
        };

        // Act
        var resultado = await controller.AgregarAlInventario(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var itemGuardado = await context.Inventarios.FirstOrDefaultAsync(i => i.Nombre == "Charizard");
        Assert.NotNull(itemGuardado);
        var vinculoGuardado = await context.InventarioUsuarios.FirstOrDefaultAsync(i => i.IdItem == itemGuardado.IdItem && i.IdUsuario == 1);
        Assert.NotNull(vinculoGuardado);
        Assert.Equal("Excelente", vinculoGuardado.EstadoFisico);
    }

    // ─────────────────────────────────────────────────────────────
    // CA: Ver Inventario de Cartas
    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task ObtenerInventario_DebeRetornarCartas_CuandoUsuarioTieneCartas()
    {
        // Arrange
        var context = CrearContextoEnMemoria("ObtenerInventario_ConCartas");
        SembrarDatos(context);
        var controller = new InventarioController(new InventarioService(context));

        // Act
        var resultado = await controller.ObtenerInventario(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var inventario = okResult.Value as IEnumerable<object>;
        Assert.NotNull(inventario);
        Assert.Single(inventario); // Tiene 1 tipo de carta (Pikachu)
    }

    [Fact]
    public async Task ObtenerInventario_DebeRetornarVacio_CuandoUsuarioNoTieneCartas()
    {
        // Arrange
        var context = CrearContextoEnMemoria("ObtenerInventario_SinCartas");
        var controller = new InventarioController(new InventarioService(context));

        // Act
        var resultado = await controller.ObtenerInventario(2); // Usuario 2 no existe/no tiene

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var inventario = okResult.Value as IEnumerable<object>;
        Assert.Empty(inventario);
    }

    // ─────────────────────────────────────────────────────────────
    // CA: Eliminar una Carta del Inventario
    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task EliminarItem_DebeRemoverCarta_CuandoExiste()
    {
        // Arrange
        var context = CrearContextoEnMemoria("EliminarItem_Exitoso");
        SembrarDatos(context);
        var controller = new InventarioController(new InventarioService(context));

        // Act
        var resultado = await controller.EliminarItem(1); // IdInventarioUser = 1

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var actualizado = await context.InventarioUsuarios.FirstOrDefaultAsync(i => i.IdInventarioUser == 1);
        Assert.NotNull(actualizado);
        Assert.Equal(1, actualizado.Cantidad);
    }

    // ─────────────────────────────────────────────────────────────
    // CA: Editar Inventario de Cartas
    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task EditarItem_DebeModificarDatos_CuandoEsValido()
    {
        // Arrange
        var context = CrearContextoEnMemoria("EditarItem_Valido");
        SembrarDatos(context);
        var controller = new InventarioController(new InventarioService(context));
        var dto = new CrearItemDto
        {
            Precio = 1500,
            Estado = "Deteriorado",
            Cantidad = 5
        };

        // Act
        var resultado = await controller.EditarItem(1, dto); // Editar el item con IdInventarioUser 1

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var actualizado = await context.InventarioUsuarios.Include(iu => iu.IdItemNavigation).FirstOrDefaultAsync(i => i.IdInventarioUser == 1);
        Assert.Equal("Deteriorado", actualizado.EstadoFisico);
        Assert.Equal(5, actualizado.Cantidad);
        Assert.Equal(1500, actualizado.IdItemNavigation.precio);
    }
}
