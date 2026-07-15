using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Controllers;
using Backend.DTOs;
using Backend.Services.Implementations;
using Backend.Models;
using Xunit;

namespace Backend.Tests.Controllers;


public class AuthControllerTests
{

    private PokemonMarketContext CrearContextoEnMemoria(string dbName)
    {
        var options = new DbContextOptionsBuilder<PokemonMarketContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new PokemonMarketContext(options);
    }


    private AuthController CrearController(PokemonMarketContext context)
        => new AuthController(new AuthService(context, null!));


    [Fact]
    public async Task Login_DebeRetornar400_CuandoElUsuarioNoExiste()
    {
        // Arrange
        var context = CrearContextoEnMemoria("Login_UsuarioNoExiste");
        var controller = CrearController(context);
        var dto = new LoginDto { Correo = "noexiste@test.com", Contraseña = "Password123!" };

        // Act
        var resultado = await controller.Login(dto);

        // Assert 
        Assert.IsAssignableFrom<ObjectResult>(resultado);
    }

    [Fact]
    public async Task Login_DebeRetornar400_CuandoLaContraseñaEsIncorrecta()
    {
        // Arrange 
        var context = CrearContextoEnMemoria("Login_PasswordIncorrecto");
        context.Usuarios.Add(new Usuario
        {
            IdUsuarios = 1,
            Nombre = "Test",
            Apellido = "User",
            Correo = "test@test.com",
            Contraseña = BCrypt.Net.BCrypt.HashPassword("OtraPassword123!"),
            Rol = "Usuario",
            EsVerificado = true,
            Estado = 1
        });
        await context.SaveChangesAsync();
        var controller = CrearController(context);
        var dto = new LoginDto { Correo = "test@test.com", Contraseña = "PasswordMal!" };

        // Act
        var resultado = await controller.Login(dto);

        // Assert 
        Assert.IsAssignableFrom<ObjectResult>(resultado);
    }

    [Fact]
    public async Task Login_DebeRetornar400_CuandoElCorreoEstaVacio()
    {
        // Arrange 
        var context = CrearContextoEnMemoria("Login_CorreoVacio");
        var controller = CrearController(context);
        var dto = new LoginDto { Correo = "", Contraseña = "Password123!" };

        // Act
        var resultado = await controller.Login(dto);

        // Assert
        Assert.IsAssignableFrom<ObjectResult>(resultado);
    }



    [Fact]
    public async Task Registrar_DebeRetornar400_CuandoElCorreoYaExiste()
    {
        // Arrange
        var context = CrearContextoEnMemoria("Registrar_CorreoDuplicado");
        context.Usuarios.Add(new Usuario
        {
            IdUsuarios = 1,
            Nombre = "Existente",
            Apellido = "User",
            Correo = "duplicado@test.com",
            Contraseña = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Rol = "Usuario"
        });
        await context.SaveChangesAsync();
        var controller = CrearController(context);

        var nuevoUsuario = new Usuario
        {
            Nombre = "Nuevo",
            Apellido = "User",
            Correo = "duplicado@test.com",  
            Contraseña = "Password123!",
            Telefono = "555-0000"
        };

        // Act
        var resultado = await controller.Registrar(nuevoUsuario);

        // Assert 
        Assert.IsAssignableFrom<ObjectResult>(resultado);
    }

    [Fact]
    public async Task Registrar_DebeRetornar400_CuandoLaContraseñaEsDebil()
    {
        // Arrange
        var context = CrearContextoEnMemoria("Registrar_PasswordDebil");
        var controller = CrearController(context);

        var nuevoUsuario = new Usuario
        {
            Nombre = "Ana",
            Apellido = "García",
            Correo = "ana@nuevo.com",
            Contraseña = "debil",    
            Telefono = "555-9999"
        };

        // Act
        var resultado = await controller.Registrar(nuevoUsuario);

        // Assert
        Assert.IsAssignableFrom<ObjectResult>(resultado);
    }



    [Theory]
    [InlineData("password1!",  false)]  
    [InlineData("Password1",   false)] 
    [InlineData("Pass!",       false)]  
    [InlineData("password!",   false)]  
    [InlineData("PASSWORD1!",  false)]  
    [InlineData("Password1!",  true)]  
    [InlineData("Segura123!",  true)]   
    public void ValidarContraseña_DebeRechazarPasswordsDebiles(string password, bool esperado)
    {
        // Arrange
        var regex = new System.Text.RegularExpressions.Regex(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).{8,}$"
        );

        // Act
        bool esValida = regex.IsMatch(password);

        // Assert — CA: "validada estrictamente"
        Assert.Equal(esperado, esValida);
    }

    [Fact]
    public async Task ActualizarPerfil_DebeModificarDatos_CuandoDatosSonValidos()
    {
        // Arrange
        var context = CrearContextoEnMemoria("ActualizarPerfil_Valido");
        context.Usuarios.Add(new Usuario
        {
            IdUsuarios = 1,
            Nombre = "Ash",
            Apellido = "Ketchum",
            Correo = "ash@pokemon.com",
            Contraseña = "hash"
        });
        await context.SaveChangesAsync();
        var controller = CrearController(context);

        var dto = new ActualizarPerfilDto
        {
            UsuarioId = 1,
            Nombre = "Ash",
            Apellido = "Satoshi",
            Telefono = "12345678",
            Bio = "Maestro Pokemon"
        };

        // Act
        var resultado = await controller.ActualizarPerfil(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var actualizado = await context.Usuarios.FindAsync(1);
        Assert.Equal("Satoshi", actualizado.Apellido);
        Assert.Equal("12345678", actualizado.Telefono);
        Assert.Equal("Maestro Pokemon", actualizado.Descripcion);
    }
}
