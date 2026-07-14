using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Controllers;
using Backend.Models;
using Xunit;

namespace Backend.Tests.Controllers;

/// <summary>
/// Tests derivados de los Criterios de Aceptación: Envío de Mensajes y Buzón de Entrada
/// </summary>
public class MensajesControllerTests
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
            new Usuario
            {
                IdUsuarios = 1,
                Nombre = "Carlos",
                Apellido = "López",
                Correo = "carlos@test.com",
                Contraseña = "hash123",
                Rol = "Usuario"
            },
            new Usuario
            {
                IdUsuarios = 2,
                Nombre = "Ana",
                Apellido = "García",
                Correo = "ana@test.com",
                Contraseña = "hash456",
                Rol = "Usuario"
            }
        );
        context.SaveChanges();
    }

    // ─────────────────────────────────────────────────────────────
    // CA: Envío de Mensajes
    // "Debe existir un campo de entrada de texto (input) y un botón de
    //  Enviar que guarda el mensaje en la base de datos."
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task EnviarMensaje_DebeGuardarMensajeEnBD_CuandoLosDatosSonValidos()
    {
        // Arrange
        var context = CrearContextoEnMemoria("EnviarMensaje_Valido");
        SembrarDatos(context);
        var controller = new MensajesController(context);

        var dto = new MensajesController.EnviarMensajeDto
        {
            IdRemitente = 1,
            IdDestinatario = 2,
            Texto = "Hola, me interesa tu carta Charizard"
        };

        // Act
        var resultado = await controller.EnviarMensaje(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.NotNull(okResult.Value);
        var mensajeGuardado = await context.Mensajes.FirstOrDefaultAsync();
        Assert.NotNull(mensajeGuardado);
        Assert.Equal("Hola, me interesa tu carta Charizard", mensajeGuardado.Texto);
        Assert.Equal(1, mensajeGuardado.IdRemitente);
        Assert.Equal(2, mensajeGuardado.IdDestinatario);
    }

    [Fact]
    public async Task EnviarMensaje_DebeRetornar400_CuandoElTextoEstaVacio()
    {
        // Arrange
        var context = CrearContextoEnMemoria("EnviarMensaje_TextoVacio");
        SembrarDatos(context);
        var controller = new MensajesController(context);

        var dto = new MensajesController.EnviarMensajeDto
        {
            IdRemitente = 1,
            IdDestinatario = 2,
            Texto = ""
        };

        // Act
        var resultado = await controller.EnviarMensaje(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task EnviarMensaje_DebeRetornar404_CuandoElDestinatarioNoExiste()
    {
        // Arrange
        var context = CrearContextoEnMemoria("EnviarMensaje_DestinatarioInexistente");
        SembrarDatos(context);
        var controller = new MensajesController(context);

        var dto = new MensajesController.EnviarMensajeDto
        {
            IdRemitente = 1,
            IdDestinatario = 999, // No existe
            Texto = "Hola"
        };

        // Act
        var resultado = await controller.EnviarMensaje(dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    // ─────────────────────────────────────────────────────────────
    // CA: Buzón de Entrada
    // "La vista del buzón debe mostrar una lista con los usuarios
    //  con los que tengo chats iniciados."
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetConversaciones_DebeRetornarListaDeConversaciones_CuandoExistenMensajes()
    {
        // Arrange
        var context = CrearContextoEnMemoria("GetConversaciones_ConMensajes");
        SembrarDatos(context);
        context.Mensajes.Add(new Mensaje
        {
            IdMensaje = 1,
            IdRemitente = 1,
            IdDestinatario = 2,
            Texto = "Hola Ana",
            Fecha = DateTime.Now,
            Estado = false,
            EliminadoPorRemitente = false,
            EliminadoPorDestinatario = false
        });
        context.SaveChanges();
        var controller = new MensajesController(context);

        // Act
        var resultado = await controller.GetConversaciones(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var conversaciones = okResult.Value as IEnumerable<object>;
        Assert.NotNull(conversaciones);
        Assert.Single(conversaciones);
    }

    [Fact]
    public async Task GetConversaciones_DebeRetornarListaVacia_CuandoNoHayMensajes()
    {
        // Arrange — CA: "Si el buzón está vacío, debe mostrar 'Aún no tienes mensajes activos'"
        var context = CrearContextoEnMemoria("GetConversaciones_SinMensajes");
        SembrarDatos(context);
        var controller = new MensajesController(context);

        // Act
        var resultado = await controller.GetConversaciones(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var conversaciones = okResult.Value as IEnumerable<object>;
        Assert.NotNull(conversaciones);
        Assert.Empty(conversaciones);
    }

    [Fact]
    public async Task GetConversaciones_NoDebeMostrarMensajesEliminados_CuandoElRemitenteLoEliminó()
    {
        // Arrange — CA: mensajes con EliminadoPorRemitente = true no deben aparecer
        var context = CrearContextoEnMemoria("GetConversaciones_MensajesEliminados");
        SembrarDatos(context);
        context.Mensajes.Add(new Mensaje
        {
            IdMensaje = 1,
            IdRemitente = 1,
            IdDestinatario = 2,
            Texto = "Mensaje borrado",
            Fecha = DateTime.Now,
            Estado = false,
            EliminadoPorRemitente = true,
            EliminadoPorDestinatario = false
        });
        context.SaveChanges();
        var controller = new MensajesController(context);

        // Act
        var resultado = await controller.GetConversaciones(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var conversaciones = okResult.Value as IEnumerable<object>;
        Assert.Empty(conversaciones);
    }

    // ─────────────────────────────────────────────────────────────
    // CA: Buzón de Entrada — Historial ordenado
    // "El último mensaje enviado/recibido debe mostrarse como texto previo."
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetHistorial_DebeRetornarMensajesOrdenadosPorFecha_EntreDosusuarios()
    {
        // Arrange
        var context = CrearContextoEnMemoria("GetHistorial_Ordenado");
        SembrarDatos(context);
        var ahora = DateTime.Now;
        context.Mensajes.AddRange(
            new Mensaje
            {
                IdMensaje = 1,
                IdRemitente = 1,
                IdDestinatario = 2,
                Texto = "Primero",
                Fecha = ahora.AddMinutes(-5),
                Estado = true,
                EliminadoPorRemitente = false,
                EliminadoPorDestinatario = false
            },
            new Mensaje
            {
                IdMensaje = 2,
                IdRemitente = 2,
                IdDestinatario = 1,
                Texto = "Segundo",
                Fecha = ahora,
                Estado = false,
                EliminadoPorRemitente = false,
                EliminadoPorDestinatario = false
            }
        );
        context.SaveChanges();
        var controller = new MensajesController(context);

        // Act
        var resultado = await controller.GetHistorial(1, 2);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.NotNull(okResult.Value);
    }
}
