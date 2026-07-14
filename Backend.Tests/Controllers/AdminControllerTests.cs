using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Controllers;
using Backend.Models;
using Xunit;
using System;
using System.Threading.Tasks;

namespace Backend.Tests.Controllers;

/// <summary>
/// Tests derivados de los Criterios de Aceptación: Reporte de Usuario
/// </summary>
public class AdminControllerTests
{
    private PokemonMarketContext CrearContextoEnMemoria(string dbName)
    {
        var options = new DbContextOptionsBuilder<PokemonMarketContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new PokemonMarketContext(options);
    }

    private void SembrarDatos(PokemonMarketContext context)
    {
        context.Usuarios.AddRange(
            new Usuario { IdUsuarios = 1, Nombre = "User", Apellido = "A", Correo = "a@test.com", Contraseña = "123" },
            new Usuario { IdUsuarios = 2, Nombre = "User", Apellido = "B", Correo = "b@test.com", Contraseña = "123" }
        );
        context.SaveChanges();
    }

    // ─────────────────────────────────────────────────────────────
    // CA: Reportar Usuario
    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task CrearReporte_DebeGuardarEnLaBd_CuandoDatosSonValidos()
    {
        // Arrange
        var context = CrearContextoEnMemoria("CrearReporte_Valido");
        SembrarDatos(context);
        var controller = new AdminController(context);
        var dto = new ReporteDto
        {
            IdUsuarioReportante = 1,
            IdUsuarioReportado = 2,
            Motivo = "Spam y mensajes ofensivos"
        };

        // Act
        var resultado = await controller.CrearReporte(dto);

        // Assert
        var okResult = Assert.IsAssignableFrom<ObjectResult>(resultado);
        Assert.Equal(200, okResult.StatusCode);
        // Validar en la BD si existe
        var reporte = await context.Reportes.FirstOrDefaultAsync(r => r.Motivo == "Spam y mensajes ofensivos");
        Assert.NotNull(reporte);
        Assert.NotNull(okResult.Value);
    }
}
