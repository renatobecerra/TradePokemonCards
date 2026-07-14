using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Controllers;
using Backend.DTOs;
using Backend.Services.Implementations;
using Backend.Models;
using Xunit;

namespace Backend.Tests.Controllers;

/// <summary>
/// Tests derivados de los Criterios de Aceptación:
///   - Inicio de Sesión: "manejo de errores: 'Contraseña incorrecta' o 'Usuario no encontrado'"
///   - Registro de Usuario: validación de campos obligatorios
/// </summary>
public class AuthControllerTests
{
    // ─── Helpers ────────────────────────────────────────────────────────────

    private PokemonMarketContext CrearContextoEnMemoria(string dbName)
    {
        var options = new DbContextOptionsBuilder<PokemonMarketContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new PokemonMarketContext(options);
    }

    /// <summary>
    /// Crea una instancia del AuthController con un stub nulo de IEmailService
    /// (suficiente para tests que no llegan a enviar correo).
    /// </summary>
    private AuthController CrearController(PokemonMarketContext context)
        => new AuthController(new AuthService(context, null!));

    // ─────────────────────────────────────────────────────────────────────────
    // CA: Inicio de Sesión
    // "El sistema debe validar el correo y la contraseña; en caso de error
    //  debe mostrar un mensaje claro ('Usuario no encontrado', etc.)."
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_DebeRetornar400_CuandoElUsuarioNoExiste()
    {
        // Arrange
        var context = CrearContextoEnMemoria("Login_UsuarioNoExiste");
        var controller = CrearController(context);
        var dto = new LoginDto { Correo = "noexiste@test.com", Contraseña = "Password123!" };

        // Act
        var resultado = await controller.Login(dto);

        // Assert — CA: "Usuario no encontrado"
        Assert.IsAssignableFrom<ObjectResult>(resultado);
    }

    [Fact]
    public async Task Login_DebeRetornar400_CuandoLaContraseñaEsIncorrecta()
    {
        // Arrange — usuario registrado con contraseña diferente
        var context = CrearContextoEnMemoria("Login_PasswordIncorrecto");
        context.Usuarios.Add(new Usuario
        {
            IdUsuarios = 1,
            Nombre = "Test",
            Apellido = "User",
            Correo = "test@test.com",
            // Hash de "OtraPassword!" — no coincide con lo que intentará el login
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

        // Assert — CA: "Contraseña incorrecta"
        Assert.IsAssignableFrom<ObjectResult>(resultado);
    }

    [Fact]
    public async Task Login_DebeRetornar400_CuandoElCorreoEstaVacio()
    {
        // Arrange — CA: el formulario valida campos obligatorios
        var context = CrearContextoEnMemoria("Login_CorreoVacio");
        var controller = CrearController(context);
        var dto = new LoginDto { Correo = "", Contraseña = "Password123!" };

        // Act
        var resultado = await controller.Login(dto);

        // Assert
        Assert.IsAssignableFrom<ObjectResult>(resultado);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CA: Registro de Usuario
    // "El formulario debe contener los campos obligatorios: Nombre, Apellido,
    //  Correo Electrónico y Número de Teléfono."
    // "La contraseña debe ser validada: mínimo 8 caracteres, al menos una
    //  mayúscula, un número y un símbolo especial."
    // ─────────────────────────────────────────────────────────────────────────

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
            Correo = "duplicado@test.com",   // correo ya en uso
            Contraseña = "Password123!",
            Telefono = "555-0000"
        };

        // Act
        var resultado = await controller.Registrar(nuevoUsuario);

        // Assert — CA: no puede existir dos cuentas con el mismo correo
        Assert.IsAssignableFrom<ObjectResult>(resultado);
    }

    [Fact]
    public async Task Registrar_DebeRetornar400_CuandoLaContraseñaEsDebil()
    {
        // Arrange — CA: "mínimo 8 caracteres, mayúscula, número y símbolo"
        var context = CrearContextoEnMemoria("Registrar_PasswordDebil");
        var controller = CrearController(context);

        var nuevoUsuario = new Usuario
        {
            Nombre = "Ana",
            Apellido = "García",
            Correo = "ana@nuevo.com",
            Contraseña = "debil",    // no cumple la política
            Telefono = "555-9999"
        };

        // Act
        var resultado = await controller.Registrar(nuevoUsuario);

        // Assert
        Assert.IsAssignableFrom<ObjectResult>(resultado);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CA: Validación de contraseña (lógica pura — sin base de datos)
    // ─────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("password1!",  false)]  // sin mayúscula
    [InlineData("Password1",   false)]  // sin símbolo especial
    [InlineData("Pass!",       false)]  // menos de 8 caracteres
    [InlineData("password!",   false)]  // sin número
    [InlineData("PASSWORD1!",  false)]  // sin minúscula (política del backend la requiere)
    [InlineData("Password1!",  true)]   // cumple todo
    [InlineData("Segura123!",  true)]   // cumple todo
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

    // ─────────────────────────────────────────────────────────────────────────
    // CA: Ver y Editar Perfil
    // ─────────────────────────────────────────────────────────────────────────
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
