using Backend.Models;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Google.Apis.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Backend.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private const string PasswordPolicyMessage = "La contraseña debe tener al menos 8 caracteres, una mayúscula, un número y un símbolo.";
        private static readonly Regex PasswordPolicy = new(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]).{8,}$", RegexOptions.Compiled);

        private readonly PokemonMarketContext _context;
        private readonly IEmailService _emailService;

        public AuthService(PokemonMarketContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        private static bool CumplePoliticaPassword(string? password)
        {
            return !string.IsNullOrWhiteSpace(password) && PasswordPolicy.IsMatch(password);
        }

        public async Task<(bool exito, string mensaje, object? usuario)> GoogleLoginAsync(string credential)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string>() { "929961623910-oas1o9fve1r93c1mppd2j7f694p0oht6.apps.googleusercontent.com" }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == payload.Email);

            if (usuario == null)
            {
                usuario = new Usuario
                {
                    Nombre = payload.GivenName,
                    Apellido = payload.FamilyName,
                    Correo = payload.Email,
                    Contraseña = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    EsVerificado = true,
                    Rol = "Usuario",
                    FechaRegistro = DateTime.Now,
                    ImgPerfil = payload.Picture,
                    EstadoPresencia = 1
                };
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
            }

            if (usuario.Estado == 0)
            {
                if (usuario.FechaDesbaneo != null && usuario.FechaDesbaneo.Value <= DateTime.Now)
                {
                    usuario.Estado = 1;
                    usuario.MotivoBaneo = null;
                    usuario.FechaDesbaneo = null;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    string fechaMsg = usuario.FechaDesbaneo != null 
                        ? usuario.FechaDesbaneo.Value.ToString("dd/MM/yyyy HH:mm") 
                        : "Indefinido";
                    return (false, $"Tu cuenta ha sido suspendida. Motivo: {usuario.MotivoBaneo ?? "Incumplimiento de las reglas"}. Suspensión activa hasta: {fechaMsg}.", null);
                }
            }

            var returnUsuario = new
            {
                id = usuario.IdUsuarios,
                nombre = usuario.Nombre,
                apellido = usuario.Apellido,
                rol = usuario.Rol,
                foto = usuario.ImgPerfil,
                estadoPresencia = usuario.EstadoPresencia,
                bio = usuario.Descripcion
            };

            return (true, "Inicio de sesión con Google exitoso.", returnUsuario);
        }

        public async Task<(bool exito, string mensaje)> CambiarPresenciaAsync(CambiarPresenciaDto datos)
        {
            var usuario = await _context.Usuarios.FindAsync(datos.UsuarioId);
            if (usuario == null) return (false, "Usuario no encontrado");

            if (datos.NuevoEstado < 0 || datos.NuevoEstado > 2)
            {
                return (false, "Estado no válido");
            }

            usuario.EstadoPresencia = (sbyte)datos.NuevoEstado;
            await _context.SaveChangesAsync();

            return (true, "Estado actualizado");
        }

        public async Task<(bool exito, string mensaje, object? usuario)> RegistrarAsync(Usuario nuevoUsuario)
        {
            var existeCorreo = await _context.Usuarios.AnyAsync(u => u.Correo == nuevoUsuario.Correo);
            if (existeCorreo)
            {
                return (false, "El correo electrónico ya está registrado.", null);
            }

            if (!CumplePoliticaPassword(nuevoUsuario.Contraseña))
            {
                return (false, PasswordPolicyMessage, null);
            }

            var random = new Random();
            nuevoUsuario.CodigoVerificacion = random.Next(100000, 999999).ToString();
            nuevoUsuario.EsVerificado = false;

            nuevoUsuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(nuevoUsuario.Contraseña);

            nuevoUsuario.FechaRegistro = DateTime.Now;
            if (string.IsNullOrEmpty(nuevoUsuario.Rol)) nuevoUsuario.Rol = "Usuario";
            if (nuevoUsuario.Calificacion == null || nuevoUsuario.Calificacion == 0) nuevoUsuario.Calificacion = 5.00m;
            nuevoUsuario.Estado = 1;

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            _ = Task.Run(async () => {
                try {
                    await _emailService.EnviarCodigoVerificacionAsync(nuevoUsuario.Correo, nuevoUsuario.Nombre, nuevoUsuario.CodigoVerificacion);
                } catch (Exception ex) {
                    Console.WriteLine("Error enviando mail en segundo plano: " + ex.Message);
                }
            });

            return (true, "Usuario registrado exitosamente. Por favor, revisa tu correo.", null);
        }

        public async Task<(bool exito, string mensaje, object? usuario)> VerificarAsync(VerificarDto datosVerificacion)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == datosVerificacion.Correo);

            if (usuario == null)
            {
                return (false, "Usuario no encontrado.", null);
            }

            if (usuario.CodigoVerificacion == datosVerificacion.Codigo)
            {
                usuario.EsVerificado = true;
                usuario.CodigoVerificacion = null;
                await _context.SaveChangesAsync();
                return (true, "Cuenta verificada con éxito.", null);
            }
            else
            {
                return (false, "Código de verificación incorrecto.", null);
            }
        }

        public async Task<(bool exito, string mensaje)> SolicitarRecuperacionAsync(RecoveryRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Correo))
            {
                return (false, "El correo es obligatorio.");
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == request.Correo);
            if (usuario == null)
            {
                return (false, "El correo no está registrado en POKET.");
            }

            var random = new Random();
            usuario.CodigoRecuperacion = random.Next(100000, 999999).ToString();
            await _context.SaveChangesAsync();

            _ = Task.Run(async () => {
                try {
                    await _emailService.EnviarCodigoRecuperacionAsync(usuario.Correo, usuario.Nombre, usuario.CodigoRecuperacion);
                } catch (Exception ex) {
                    Console.WriteLine("Error enviando recovery mail: " + ex.Message);
                }
            });

            return (true, "Código enviado. Revisa tu bandeja de entrada.");
        }

        public async Task<(bool exito, string mensaje)> ValidarCodigoRecuperacionAsync(VerificarDto datos)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == datos.Correo);
            
            if (usuario == null)
            {
                return (false, "El correo no está registrado.");
            }

            if (usuario.CodigoRecuperacion != datos.Codigo)
            {
                return (false, "Código de recuperación incorrecto.");
            }

            return (true, "Código correcto. Puedes cambiar tu contraseña.");
        }

        public async Task<(bool exito, string mensaje)> ResetearPasswordAsync(ResetPasswordDto datos)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == datos.Correo);
            
            if (usuario == null)
            {
                return (false, "El usuario no existe.");
            }

            if (usuario.CodigoRecuperacion != datos.Codigo)
            {
                return (false, "El código es incorrecto o ha expirado.");
            }

            if (!CumplePoliticaPassword(datos.NuevaPassword))
            {
                return (false, PasswordPolicyMessage);
            }

            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(datos.NuevaPassword);
            usuario.CodigoRecuperacion = null;
            
            await _context.SaveChangesAsync();

            return (true, "Contraseña actualizada exitosamente.");
        }

        public async Task<(bool exito, string mensaje, object? usuario)> LoginAsync(LoginDto datosLogin)
        {
            var usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == datosLogin.Correo);
            if(usuarioExistente == null)
            {
                return (false, "Correo o Contraseña incorrectas.", null);
            }

            if (usuarioExistente.EsVerificado != true)
            {
                return (false, "requiere_verificacion", null);
            }

            bool claveCorrecta = BCrypt.Net.BCrypt.Verify(datosLogin.Contraseña, usuarioExistente.Contraseña);
            if (!claveCorrecta)
            {
                return (false, "Correo o Contraseña incorrectas.", null);
            }

            if (usuarioExistente.Estado == 0)
            {
                if (usuarioExistente.FechaDesbaneo != null && usuarioExistente.FechaDesbaneo.Value <= DateTime.Now)
                {
                    usuarioExistente.Estado = 1;
                    usuarioExistente.MotivoBaneo = null;
                    usuarioExistente.FechaDesbaneo = null;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    string fechaMsg = usuarioExistente.FechaDesbaneo != null 
                        ? usuarioExistente.FechaDesbaneo.Value.ToString("dd/MM/yyyy HH:mm") 
                        : "Indefinido";
                    return (false, $"Tu cuenta ha sido suspendida. Motivo: {usuarioExistente.MotivoBaneo ?? "Incumplimiento de las reglas"}. Suspensión activa hasta: {fechaMsg}.", null);
                }
            }

            var returnUsuario = new
            {
                id = usuarioExistente.IdUsuarios,
                nombre = usuarioExistente.Nombre,
                apellido = usuarioExistente.Apellido,
                correo = usuarioExistente.Correo,
                telefono = usuarioExistente.Telefono,
                rol = usuarioExistente.Rol,
                foto = usuarioExistente.ImgPerfil,
                estadoPresencia = usuarioExistente.EstadoPresencia,
                bio = usuarioExistente.Descripcion
            };

            return (true, "Inicio de sesión exitoso.", returnUsuario);
        }

        public async Task<(bool exito, string mensaje)> HacerAdminAsync(string correo)
        {
            if (string.IsNullOrEmpty(correo))
            {
                return (false, "El correo es obligatorio.");
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
            if (usuario == null)
            {
                return (false, "Usuario no encontrado.");
            }

            usuario.Rol = "Administrador";
            await _context.SaveChangesAsync();

            return (true, $"El usuario con correo {correo} ahora es Administrador exitosamente.");
        }

        public async Task<(bool exito, string mensaje, object? perfil)> GetPerfilPublicoAsync(int id)
        {
            var usuario = await _context.Usuarios
                .Where(u => u.IdUsuarios == id)
                .Select(u => new {
                    id = u.IdUsuarios,
                    nombre = u.Nombre,
                    apellido = u.Apellido,
                    foto = u.ImgPerfil,
                    bio = u.Descripcion,
                    calificacion = u.Calificacion,
                    fechaRegistro = u.FechaRegistro,
                    estadoPresencia = u.EstadoPresencia
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return (false, "Usuario no encontrado", null);
            }

            return (true, "", usuario);
        }

        public async Task<(bool exito, string mensaje)> ActualizarPerfilAsync(ActualizarPerfilDto datos)
        {
            var usuario = await _context.Usuarios.FindAsync(datos.UsuarioId);
            if (usuario == null) return (false, "Usuario no encontrado");

            usuario.Nombre = datos.Nombre ?? usuario.Nombre;
            usuario.Apellido = datos.Apellido ?? usuario.Apellido;
            usuario.Telefono = datos.Telefono;
            usuario.ImgPerfil = datos.ImgPerfil;
            usuario.Descripcion = datos.Bio;

            await _context.SaveChangesAsync();

            return (true, "Perfil actualizado correctamente");
        }

        public async Task<(bool exito, string mensaje)> CambiarPasswordAsync(CambiarPasswordDto datos)
        {
            var usuario = await _context.Usuarios.FindAsync(datos.UsuarioId);
            if (usuario == null) return (false, "Usuario no encontrado");

            bool claveCorrecta = BCrypt.Net.BCrypt.Verify(datos.PasswordActual, usuario.Contraseña);
            if (!claveCorrecta)
            {
                return (false, "La contraseña actual es incorrecta.");
            }

            if (!CumplePoliticaPassword(datos.NuevaPassword))
            {
                return (false, PasswordPolicyMessage);
            }

            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(datos.NuevaPassword);
            await _context.SaveChangesAsync();

            return (true, "Contraseña actualizada exitosamente.");
        }
    }
}
